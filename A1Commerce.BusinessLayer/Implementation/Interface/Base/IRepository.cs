using A1Commerce.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace A1Commerce.BusinessLayer.Implementation.Interface
{
    public interface IRepository
    {
        I25DbContext Context { get; }

        DbSet<T> GetQuery<T>() where T : class;
    }
}