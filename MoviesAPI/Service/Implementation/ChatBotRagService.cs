using MoviesAPI.Models;
using MoviesAPI.Repositories.Interface;
using MoviesAPI.Service.Interface;
using System.Text;

namespace MoviesAPI.Service.Implementation
{
    public class ChatBotRagService : IChatBotRagService
    {
        private readonly IChatBotRepository _chatBotRepository;
        private readonly IOpenAIService _openAIService;
        private readonly IScreeningService _screeningService;
        private readonly IMovieService _movieService;
        private readonly IHallService _hallService;

        public ChatBotRagService(
            IChatBotRepository chatBotRepository,
            IOpenAIService openAIService,
            IScreeningService screeningService,
            IMovieService movieService,
            IHallService hallService)
        {
            _chatBotRepository = chatBotRepository;
            _openAIService = openAIService;
            _screeningService = screeningService;
            _movieService = movieService;
            _hallService = hallService;
        }

        public async Task<string> AskQuestionAsync(string userQuestion)
        {
            var contextBuilder = new StringBuilder();

            var faqs = await _chatBotRepository.GetAllFaqAsync();
            var matchedFaq = faqs.FirstOrDefault(f =>
                f.Question.ToLower().Contains(userQuestion.ToLower()));

            if (matchedFaq != null)
            {
                contextBuilder.AppendLine($"FAQ: {matchedFaq.Question}");
                contextBuilder.AppendLine($"Answer: {matchedFaq.Answer}");
                contextBuilder.AppendLine();
            }

            DateOnly date = DateOnly.FromDateTime(DateTime.Now);
            if (userQuestion.ToLower().Contains("tomorrow"))
                date = date.AddDays(1);

            var allScreenings = await _screeningService.GetAllAsync();
            var screeningsForDate = allScreenings
                .Where(s => s.Screening_Date_Time.Date == date.ToDateTime(TimeOnly.MinValue).Date)
                .ToList();

            if (screeningsForDate.Any())
            {
                contextBuilder.AppendLine($"🎬 Screenings for {date}:");

                foreach (var screening in screeningsForDate)
                {
                    var movie = await _movieService.GetMovieByIdAsync(screening.Movie_Id);
                    if (movie == null) continue;

                    var hallSeats = await _hallService.GetSeatsByHallIdAsync(screening.Hall_Id);

                    int totalSeats = hallSeats.Count;
                    int availableTickets = screening.Available_Tickets;

                    string ticketInfo = availableTickets > 0
                        ? $"{availableTickets}/{totalSeats} tickets available"
                        : "Sold out";

                    contextBuilder.AppendLine(
                        $"- {movie.Name} at {screening.Screening_Date_Time:HH:mm}, {ticketInfo}");
                }

                contextBuilder.AppendLine();
            }
            else
            {
                contextBuilder.AppendLine($"No screenings found for {date}.");
            }


            if (contextBuilder.Length == 0)
            {
                contextBuilder.AppendLine("No relevant FAQ or movie data found.");
            }

            string prompt = $@"
                You are a friendly cinema assistant.
                Use the context below to answer the user's question as helpfully as possible.

                Context:
                {contextBuilder}

                User Question: {userQuestion}
                Answer:";

            var answer = await _openAIService.AskQuestionAsync(prompt);
            return answer;
        }


    }
}
