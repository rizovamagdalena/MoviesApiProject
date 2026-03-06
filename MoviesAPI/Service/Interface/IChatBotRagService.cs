namespace MoviesAPI.Service.Interface
{
    public interface IChatBotRagService
    {
        Task<string> AskQuestionAsync(string userQuestion);
    }
}