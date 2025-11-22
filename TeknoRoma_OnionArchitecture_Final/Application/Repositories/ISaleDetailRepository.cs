using Domain.Entities;

namespace Application.Repositories
{
    public interface ISaleDetailRepository : IRepository<SaleDetail>
    {
        Task<IReadOnlyList<SaleDetail>> GetBySaleAsync(int saleId);
        Task<IReadOnlyList<SaleDetail>> GetByProductAsync(int productId);
        Task<IReadOnlyList<SaleDetail>> GetTopSellingProductsAsync(int count, DateTime? startDate = null, DateTime? endDate = null);
    }
}
