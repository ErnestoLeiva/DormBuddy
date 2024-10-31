using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DormBuddy.Models;
using System.Threading.Tasks;

namespace DormBuddy.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GroupsController : ControllerBase
    {
        private readonly DBContext _context;

        public GroupsController(DBContext context)
        {
            _context = context;
        }

        // GET: api/groups
        [HttpGet]
        public async Task<IActionResult> GetGroups()
        {
            var groups = await _context.Groups
                .Include(g => g.Users) // If you need members' details in the response
                .ToListAsync();

            return Ok(groups);
        }

        // Optionally, GET: api/groups/{id} to fetch a single group by ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetGroup(int id)
        {
            var group = await _context.Groups
                .Include(g => g.Users)
                .FirstOrDefaultAsync(g => g.Id == id);

            if (group == null)
            {
                return NotFound();
            }

            return Ok(group);
        }

        [HttpPost]
    public async Task<IActionResult> CreateGroup([FromBody] GroupModel group)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        _context.Groups.Add(group);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetGroup), new { id = group.Id }, group);
    }

    }
}
