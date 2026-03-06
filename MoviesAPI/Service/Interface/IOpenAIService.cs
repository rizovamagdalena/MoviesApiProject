namespace MoviesAPI.Service.Interface
{
    public interface IOpenAIService
    {
        Task<string> AskQuestionAsync(string question);
    }
}
