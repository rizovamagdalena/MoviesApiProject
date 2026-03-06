using Microsoft.AspNetCore.Mvc;
using MoviesAPI.Models;
using MoviesAPI.Repositories.Interface;
using MoviesAPI.Service.Interface;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace MoviesAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TicketsController : ControllerBase
    {
        private readonly ITicketService _ticketService;

        public TicketsController(ITicketService ticketService)
        {
            _ticketService = ticketService;
        }

        // GET api/tickets
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TicketResponse>>> GetAll()
            => Ok(await _ticketService.GetAllAsync());

        // GET api/tickets/2
        [HttpGet("{id}")]
        public async Task<ActionResult<TicketResponse>> GetById(long id)
        {
            var ticket = await _ticketService.GetByIdAsync(id);
            return ticket is null ? NotFound() : Ok(ticket);
        }

        // DELETE api/tickets/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(long id)
        {
            var existing = await _ticketService.GetByIdAsync(id);
            if (existing is null) return NotFound();

            await _ticketService.DeleteAsync(id);
            return NoContent();
        }
    }
}
