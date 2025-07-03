using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using DNASystemBackend.Interfaces;
using DNASystemBackend.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Text.Json;

namespace DNASystemBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthGoogleController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IConfiguration _configuration;

        public AuthGoogleController(IUserService userService, IConfiguration configuration)
        {
            _userService = userService;
            _configuration = configuration;
        }

        [HttpGet("signin-google")]
        public IActionResult SignInGoogle()
        {
            try
            {
                Console.WriteLine("SignInGoogle endpoint called");
                var redirectUrl = "http://localhost:5198/api/AuthGoogle/google-callback";
                Console.WriteLine($"Redirect URL: {redirectUrl}");
                
                var properties = new AuthenticationProperties 
                { 
                    RedirectUri = redirectUrl,
                    AllowRefresh = true,
                    IsPersistent = false
                };
                
                // Add custom state to help with debugging
                properties.Items["login_hint"] = "google_oauth";
                
                Console.WriteLine("Challenging with Google authentication scheme");
                return Challenge(properties, GoogleDefaults.AuthenticationScheme);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in SignInGoogle: {ex.Message}");
                return BadRequest($"Error initiating Google authentication: {ex.Message}");
            }
        }

        [HttpGet("google-callback")]
        public async Task<IActionResult> GoogleCallback()
        {
            try
            {
                Console.WriteLine("GoogleCallback endpoint called");
                
                var result = await HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);
                Console.WriteLine($"Authentication result succeeded: {result.Succeeded}");
                
                if (!result.Succeeded)
                {
                    Console.WriteLine($"Authentication failed: {result.Failure?.Message}");
                    return BadRequest($"Google authentication failed: {result.Failure?.Message}");
                }

                var claims = result.Principal.Identities.FirstOrDefault()?.Claims;
                var email = claims?.FirstOrDefault(x => x.Type == ClaimTypes.Email)?.Value;
                var name = claims?.FirstOrDefault(x => x.Type == ClaimTypes.Name)?.Value;
                var googleId = claims?.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;

                Console.WriteLine($"Google user email: {email}");
                Console.WriteLine($"Google user name: {name}");
                Console.WriteLine($"Google user ID: {googleId}");

                if (string.IsNullOrEmpty(email))
                {
                    Console.WriteLine("No email found in Google claims");
                    return BadRequest("Unable to get email from Google");
                }

            // Check if user exists
            var existingUser = await _userService.GetUserByEmailAsync(email);
            
            if (existingUser == null)
            {
                Console.WriteLine("Creating new user");
                // Create new user
                var newUser = new User
                {
                    Email = email,
                    Fullname = name ?? email,
                    Username = !string.IsNullOrEmpty(googleId) 
                        ? $"google_{googleId.Substring(0, Math.Min(8, googleId.Length))}"
                        : $"google_{Guid.NewGuid().ToString().Substring(0, 8)}", // Short username from Google ID
                    Password = "GOOGLE_AUTH", // Simple password for Google OAuth users
                    // Let UserService set the default role automatically
                };

                var (success, message) = await _userService.CreateAsync(newUser);
                if (!success)
                {
                    Console.WriteLine($"Failed to create user: {message}");
                    return BadRequest($"Failed to create user: {message}");
                }

                existingUser = await _userService.GetUserByEmailAsync(email);
            }

            // Generate JWT token
            if (existingUser == null)
                return BadRequest("Failed to retrieve user after creation");
                
            var token = GenerateJwtToken(existingUser);

            // Return success page with token embedded
            var html = $@"
<!DOCTYPE html>
<html>
<head>
    <title>Login Success</title>
    <style>
        body {{ font-family: Arial, sans-serif; text-align: center; padding: 50px; background: #f0f8ff; }}
        .container {{ background: white; padding: 30px; border-radius: 10px; box-shadow: 0 2px 10px rgba(0,0,0,0.1); display: inline-block; }}
        .success {{ color: #28a745; }}
        .token {{ background: #f8f9fa; padding: 15px; border-radius: 5px; word-break: break-all; margin: 20px 0; }}
    </style>
</head>
<body>
    <div class='container'>
        <h1 class='success'>🎉 Google Login Successful!</h1>
        <p><strong>Email:</strong> {existingUser.Email}</p>
        <p><strong>Name:</strong> {existingUser.Fullname}</p>
        <p><strong>User ID:</strong> {existingUser.UserId}</p>
        <div class='token'>
            <h3>JWT Token:</h3>
            <p>{token}</p>
        </div>
        <button onclick=""navigator.clipboard.writeText('{token}'); alert('Token copied!')"">Copy Token</button>
    </div>
    <script>
        // Store token in localStorage
        localStorage.setItem('authToken', '{token}');
        console.log('Token stored in localStorage');
    </script>
</body>
</html>";
            
            return Content(html, "text/html");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GoogleCallback: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return BadRequest($"Authentication error: {ex.Message}");
            }
        }

        [HttpGet("test-config")]
        public IActionResult TestConfig()
        {
            try
            {
                var googleSettings = _configuration.GetSection("Authentication:Google");
                var clientId = googleSettings["ClientId"];
                var clientSecret = googleSettings["ClientSecret"];
                
                return Ok(new
                {
                    HasClientId = !string.IsNullOrEmpty(clientId),
                    ClientIdLength = clientId?.Length ?? 0,
                    HasClientSecret = !string.IsNullOrEmpty(clientSecret),
                    ClientSecretLength = clientSecret?.Length ?? 0,
                    CallbackUrl = "http://localhost:5198/api/AuthGoogle/google-callback"
                });
            }
            catch (Exception ex)
            {
                return BadRequest($"Configuration error: {ex.Message}");
            }
        }

        [HttpGet("diagnose")]
        public IActionResult Diagnose()
        {
            try
            {
                var googleSettings = _configuration.GetSection("Authentication:Google");
                var clientId = googleSettings["ClientId"];
                var clientSecret = googleSettings["ClientSecret"];
                
                var diagnosticInfo = new
                {
                    GoogleAuth = new
                    {
                        ClientId = clientId?.Substring(0, Math.Min(20, clientId.Length)) + "...",
                        HasClientSecret = !string.IsNullOrEmpty(clientSecret),
                        RedirectUri = "http://localhost:5198/api/AuthGoogle/google-callback",
                        SignInEndpoint = "http://localhost:5198/api/AuthGoogle/signin-google"
                    },
                    Instructions = new
                    {
                        Step1 = "Verify in Google Cloud Console that the OAuth 2.0 Client ID has the redirect URI: http://localhost:5198/api/AuthGoogle/google-callback",
                        Step2 = "Make sure the OAuth consent screen is configured and your test user is added",
                        Step3 = "Ensure your application is in 'Testing' mode if using a restricted OAuth consent screen",
                        Step4 = "Try the manual test URL below"
                    },
                    TestUrls = new
                    {
                        ManualSignIn = "http://localhost:5198/api/AuthGoogle/signin-google",
                        SimpleTest = "http://localhost:5198/simple-test.html",
                        ConfigCheck = "http://localhost:5198/api/AuthGoogle/test-config"
                    }
                };
                
                return Ok(diagnosticInfo);
            }
            catch (Exception ex)
            {
                return BadRequest($"Diagnostic error: {ex.Message}");
            }
        }

        [HttpGet("auth-error")]
        public IActionResult AuthError(string? error = null, string? error_description = null)
        {
            Console.WriteLine($"Auth error endpoint called: {error} - {error_description}");
            
            var html = $@"
<!DOCTYPE html>
<html>
<head>
    <title>Authentication Error</title>
    <style>
        body {{ font-family: Arial, sans-serif; text-align: center; padding: 50px; background: #fee; }}
        .container {{ background: white; padding: 30px; border-radius: 10px; box-shadow: 0 2px 10px rgba(0,0,0,0.1); display: inline-block; }}
        .error {{ color: #dc3545; }}
    </style>
</head>
<body>
    <div class='container'>
        <h1 class='error'>🚫 Authentication Failed</h1>
        <p><strong>Error:</strong> {error ?? "Unknown error"}</p>
        <p><strong>Description:</strong> {error_description ?? "No description provided"}</p>
        <button onclick=""window.location.href='/simple-test.html'"">Try Again</button>
        <button onclick=""window.location.href='/api/AuthGoogle/diagnose'"">View Diagnostics</button>
    </div>
</body>
</html>";
            
            return Content(html, "text/html");
        }

        [HttpGet("debug-redirect-uri")]
        public IActionResult DebugRedirectUri()
        {
            var redirectUrl = Url.Action("GoogleCallback", "AuthGoogle", null, Request.Scheme);
            var hardcodedUrl = "http://localhost:5198/api/AuthGoogle/google-callback";
            
            return Ok(new
            {
                GeneratedRedirectUri = redirectUrl,
                HardcodedRedirectUri = hardcodedUrl,
                CurrentScheme = Request.Scheme,
                CurrentHost = Request.Host.ToString(),
                Instructions = new
                {
                    Step1 = "Copy the EXACT RedirectUri shown above",
                    Step2 = "Go to Google Cloud Console > APIs & Services > Credentials",
                    Step3 = "Edit your OAuth 2.0 Client ID",
                    Step4 = "Add the EXACT URI to 'Authorized redirect URIs'",
                    Step5 = "Save and try again"
                }
            });
        }

        [HttpGet("manual-callback")]
        public async Task<IActionResult> ManualCallback(string? code = null, string? error = null)
        {
            try
            {
                Console.WriteLine($"ManualCallback called");
                Console.WriteLine($"Code: {code}");
                Console.WriteLine($"Error: {error}");
                Console.WriteLine($"Query string: {Request.QueryString}");
                
                if (!string.IsNullOrEmpty(error))
                {
                    Console.WriteLine($"Error in manual callback: {error}");
                    return BadRequest($"Google authentication error: {error}");
                }
                
                if (string.IsNullOrEmpty(code))
                {
                    Console.WriteLine("No authorization code received");
                    return BadRequest("No authorization code received");
                }

                // Exchange the authorization code for tokens manually
                var googleSettings = _configuration.GetSection("Authentication:Google");
                var clientId = googleSettings["ClientId"];
                var clientSecret = googleSettings["ClientSecret"];
                var redirectUri = "http://localhost:5198/api/AuthGoogle/google-callback";

                using var httpClient = new HttpClient();
                var tokenRequest = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("code", code),
                    new KeyValuePair<string, string>("client_id", clientId ?? ""),
                    new KeyValuePair<string, string>("client_secret", clientSecret ?? ""),
                    new KeyValuePair<string, string>("redirect_uri", redirectUri),
                    new KeyValuePair<string, string>("grant_type", "authorization_code")
                });

                var tokenResponse = await httpClient.PostAsync("https://oauth2.googleapis.com/token", tokenRequest);
                var tokenContent = await tokenResponse.Content.ReadAsStringAsync();
                
                Console.WriteLine($"Token response: {tokenContent}");

                if (!tokenResponse.IsSuccessStatusCode)
                {
                    return BadRequest($"Failed to exchange code for tokens: {tokenContent}");
                }

                // Parse the token response
                using var tokenDoc = JsonDocument.Parse(tokenContent);
                var accessToken = tokenDoc.RootElement.GetProperty("access_token").GetString();

                if (string.IsNullOrEmpty(accessToken))
                {
                    return BadRequest("No access token received");
                }

                // Get user info from Google
                httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
                var userInfoResponse = await httpClient.GetAsync("https://www.googleapis.com/oauth2/v2/userinfo");
                var userInfoContent = await userInfoResponse.Content.ReadAsStringAsync();
                
                Console.WriteLine($"User info response: {userInfoContent}");

                if (!userInfoResponse.IsSuccessStatusCode)
                {
                    return BadRequest($"Failed to get user info: {userInfoContent}");
                }

                using var userInfoDoc = JsonDocument.Parse(userInfoContent);
                var email = userInfoDoc.RootElement.GetProperty("email").GetString();
                var name = userInfoDoc.RootElement.TryGetProperty("name", out var nameElement) ? nameElement.GetString() : null;

                Console.WriteLine($"Manual callback - Email: {email}, Name: {name}");

                if (string.IsNullOrEmpty(email))
                {
                    return BadRequest("Unable to get email from Google user info");
                }

                // Check if user exists
                var existingUser = await _userService.GetUserByEmailAsync(email);
                
                if (existingUser == null)
                {
                    Console.WriteLine("Creating new user");
                    // Create new user with Google ID if available, otherwise generate one
                    var userId = userInfoDoc.RootElement.TryGetProperty("id", out var idElement) ? idElement.GetString() : null;
                    var shortUsername = !string.IsNullOrEmpty(userId) 
                        ? $"google_{userId.Substring(0, Math.Min(8, userId.Length))}"
                        : $"google_{Guid.NewGuid().ToString().Substring(0, 8)}";
                        
                    var newUser = new User
                    {
                        Email = email,
                        Fullname = name ?? email,
                        Username = shortUsername,
                        Password = "GOOGLE_AUTH", // Simple password for Google OAuth users
                        // Let UserService set the default role automatically
                    };

                    var (success, message) = await _userService.CreateAsync(newUser);
                    if (!success)
                    {
                        Console.WriteLine($"Failed to create user: {message}");
                        return BadRequest($"Failed to create user: {message}");
                    }

                    existingUser = await _userService.GetUserByEmailAsync(email);
                }

                // Generate JWT token
                if (existingUser == null)
                    return BadRequest("Failed to retrieve user after creation");
                    
                var token = GenerateJwtToken(existingUser);

                // Return success page with token embedded
                var html = $@"
<!DOCTYPE html>
<html>
<head>
    <title>Login Success</title>
    <style>
        body {{ font-family: Arial, sans-serif; text-align: center; padding: 50px; background: #f0f8ff; }}
        .container {{ background: white; padding: 30px; border-radius: 10px; box-shadow: 0 2px 10px rgba(0,0,0,0.1); display: inline-block; }}
        .success {{ color: #28a745; }}
        .token {{ background: #f8f9fa; padding: 15px; border-radius: 5px; word-break: break-all; margin: 20px 0; }}
    </style>
</head>
<body>
    <div class='container'>
        <h1 class='success'>🎉 Google Login Successful!</h1>
        <p><strong>Method:</strong> Manual OAuth Code Exchange</p>
        <p><strong>Email:</strong> {existingUser.Email}</p>
        <p><strong>Name:</strong> {existingUser.Fullname}</p>
        <p><strong>User ID:</strong> {existingUser.UserId}</p>
        <div class='token'>
            <h3>JWT Token:</h3>
            <p>{token}</p>
        </div>
        <button onclick=""navigator.clipboard.writeText('{token}'); alert('Token copied!')"">Copy Token</button>
    </div>
    <script>
        // Store token in localStorage
        localStorage.setItem('authToken', '{token}');
        console.log('Token stored in localStorage');
    </script>
</body>
</html>";
                
                return Content(html, "text/html");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in ManualCallback: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return BadRequest($"Manual callback error: {ex.Message}");
            }
        }

        [HttpGet("success-callback")]
        public async Task<IActionResult> SuccessCallback()
        {
            try
            {
                Console.WriteLine("SuccessCallback called");
                
                // Retrieve user info from session
                var email = HttpContext.Session.GetString("GoogleEmail");
                var name = HttpContext.Session.GetString("GoogleName");
                var googleId = HttpContext.Session.GetString("GoogleId");
                
                Console.WriteLine($"Session data - Email: {email}, Name: {name}, GoogleId: {googleId}");
                
                if (string.IsNullOrEmpty(email))
                {
                    Console.WriteLine("No email found in session");
                    return BadRequest("No user information found in session");
                }

                // Check if user exists
                var existingUser = await _userService.GetUserByEmailAsync(email);
                
                if (existingUser == null)
                {
                    Console.WriteLine("Creating new user");
                    // Create new user
                    var newUser = new User
                    {
                        Email = email,
                        Fullname = name ?? email,
                        Username = !string.IsNullOrEmpty(googleId) 
                            ? $"google_{googleId.Substring(0, Math.Min(8, googleId.Length))}"
                            : $"google_{Guid.NewGuid().ToString().Substring(0, 8)}", // Short username from Google ID
                        Password = "GOOGLE_AUTH", // Simple password for Google OAuth users
                        // Let UserService set the default role automatically
                    };

                    var (success, message) = await _userService.CreateAsync(newUser);
                    if (!success)
                    {
                        Console.WriteLine($"Failed to create user: {message}");
                        return BadRequest($"Failed to create user: {message}");
                    }

                    existingUser = await _userService.GetUserByEmailAsync(email);
                }
                else
                {
                    Console.WriteLine("User already exists");
                }

                // Generate JWT token
                if (existingUser == null)
                {
                    Console.WriteLine("Failed to retrieve user after creation");
                    return BadRequest("Failed to retrieve user after creation");
                }
                    
                var token = GenerateJwtToken(existingUser);
                Console.WriteLine($"Generated JWT token for user: {existingUser.UserId}");

                // Clear session data
                HttpContext.Session.Remove("GoogleEmail");
                HttpContext.Session.Remove("GoogleName");
                HttpContext.Session.Remove("GoogleId");

                // Return success page with token embedded
                var html = $@"
<!DOCTYPE html>
<html>
<head>
    <title>Login Success</title>
    <style>
        body {{ font-family: Arial, sans-serif; text-align: center; padding: 50px; background: #f0f8ff; }}
        .container {{ background: white; padding: 30px; border-radius: 10px; box-shadow: 0 2px 10px rgba(0,0,0,0.1); display: inline-block; }}
        .success {{ color: #28a745; }}
        .token {{ background: #f8f9fa; padding: 15px; border-radius: 5px; word-break: break-all; margin: 20px 0; }}
    </style>
</head>
<body>
    <div class='container'>
        <h1 class='success'>🎉 Google Login Successful!</h1>
        <p><strong>Method:</strong> Session-based OAuth Success</p>
        <p><strong>Email:</strong> {existingUser.Email}</p>
        <p><strong>Name:</strong> {existingUser.Fullname}</p>
        <p><strong>User ID:</strong> {existingUser.UserId}</p>
        <div class='token'>
            <h3>JWT Token:</h3>
            <p>{token}</p>
        </div>
        <button onclick=""navigator.clipboard.writeText('{token}'); alert('Token copied!')"">Copy Token</button>
        <button onclick=""window.close()"">Close Window</button>
    </div>
    <script>
        // Store token in localStorage
        localStorage.setItem('authToken', '{token}');
        console.log('Token stored in localStorage');
        
        // If this is a popup window, notify parent
        if (window.opener) {{
            window.opener.postMessage({{
                type: 'GOOGLE_AUTH_SUCCESS',
                token: '{token}',
                user: {{
                    email: '{existingUser.Email}',
                    name: '{existingUser.Fullname}',
                    userId: '{existingUser.UserId}'
                }}
            }}, '*');
        }}
    </script>
</body>
</html>";
                
                return Content(html, "text/html");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in SuccessCallback: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return BadRequest($"Success callback error: {ex.Message}");
            }
        }

        private string GenerateJwtToken(User user)
        {
            var jwtSettings = _configuration.GetSection("Jwt");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"] ?? throw new InvalidOperationException("JWT Key not configured")));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId ?? ""),
                new Claim(ClaimTypes.Email, user.Email ?? ""),
                new Claim(ClaimTypes.Name, user.Fullname ?? ""),
                new Claim(ClaimTypes.Role, user.Role?.Rolename ?? "Customer")
            };

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(24),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
