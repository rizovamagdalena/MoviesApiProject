namespace MoviesAPI.Repositories.Interface
{
    public interface IGenreRepository
    {
        Task<List<string>> GetAllAsync();
    }
}
