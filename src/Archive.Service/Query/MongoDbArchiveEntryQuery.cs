using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Archive.Service.Model;

namespace Archive.Service.Query
{
    public class MongoDbArchiveEntryQuery : IArchiveEntryQuery
	{
		private readonly IMongoCollection<ArchiveEntryModel> _collection;

		public MongoDbArchiveEntryQuery(IMongoDatabase database)
		{
			_collection = database.GetCollection<ArchiveEntryModel>(nameof(ArchiveEntryModel)[0..^5]);
		}

		public async Task<ArchiveEntryModel> GetAsync(Guid id)
		{
			var result = await _collection.FindAsync(item => item.Id == id);
			var item = result.FirstOrDefault();

			if (item == null)
			{
				return null;
			}

			return item;
		}

		public async IAsyncEnumerable<ArchiveEntryModel> GetAllAsync()
		{
			var items = await _collection.FindAsync(FilterDefinition<ArchiveEntryModel>.Empty);
			
			while (await items.MoveNextAsync())
			{
				foreach (var item in items.Current)
				{
					yield return item;
				}
			}
		}

		public async IAsyncEnumerable<ArchiveEntryModel> GetAllOrderedByDateAsync()
		{
			var items = await _collection.Find(FilterDefinition<ArchiveEntryModel>.Empty)
				.SortByDescending(x => x.Date)
				.ToCursorAsync();
			
			while (await items.MoveNextAsync())
			{
				foreach (var item in items.Current)
				{
					yield return item;
				}
			}
		}

		public async IAsyncEnumerable<ArchiveEntryModel> GetAllAsync(Expression<Func<ArchiveEntryModel, bool>> predicate)
		{
			var items = await _collection.FindAsync(predicate);

			while (await items.MoveNextAsync())
			{
				foreach (var item in items.Current)
				{
					yield return item;
				}
			}
		}
	}
}
