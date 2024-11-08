using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using DormBuddy.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Collections.Generic;


namespace DormBuddy.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]  // Ensure this is added to require "Admin" role for all actions
    public class GroupsController : ControllerBase  // Inherit from ControllerBase for API controllers
    {
        private readonly DBContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public GroupsController(DBContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: api/groups
        [HttpGet]
        public async Task<IActionResult> GetGroups()
        {
            var groups = await _context.Groups
                .Include(g => g.Members) 
                .ToListAsync();

            return Ok(groups);
        }

        // GET: api/groups/{id} to fetch a single group by ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetGroup(int id)
        {
            var group = await _context.Groups
                .Include(g => g.Members)
                .FirstOrDefaultAsync(g => g.Id == id);

            if (group == null)
            {
                return NotFound();
            }

            return Ok(group);
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> CreateGroup([FromBody] GroupModel group)
        {
            if (group == null)
            {
                return BadRequest("Group data is required.");
            }

            // Ensure Members is initialized (empty or null)
            if (group.Members == null)
            {
                group.Members = new List<ApplicationUser>();
            }

            // Assign some default members, or leave it empty
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser != null)
            {
                group.Members.Add(currentUser);
            }

            try
            {
                // Save the new group with members
                _context.Groups.Add(group);
                await _context.SaveChangesAsync();

                return Ok(group);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
