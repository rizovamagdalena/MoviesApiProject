using Microsoft.AspNetCore.Mvc;
using MoviesAPI.Models;
using MoviesAPI.Repositories.Interface;
using MoviesAPI.Service;

namespace MoviesAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatBotController : ControllerBase
    {
        private readonly IChatBotRepository _chatbotRepository;
        private readonly IOpenAIService _openAIService;

        public ChatBotController(IChatBotRepository chatbotRepository,IOpenAIService openAIService)
        {
            _chatbotRepository = chatbotRepository;
            _openAIService = openAIService;
        }

        // GET: api/chatbot/faqs
        [HttpGet("faqs")]
        public async Task<ActionResult<List<Faq>>> GetAllFaq()
        {
            var faqs = await _chatbotRepository.GetAllFaqAsync();
            return Ok(faqs);
        }

        // GET: api/chatbot/faqs/{id}
        [HttpGet("faqs/{id}")]
        public async Task<ActionResult<Faq>> GetFaqById(int id)
        {
            var faq = await _chatbotRepository.GetFaqByIdAsync(id);
            if (faq == null) return NotFound();
            return Ok(faq);
        }

        // POST: api/chatbot/faq
        [HttpPost("faq")]
        public async Task<ActionResult> CreateFaq(Faq faq)
        {
            await _chatbotRepository.AddFaqAsync(faq);
            return CreatedAtAction(nameof(GetFaqById), new { id = faq.Id }, faq);
        }

        // PUT: api/chatbot/faq/{id}
        [HttpPut("faq/{id}")]
        public async Task<ActionResult> Update(int id, Faq faq)
        {
            if (id != faq.Id) return BadRequest();
            var existing = await _chatbotRepository.GetFaqByIdAsync(id);
            if (existing == null) return NotFound();

            await _chatbotRepository.UpdateFaqAsync(faq);
            return NoContent();
        }

        // DELETE: api/chatbot/faq/{id}
        [HttpDelete("faq/{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            var existing = await _chatbotRepository.GetFaqByIdAsync(id);
            if (existing == null) return NotFound();

            await _chatbotRepository.DeleteFaqAsync(id);
            return NoContent();
        }

        // POST: api/chatbot/ask
        [HttpPost("ask")]
        public async Task<ActionResult> Ask([FromBody] UserQuestionRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Question))
                return BadRequest(new { answer = "Please ask a question." });

            var answer = await _openAIService.AskQuestionAsync(request.Question);
            return Ok(new { answer });
        }

        public class UserQuestionRequest
        {
            public string Question { get; set; }
        }
    }
}