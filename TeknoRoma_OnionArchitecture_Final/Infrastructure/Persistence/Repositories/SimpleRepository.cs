// ===================================================================================
// TEKNOROMA - SIMPLE REPOSITORY IMPLEMENTASYONU (SimpleRepository.cs)
// ===================================================================================
//
// Basit entity'ler icin repository implementasyonu.
// Category, Supplier, Store, Department gibi lookup tablolari icin kullanilir.
//
// ===================================================================================

using Application.Repositories;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    /// <summary>
    /// Simple Repository Implementasyonu
    ///
    /// ISimpleRepository interface'ini implement eder.
    /// GetAllActiveAsync metodunu saglar.
    /// </summary>
    /// <typeparam name="T">BaseEntity'den tureyen entity tipi</typeparam>
    public class SimpleRepository<T> : EfRepository<T>, ISimpleRepository<T> where T : BaseEntity
    {
        public SimpleRepository(AppDbContext context) : base(context)
        {
        }

        /// <summary>
        /// Aktif kayitlari getir
        /// IsActive = true ve IsDeleted = false olan kayitlar
        /// </summary>
        public virtual async Task<IReadOnlyList<T>> GetAllActiveAsync()
        {
            // T tipinin IsActive property'sine sahip olup olmadigini kontrol et
            // Category, Supplier gibi entity'ler IsActive property'sine sahip
            var query = _dbSet.Where(e => !e.IsDeleted);

            // Dinamik olarak IsActive kontrolu yapmak icin reflection kullaniyoruz
            // Ancak daha performansli cozum icin her entity icin ayri repository yazilabilir
            var propertyInfo = typeof(T).GetProperty("IsActive");
            if (propertyInfo != null)
            {
                // Expression tree ile dinamik filtreleme
                var parameter = System.Linq.Expressions.Expression.Parameter(typeof(T), "e");
                var property = System.Linq.Expressions.Expression.Property(parameter, "IsActive");
                var trueConstant = System.Linq.Expressions.Expression.Constant(true);
                var equals = System.Linq.Expressions.Expression.Equal(property, trueConstant);
                var lambda = System.Linq.Expressions.Expression.Lambda<Func<T, bool>>(equals, parameter);

                query = query.Where(lambda);
            }

            return await query.ToListAsync();
        }
    }
}
