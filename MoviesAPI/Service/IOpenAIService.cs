namespace MoviesAPI.Service
{
    public interface IOpenAIService
    {
        Task<string> AskQuestionAsync(string question);
    }
}
