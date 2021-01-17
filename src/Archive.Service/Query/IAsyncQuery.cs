using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Archive.Service.Query
{
	public interface IAsyncQuery<TKey, TModel>
		where TModel : class, new()
	{
		Task<TModel> GetAsync(TKey id);

		IAsyncEnumerable<TModel> GetAllAsync();

		IAsyncEnumerable<TModel> GetAllAsync(Expression<Func<TModel, bool>> predicate);
	}
}
