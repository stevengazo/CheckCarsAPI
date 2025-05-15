using CheckCarsAPI.Data;
using CheckCarsAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CheckCarsAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Admin,Manager,User,Guest")]
public class CommentaryController : ControllerBase
{
    #region Properties
    private readonly ReportsDbContext dbContext;

    #endregion

    #region Constructor
    public CommentaryController(ReportsDbContext context)
    {
        dbContext = context;
    }
    #endregion

    #region Endpoints
    // GET: api/Commentary
    /*  
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Commentary>>> GetCommentaries()
    {
        return await dbContext.commentaries.ToListAsync();
    }
    */

    // GET: api/Commentary
    [HttpGet("ByReport/{id}")]
    public async Task<ActionResult<IEnumerable<Commentary>>> GetByReport(string id)
    {
        return await dbContext.commentaries
            .Include(e => e.Report)
            .Where(e => e.ReportId == id)
            .ToListAsync();
    }


    // GET: api/Commentary/5
    [HttpGet("{id}")]
    public async Task<ActionResult<Commentary>> GetCommentary(int id)
    {
        var commentary = await dbContext.commentaries.FindAsync(id);

        if (commentary == null)
        {
            return NotFound();
        }

        return commentary;
    }

    // POST: api/Commentary
    [HttpPost]
    public async Task<ActionResult<Commentary>> PostCommentary(Commentary commentary)
    {
        dbContext.commentaries.Add(commentary);
        await dbContext.SaveChangesAsync();

        return CreatedAtAction(nameof(GetCommentary), new { id = commentary.Id }, commentary);
    }

    // PUT: api/Commentary/5
    [HttpPut("{id}")]
    public async Task<IActionResult> PutCommentary(int id, Commentary commentary)
    {
        if (id != commentary.Id)
        {
            return BadRequest();
        }

        dbContext.Entry(commentary).State = EntityState.Modified;

        try
        {
            await dbContext.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!CommentaryExists(id))
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

    // DELETE: api/Commentary/5
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> DeleteCommentary(int id)
    {
        var commentary = await dbContext.commentaries.FindAsync(id);
        if (commentary == null)
        {
            return NotFound();
        }

        dbContext.commentaries.Remove(commentary);
        await dbContext.SaveChangesAsync();

        return NoContent();
    }

    #endregion

    #region Private Methods
    private bool CommentaryExists(int id)
    {
        return dbContext.commentaries.Any(e => e.Id == id);
    }
    #endregion
}