using Plainquire.Filter;
using Plainquire.Sort;
using Plainquire.Page;

namespace Backend_Boarding_house_management_system.Repositories.Interfaces.Base
{
    /// <summary>
    /// Generic repository interface chứa các CRUD operations chung cho tất cả entity.
    /// TEntity: kiểu entity, TKey: kiểu khóa chính (thường là string).
    /// </summary>
    public interface IRepository<TEntity, TKey> where TEntity : class
    {
        /// <summary>Lấy entity theo khóa chính.</summary>
        Task<TEntity?> GetByIdAsync(TKey id);

        /// <summary>Lấy tất cả entity (không phân trang).</summary>
        Task<List<TEntity>> GetAllAsync();

        /// <summary>Lấy danh sách entity với filter/sort/page của Plainquire.</summary>
        Task<(IEnumerable<TEntity> Items, int TotalCount)> GetByFilterAsync(
            EntityFilter<TEntity> filter,
            EntitySort<TEntity> sort,
            EntityPage page);

        /// <summary>Thêm entity mới.</summary>
        Task AddAsync(TEntity entity);

        /// <summary>Cập nhật entity đã tồn tại.</summary>
        Task UpdateAsync(TEntity entity);

        /// <summary>Xóa entity theo khóa chính.</summary>
        Task DeleteAsync(TKey id);

        /// <summary>Kiểm tra entity có tồn tại theo khóa chính không.</summary>
        Task<bool> ExistsAsync(TKey id);
    }
}
