using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MigraTrackAPI.Data;
using MigraTrackAPI.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MigraTrackAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly MigraTrackDbContext _context;

        public UsersController(MigraTrackDbContext context)
        {
            _context = context;
        }

        // GET: api/Users
        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            // Only return active users by default due to soft delete filter, but good to be explicit
            return await _context.Users
                .Include(u => u.Permissions)
                .OrderBy(u => u.Username)
                .ToListAsync();
        }

        // GET: api/Users/5
        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUser(int id)
        {
            var user = await _context.Users
                .Include(u => u.Permissions)
                .FirstOrDefaultAsync(u => u.UserId == id);

            if (user == null)
            {
                return NotFound();
            }

            return user;
        }

        // POST: api/Users
        [HttpPost]
        public async Task<ActionResult<User>> CreateUser(User user)
        {
            if (await _context.Users.AnyAsync(u => u.Username == user.Username))
            {
                return BadRequest("Username already exists.");
            }

            // Ensure password is set (in a real app, hash this!)
            if (string.IsNullOrEmpty(user.PasswordHash))
            {
                return BadRequest("Password is required.");
            }

            // user.Permissions will be automatically added if present in the payload
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetUser", new { id = user.UserId }, user);
        }

        // PUT: api/Users/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, User user)
        {
            if (id != user.UserId)
            {
                return BadRequest();
            }

            var existingUser = await _context.Users
                .Include(u => u.Permissions)
                .FirstOrDefaultAsync(u => u.UserId == id);

            if (existingUser == null)
            {
                return NotFound();
            }

            // Update basic fields
            existingUser.FirstName = user.FirstName;
            existingUser.LastName = user.LastName;
            existingUser.Email = user.Email;
            existingUser.Role = user.Role;
            existingUser.IsActive = user.IsActive;

            // Only update password if provided
            if (!string.IsNullOrEmpty(user.PasswordHash))
            {
                existingUser.PasswordHash = user.PasswordHash;
            }

            // Update Permissions
            // 1. Remove existing permissions not in the new list (or remove all and re-add)
            // Strategy: Remove all and re-add is simplest for full update
            _context.UserPermissions.RemoveRange(existingUser.Permissions);
            
            // 2. Add new permissions
            if (user.Permissions != null)
            {
                foreach (var perm in user.Permissions)
                {
                    perm.UserId = id; // Ensure ID linkage
                    _context.UserPermissions.Add(perm);
                }
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/Users/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            // Soft delete
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.UserId == id);
        }
    }
}
