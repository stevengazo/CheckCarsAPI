using CheckCarsAPI.Data;
using CheckCarsAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CheckCarsAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CommentaryController : ControllerBase
{
    private readonly ReportsDbContext dbContext;

    public CommentaryController(ReportsDbContext context)
    {
        dbContext = context;
    }

    // GET: api/Commentary
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Commentary>>> GetCommentaries()
    {
        return await dbContext.commentaries.ToListAsync();
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


    // GET: api/Commentary/5
    [HttpGet("/byreport/{id}")]
    public async Task<ActionResult<List<Commentary>>> GetCommentaryByReport(string id)
    {
        var commentary = await dbContext.commentaries.Where(e => e.ReportId == id).ToListAsync();

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

    private bool CommentaryExists(int id)
    {
        return dbContext.commentaries.Any(e => e.Id == id);
    }
}