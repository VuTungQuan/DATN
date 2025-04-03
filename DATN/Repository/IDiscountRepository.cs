using DATN.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DATN.Repositories
{
    public interface IDiscountRepository
    {
        Task<IEnumerable<Discount>> GetAllDiscountsAsync();
        Task<Discount?> GetDiscountByIdAsync(int id);
        Task AddDiscountAsync(Discount discount);
        Task UpdateDiscountAsync(Discount discount);
        Task DeleteDiscountAsync(int id);
    }
}
