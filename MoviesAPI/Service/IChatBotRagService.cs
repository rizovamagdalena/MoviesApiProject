namespace MoviesAPI.Service
{
    public interface IChatBotRagService
    {
        Task<string> AskQuestionAsync(string userQuestion);
    }
}