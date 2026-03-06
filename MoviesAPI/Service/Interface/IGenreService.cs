namespace MoviesAPI.Service.Interface
{
    public interface IGenreService
    {
        Task<List<string>> GetAllAsync();
    }
}
