using A1Commerce.Infrastructure;
using Microsoft.EntityFrameworkCore;
using A1Commerce.BusinessLayer.Implementation.Interface;

namespace A1Commerce.BusinessLayer.Implementation
{
    public class BaseRepository : IRepository
    {
        private readonly I25DbContext _context;

        public I25DbContext Context => _context;

        public BaseRepository(I25DbContext context)
        {
            _context = context;
        }

        public DbSet<T> GetQuery<T>() where T : class
        {
            return _context.Set<T>();
        }
    }
}