using MoviesAPI.Models;

namespace MoviesAPI.Repositories.Interface
{
    public interface IChatBotRepository
    {
        Task<List<Faq>> GetAllFaqAsync();                       
        Task<Faq?> GetFaqByIdAsync(int id);                    
        Task AddFaqAsync(Faq faq);                              
        Task UpdateFaqAsync(Faq faq);                           
        Task DeleteFaqAsync(int id);                        
        Task<Faq?> GetClosestMatchToFaqAsync(string userQuestion);
    }
}
