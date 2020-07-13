using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Algoserver.API.Models.REST;

namespace Algoserver.API.Helpers
{
    public static class PaggingHelper
    {
        public static CountResponse<List<TResponse>> GetCountResponse<T, TResponse>(this IEnumerable<T> collection, PaggingRequest pagging,
            Expression<Func<T, bool>> search = null, Expression<Func<T, bool>> filter = null,
            Expression<Func<T, TResponse>> select = null, int maxPerPage = int.MaxValue)
                  where TResponse : class
        {
            if (search != null)
                collection = collection.Where(search.Compile());

            if (filter != null)
                collection = collection.Where(filter.Compile());

            var maxTake = pagging.Take > maxPerPage ? maxPerPage : pagging.Take;
            select = select ?? (t => t as TResponse);

            IEnumerable<TResponse> response = collection.Skip(pagging.Skip).Take(maxTake).Select(select.Compile());

            return new CountResponse<List<TResponse>>
            {
                Count = response.Count(),
                Data = response.ToList()
            };
        }
    }
}
