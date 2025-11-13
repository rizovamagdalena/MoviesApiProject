using MoviesAPI.Repositories.Interface;
using System.Text;

namespace MoviesAPI.Service
{
    public class ChatBotRagService : IChatBotRagService
    {
        private readonly IChatBotRepository _chatBotRepository;
        private readonly IOpenAIService _openAIService;
        private readonly IScreeningRepository _screeningRepository;
        private readonly IMovieRepository _movieRepository;
        private readonly ITicketRepository _ticketRepository;
        private readonly IHallRepository _hallRepository;

        public ChatBotRagService(
            IChatBotRepository chatBotRepository,
            IOpenAIService openAIService,
            IScreeningRepository screeningRepository,
            IMovieRepository movieRepository,
            ITicketRepository ticketRepository,
            IHallRepository hallRepository)
        {
            _chatBotRepository = chatBotRepository;
            _openAIService = openAIService;
            _screeningRepository = screeningRepository;
            _movieRepository = movieRepository;
            _ticketRepository = ticketRepository;
            _hallRepository = hallRepository;   
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

            var screenings = await _screeningRepository.GetScreeningsAsync();

            if (screenings != null && screenings.Any())
            {
                contextBuilder.AppendLine($"🎬 Screenings for {date}:");

                foreach (var s in screenings.Where(s =>
                    s.Screening_Date_Time.Date == date.ToDateTime(TimeOnly.MinValue).Date))
                {
                    var movie = await _movieRepository.GetMovieAsync(s.Movie_Id);
                    if (movie == null) continue;

                    var hallSeats = await _hallRepository.GetSeatsByHallIdAsync(s.Hall_Id);

                    int purchasedTickets = await _ticketRepository.GetPurchasedTicketsAsync(
                        s.Movie_Id, s.Screening_Date_Time
                    );

                    int availableTickets = hallSeats.Count - purchasedTickets;

                    string ticketInfo = availableTickets > 0
                        ? $"{availableTickets}/{hallSeats.Count} tickets available"
                        : "Sold out";

                    contextBuilder.AppendLine(
                        $"- {movie.Name} at {s.Screening_Date_Time:HH:mm}, {ticketInfo}"
                    );
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


        //public async Task<string> AskQuestionAsync(string userQuestion)
        //{
        //    var contextBuilder = new StringBuilder();

        //    var faqs = await _chatBotRepository.GetAllFaqAsync();
        //    var matchedFaq = faqs.FirstOrDefault(f =>
        //        f.Question.ToLower().Contains(userQuestion.ToLower()));

        //    if (matchedFaq != null)
        //    {
        //        contextBuilder.AppendLine($"FAQ: {matchedFaq.Question}");
        //        contextBuilder.AppendLine($"Answer: {matchedFaq.Answer}");
        //        contextBuilder.AppendLine();
        //    }

        //    DateOnly date = DateOnly.FromDateTime(DateTime.Now);
        //    if (userQuestion.ToLower().Contains("tomorrow"))
        //        date = date.AddDays(1);

        //    var screenings = await _screeningRepository.GetScreeningsAsync();

        //    if (screenings != null && screenings.Any())
        //    {
        //        contextBuilder.AppendLine($"🎬 Screenings for {date}:");
        //        foreach (var s in screenings)
        //        {
        //            var movie = await _movieRepository.GetMovieAsync(s.Movie_Id);
        //            if (movie == null) continue;

        //            int availableTickets = 0;
        //            try
        //            {
        //                availableTickets = await _ticketRepository.GetPurchasedTicketsForChatbotAsync(
        //                    s.Movie_Id,
        //                    s.Screening_Date_Time
        //                );
        //            }
        //            catch
        //            {
        //            }

        //            contextBuilder.AppendLine(
        //                $"- {movie.Name} at {s.Screening_Date_Time:HH:mm}, " +
        //                $"tickets available: {availableTickets}");
        //        }
        //        contextBuilder.AppendLine();
        //    }
        //    else
        //    {
        //        contextBuilder.AppendLine($"No screenings found for {date}.");
        //    }

        //    if (contextBuilder.Length == 0)
        //    {
        //        contextBuilder.AppendLine("No relevant FAQ or movie data found.");
        //    }

        //    string prompt = $@"
        //        You are a friendly cinema assistant.
        //        Use the context below to answer the user's question as helpfully as possible.

        //        Context:
        //        {contextBuilder}

        //        User Question: {userQuestion}
        //        Answer:";

        //    var answer = await _openAIService.AskQuestionAsync(prompt);
        //    return answer;
        //}
    }
}
