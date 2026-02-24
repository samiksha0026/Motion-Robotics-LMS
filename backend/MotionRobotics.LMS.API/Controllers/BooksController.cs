using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MotionRobotics.LMS.API.Data;

namespace MotionRobotics.LMS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class BooksController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public BooksController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Get all available books
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetBooks()
        {
            var books = await _context.Books
                .Select(b => new
                {
                    Id = b.Id,
                    Title = b.Title,
                    Author = b.Author,
                    Description = b.Description
                })
                .ToListAsync();

            return Ok(books);
        }

        /// <summary>
        /// Get book by title (for level-based access)
        /// </summary>
        [HttpGet("by-title/{title}")]
        public async Task<ActionResult<object>> GetBookByTitle(string title)
        {
            var book = await _context.Books
                .Where(b => b.Title.ToLower().Contains(title.ToLower()) ||
                           title.ToLower().Contains(b.Title.ToLower()))
                .Select(b => new
                {
                    Id = b.Id,
                    Title = b.Title,
                    Author = b.Author,
                    Description = b.Description
                })
                .FirstOrDefaultAsync();

            if (book == null)
            {
                return NotFound(new { message = "Book not found" });
            }

            return Ok(book);
        }

        /// <summary>
        /// Serve PDF files from syllabus directory
        /// </summary>
        [HttpGet("syllabus/{fileName}")]
        [AllowAnonymous] // Allow non-authenticated access to serve PDF files
        public IActionResult GetSyllabusFile(string fileName)
        {
            // Security: Validate file extension
            if (!fileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest("Only PDF files are supported");
            }

            // Security: Prevent path traversal attacks
            if (fileName.Contains("..") || fileName.Contains("/") || fileName.Contains("\\"))
            {
                return BadRequest("Invalid file name");
            }

            var filePath = Path.Combine("wwwroot", "syllabus", fileName);

            if (!System.IO.File.Exists(filePath))
            {
                return NotFound(new { message = $"Syllabus file '{fileName}' not found. Please contact administrator." });
            }

            var fileBytes = System.IO.File.ReadAllBytes(filePath);

            // Set proper headers for PDF viewing in browser (prevent download)
            Response.Headers.Append("Content-Type", "application/pdf");
            Response.Headers.Append("Content-Disposition", "inline; filename=\"" + fileName + "\"");
            Response.Headers.Append("Cache-Control", "no-store, no-cache, must-revalidate, max-age=0");
            Response.Headers.Append("Pragma", "no-cache");
            Response.Headers.Append("Expires", "0");
            Response.Headers.Append("X-Content-Type-Options", "nosniff");
            Response.Headers.Append("X-Frame-Options", "SAMEORIGIN");

            return File(fileBytes, "application/pdf", fileName);
        }
    }
}
