using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using DNASystemBackend.Interfaces;
using DNASystemBackend.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

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
