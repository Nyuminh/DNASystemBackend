using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;
using DNASystemBackend.Interfaces;
using DNASystemBackend.Models;
using DNASystemBackend.Repositories;
using DNASystemBackend.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using QuestPDF.Infrastructure;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(x =>
    {
        x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        x.JsonSerializerOptions.WriteIndented = true;
        x.JsonSerializerOptions.Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IServiceService, ServiceService>();
builder.Services.AddScoped<IServiceRepository, ServiceRepository>();
builder.Services.AddScoped<IAppointmentRepository, AppointmentRepository>();
builder.Services.AddScoped<IAppointmentService, AppointmentService>();
builder.Services.AddScoped<IKitRepository, KitRepository>();
builder.Services.AddScoped<IKitService, KitService>();
builder.Services.AddScoped<ITestResultRepository, TestResultRepository>();
builder.Services.AddScoped<ITestResultService, TestResultService>();
builder.Services.AddScoped<IFeedbackRepository, FeedbackRepository>();
builder.Services.AddScoped<IFeedbackService, FeedbackService>();
builder.Services.AddScoped<IKitRepository, KitRepository>();
builder.Services.AddScoped<IKitService, KitService>();
builder.Services.AddScoped<ICourseService, CourseService>();
builder.Services.AddScoped<ICourseRepository, CourseRepository>();
builder.Services.AddScoped<IInvoiceService, InvoiceService>();
builder.Services.AddScoped<IInvoiceDetailService, InvoiceDetailService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IRelativeRepository, RelativeRepository>();
builder.Services.AddScoped<IRelativeService, RelativeService>();


builder.Services.AddLogging(logging =>
{
    logging.AddConsole();
    logging.AddDebug();
    logging.SetMinimumLevel(LogLevel.Debug);
});

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultSignInScheme = "Cookies";
})
.AddCookie("Cookies", options =>
{
    options.LoginPath = "/api/AuthGoogle/signin-google";
    options.LogoutPath = "/api/AuthGoogle/signout";
    options.Cookie.Name = "GoogleAuth";
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.IsEssential = true;
    options.ExpireTimeSpan = TimeSpan.FromHours(1);
    options.SlidingExpiration = true;
})
.AddJwtBearer(options =>
{
    var jwtSettings = builder.Configuration.GetSection("Jwt");
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"] ?? throw new InvalidOperationException("JWT Key not configured")))
    };
    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            Console.WriteLine("OnAuthenticationFailed: " + context.Exception.Message);
            return Task.CompletedTask;
        },
        OnTokenValidated = context =>
        {
            Console.WriteLine("OnTokenValidated: " + context.SecurityToken);
            return Task.CompletedTask;
        }
    };
})
.AddGoogle(options =>
{
    var googleSettings = builder.Configuration.GetSection("Authentication:Google");
    options.ClientId = googleSettings["ClientId"] ?? throw new InvalidOperationException("Google ClientId not configured");
    options.ClientSecret = googleSettings["ClientSecret"] ?? throw new InvalidOperationException("Google ClientSecret not configured");
    
   
    options.Scope.Clear();
    options.Scope.Add("openid");
    options.Scope.Add("profile");
    options.Scope.Add("email");
    
    options.SaveTokens = true;
    options.SignInScheme = "Cookies";
    
    // Add callback path explicitly
    options.CallbackPath = new PathString("/api/AuthGoogle/google-callback");
    
    // Configure cookies for better compatibility
    options.CorrelationCookie.Name = "GoogleCorrelation";
    options.CorrelationCookie.HttpOnly = true;
    options.CorrelationCookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    options.CorrelationCookie.SameSite = SameSiteMode.Lax;
    options.CorrelationCookie.IsEssential = true;
    
    // Add events for debugging
    options.Events.OnCreatingTicket = context =>
    {
        Console.WriteLine($"Google OnCreatingTicket: {context.Principal?.Identity?.Name}");
        return Task.CompletedTask;
    };
    
    options.Events.OnRemoteFailure = context =>
    {
        Console.WriteLine($"Google OnRemoteFailure: {context.Failure?.Message}");
        
        // Redirect to error page with details
        context.Response.Redirect($"/error-handler.html?error={Uri.EscapeDataString(context.Failure?.Message ?? "Unknown error")}");
        context.HandleResponse();
        return Task.CompletedTask;
    };
    
    options.Events.OnAccessDenied = context =>
    {
        Console.WriteLine("Google OnAccessDenied");
        context.Response.Redirect("/error-handler.html?error=access_denied");
        context.HandleResponse();
        return Task.CompletedTask;
    };
    
    options.Events.OnTicketReceived = context =>
    {
        Console.WriteLine("Google OnTicketReceived - Success!");
        
        // Extract user information from the successful authentication
        var claims = context.Principal?.Identity as ClaimsIdentity;
        var email = claims?.FindFirst(ClaimTypes.Email)?.Value;
        var name = claims?.FindFirst(ClaimTypes.Name)?.Value;
        var googleId = claims?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        Console.WriteLine($"Authenticated user - Email: {email}, Name: {name}");
        
        // Store the user info in session to pass to our callback
        context.HttpContext.Session.SetString("GoogleEmail", email ?? "");
        context.HttpContext.Session.SetString("GoogleName", name ?? "");
        context.HttpContext.Session.SetString("GoogleId", googleId ?? "");
        
        // Redirect to our custom success handler instead of continuing with OAuth
        context.Response.Redirect("/api/AuthGoogle/success-callback");
        context.HandleResponse(); // This prevents further OAuth processing
        
        return Task.CompletedTask;
    };
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdminRole", policy => policy.RequireRole("Admin"));
});

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "DNASystem API", Version = "v1" });

 
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter 'Bearer' [space] and then your valid token."
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 104857600; 
});
builder.Services.AddDistributedMemoryCache();
builder.Services.AddDataProtection();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    options.Cookie.SameSite = SameSiteMode.Lax;
    }
);
builder.Services.AddDbContext<DnasystemContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DNASystemDb")));
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
       policy => policy.AllowAnyOrigin()
                       .AllowAnyMethod()
                       .AllowAnyHeader());
});

QuestPDF.Settings.License = LicenseType.Community;
var app = builder.Build();
app.UseCors("AllowAll");
app.UseStaticFiles();
app.Use(async (context, next) =>
{
    if (context.Request.ContentType != null &&
        context.Request.ContentType.StartsWith("application/json", StringComparison.OrdinalIgnoreCase) &&
        !context.Request.ContentType.Contains("charset"))
    {
        context.Request.ContentType = "application/json; charset=utf-8";
    }

    await next();
});
app.UseSession();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseAuthentication(); 
app.UseAuthorization();
app.MapControllers();
app.Run();