using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MigraTrackAPI.Data;
using MigraTrackAPI.Models;
using System;
using System.Threading.Tasks;

namespace MigraTrackAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly MigraTrackDbContext _context;

        public AuthController(MigraTrackDbContext context)
        {
            _context = context;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                // Allow empty password if user manually cleared it in DB
                if (string.IsNullOrEmpty(request.Username))
                {
                    return BadRequest("Username is required.");
                }

                var user = await _context.Users
                    .Include(u => u.Permissions)
                    .FirstOrDefaultAsync(u => u.Username == request.Username && (u.PasswordHash == request.Password || (u.PasswordHash == null && string.IsNullOrEmpty(request.Password))));

                if (user == null)
                {
                    return Unauthorized("Invalid cloud credentials.");
                }

                return Ok(new
                {
                    user.UserId,
                    user.Username,
                    user.Role,
                    Permissions = user.Permissions,
                    Token = "mock-jwt-token-" + Guid.NewGuid()
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Login Error: {ex}");
                return StatusCode(500, "Internal Server Error: " + ex.Message);
            }
        }

        [HttpGet("fix-db")]
        public async Task<IActionResult> FixDatabase()
        {
            try
            {
                // SQL to fix the table if Password column is missing
                var sql = @"
                    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Users]') AND name = 'Password')
                    BEGIN
                        ALTER TABLE [dbo].[Users] ADD [Password] NVARCHAR(100) NULL;
                    END
                    
                    -- Also ensure Username exists (just in case)
                    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Users]') AND name = 'Username')
                    BEGIN
                        ALTER TABLE [dbo].[Users] ADD [Username] NVARCHAR(50) NOT NULL DEFAULT 'admin';
                    END

                    -- Reset admin password ensuring it works for empty or 'admin123'
                    IF EXISTS (SELECT * FROM [dbo].[Users] WHERE Username = 'admin')
                    BEGIN
                        UPDATE [dbo].[Users] SET Password = 'admin123' WHERE Username = 'admin';
                    END
                    ELSE
                    BEGIN
                        INSERT INTO [dbo].[Users] (Username, Password, Role) VALUES ('admin', 'admin123', 'Admin');
                    END
                ";

                await _context.Database.ExecuteSqlRawAsync(sql);
                return Ok("Database schema fixed! Password column added and admin user reset.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Failed to fix DB: " + ex.Message);
            }
        }
    }

    public class LoginRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
