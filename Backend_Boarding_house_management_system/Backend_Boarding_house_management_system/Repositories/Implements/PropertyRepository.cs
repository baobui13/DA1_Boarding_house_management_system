using Backend_Boarding_house_management_system.Data;
using Backend_Boarding_house_management_system.Entities;
using Backend_Boarding_house_management_system.Repositories.Implements.Base;
using Backend_Boarding_house_management_system.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Plainquire.Filter;
using Plainquire.Page;
using Plainquire.Sort;

namespace Backend_Boarding_house_management_system.Repositories.Implements
{
    public class PropertyRepository : Repository<Property, string>, IPropertyRepository
    {
        public PropertyRepository(AppDbContext context) : base(context) { }

        private IQueryable<Property> GetDetailsQuery()
            => _dbSet
                .Include(p => p.Landlord)
                .Include(p => p.Area)
                .Include(p => p.PropertyImages)
                .Include(p => p.RoomAmenities)
                    .ThenInclude(ra => ra.Amenity)
                .Include(p => p.Contracts)
                .Include(p => p.Appointments)
                .Include(p => p.Ratings)
                .Include(p => p.PropertyAspectScores);

        protected override IQueryable<Property> GetQueryWithIncludes()
            => _dbSet.Include(p => p.Ratings);

        public async Task<Property?> GetByIdWithDetailsAsync(string id)
            => await GetDetailsQuery().FirstOrDefaultAsync(p => p.Id == id);

        public override async Task<(IEnumerable<Property> Items, int TotalCount)> GetByFilterAsync(
            EntityFilter<Property> filter,
            EntitySort<Property> sort,
            EntityPage page)
        {
            page = EnsurePage(page);
            var query = GetQueryWithIncludes().AsNoTracking();

            // Enforce that only Approved and Available properties can be listed publicly
            query = query.Where(p => p.ModerationStatus == ModerationStatusEnum.Approved && p.AvailabilityStatus == AvailabilityStatusEnum.Available);

            query = query.Where(filter);
            query = query.OrderBy(sort);

            var totalCount = await query.CountAsync();
            var items = await query.Page(page).ToListAsync();
            return (items, totalCount);
        }

        public async Task<(IEnumerable<Property> Items, int TotalCount)> GetByFilterWithDetailsAsync(
            EntityFilter<Property> filter,
            EntitySort<Property> sort,
            EntityPage page)
        {
            page = EnsurePage(page);
            var query = GetDetailsQuery().AsNoTracking();
            query = query.Where(filter);
            query = query.OrderBy(sort);

            var totalCount = await query.CountAsync();
            var items = await query.Page(page).ToListAsync();
            return (items, totalCount);
        }

        public async Task<IEnumerable<Property>> GetFilteredCandidatesForRecAsync(EntityFilter<Property> filter, int maxCandidates = 200)
        {
            var query = GetDetailsQuery().AsNoTracking();
            
            // Lọc ra các property được hiển thị công khai (Approved & Available)
            query = query.Where(p => p.ModerationStatus == ModerationStatusEnum.Approved && p.AvailabilityStatus == AvailabilityStatusEnum.Available);
            
            query = query.Where(filter);
            // Dùng order ổn định để có tập candidate deterministic trước khi re-rank ở service
            query = query.OrderByDescending(p => p.CreatedAt);

            return await query.Take(maxCandidates).ToListAsync();
        }

        public async Task<Dictionary<string, int>> GetPropertyViewCountsAsync(EntityFilter<Property> filter, int maxCandidates = 300)
        {
            // Lấy candidate properties trước (giới hạn) để join/group an toàn
            var candidateQuery = _dbSet.AsNoTracking().Where(filter);
            var candidateIds = await candidateQuery
                .OrderByDescending(p => p.CreatedAt)
                .Take(maxCandidates)
                .Select(p => p.Id)
                .ToListAsync();

            if (candidateIds.Count == 0)
                return new Dictionary<string, int>();

            var counts = await _context.ViewHistories
                .AsNoTracking()
                .Where(vh => candidateIds.Contains(vh.PropertyId))
                .GroupBy(vh => vh.PropertyId)
                .Select(g => new { PropertyId = g.Key, Count = g.Count() })
                .ToListAsync();

            return counts.ToDictionary(x => x.PropertyId, x => x.Count);
        }

        public async Task<Dictionary<string, int>> GetPriceBucketCountsAsync(
            EntityFilter<Property> filter,
            List<(string Label, decimal Min, decimal Max)> buckets)
        {
            var query = _dbSet.AsNoTracking().Where(filter);

            // Chỉ xét các property đang active/approved để "phổ biến hiện nay"
            // (có thể mở rộng filter từ caller)
            var prices = await query
                .Where(p => p.ModerationStatus == ModerationStatusEnum.Approved &&
                            p.AvailabilityStatus == AvailabilityStatusEnum.Available)
                .Select(p => p.Price)
                .ToListAsync();

            var result = new Dictionary<string, int>();
            foreach (var (label, min, max) in buckets)
            {
                int count = prices.Count(p => p >= min && p < max);
                result[label] = count;
            }

            // Thêm bucket "Khác" nếu cần
            return result;
        }
    }
}
