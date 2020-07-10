using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Common.Data.Repositories
{
    internal static class QueryExtensions
    {
        internal static IQueryable<T> OptionalInclude<T>(this IQueryable<T> query, string include) where T : class
        {
            return query = include != null ? query.Include(include) : query;
        }
    }
}
