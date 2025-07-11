using Microsoft.AspNetCore.Mvc;

namespace DNASystemBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(new { message = "Server is working!", timestamp = DateTime.Now });
        }
        
        [HttpGet("static-test")]
        public IActionResult StaticTest()
        {
            return Content(@"
<!DOCTYPE html>
<html>
<head>
    <title>Test Page</title>
    <style>
        body { 
            font-family: Arial, sans-serif; 
            text-align: center; 
            padding: 50px;
            background: #f0f8ff;
        }
        .container {
            background: white;
            padding: 30px;
            border-radius: 10px;
            box-shadow: 0 2px 10px rgba(0,0,0,0.1);
            display: inline-block;
        }
        button {
            background: #4285f4;
            color: white;
            border: none;
            padding: 15px 30px;
            border-radius: 5px;
            cursor: pointer;
            font-size: 16px;
            margin: 10px;
        }
        button:hover { background: #3367d6; }
    </style>
</head>
<body>
    <div class='container'>
        <h1>🚀 DNA System - Google Login Test</h1>
        <p>Server is running successfully!</p>
        <a href='http://localhost:5198/api/AuthGoogle/signin-google'>
            <button>🔐 Login with Google</button>
        </a>
        <br>
        <small>Time: " + DateTime.Now + @"</small>
    </div>
</body>
</html>", "text/html");
        }
    }
}
