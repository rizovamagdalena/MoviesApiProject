using Microsoft.AspNetCore.Mvc;
using MoviesAPI.Models;
using MoviesAPI.Repositories.Interface;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace MoviesAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TicketsController : ControllerBase
    {
        private readonly ITicketRepository _ticketRepository;

        public TicketsController(ITicketRepository ticketRepository)
        {
            _ticketRepository = ticketRepository;
        }


        // GET: api/<TicketsController> or GET: api/tickets
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TicketResponse>>> Get()
        {
            var tickets = await _ticketRepository.GetTicketsAsync();
            System.Diagnostics.Debug.WriteLine("tickets" + tickets);
            foreach (var ticket in tickets)
            {

                System.Diagnostics.Debug.WriteLine("row" + ticket.Row);
            }

            return Ok(tickets);
        }

        // GET api/<TicketsController>/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Ticket>> Get(long id)
        {
            var ticket = await _ticketRepository.GetTicketAsync(id);

            if (ticket == null)
            {
                return NotFound();
            }

            return Ok(ticket);
        }

        // POST api/<TicketsController>
        [HttpPost]
        public async Task<ActionResult> Post([FromBody] CreateTicket ticket)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var id = await _ticketRepository.CreateTicketAsync(ticket);
            return Ok(id);
        }

        //// PUT api/<TicketsController>/5
        //[HttpPut]
        //public async Task<ActionResult> Put(int id, [FromForm] UpdateTicket ticket)
        //{
        //    var existing = await _ticketRepository.GetTicketForUpdateAsync(id);
        //    if (existing == null)
        //    {
        //        return NotFound();
        //    }

        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest();
        //    }

        //    var result = await _ticketRepository.UpdateTicketAsync(id, ticket);
        //    return Ok(result);
        //}

        // DELETE api/<TicketsController>/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var existing = await _ticketRepository.GetTicketAsync(id);
            if (existing == null)
                return NotFound();

            var result = await _ticketRepository.DeleteTicketAsync(id);
            return Ok(result);
        }
    }
}
