using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Archive.Service.Model;

namespace Archive.Service.Repository
{
	public class MongoDbArchiveEntryRepository : IArchiveEntryRepository
	{
		private readonly IMongoCollection<ArchiveEntryModel> _collection;

		public MongoDbArchiveEntryRepository(IMongoDatabase database)
		{
			_collection = database.GetCollection<ArchiveEntryModel>(nameof(ArchiveEntryModel)[0..^5]);
		}

		public void Delete(Guid id)
		{
			_collection.DeleteOne(item => item.Id == id);
		}

		public ArchiveEntryModel Get(Guid id)
		{
			return _collection.Find(item => item.Id == id).FirstOrDefault();
		}

		public IEnumerable<ArchiveEntryModel> GetAll()
		{
			return _collection.AsQueryable();
		}

		public IEnumerable<ArchiveEntryModel> GetAll(Expression<Func<ArchiveEntryModel, bool>> predicate)
		{
			return _collection.Find(predicate).ToEnumerable();
		}

		public void Insert(ArchiveEntryModel model)
		{
			_collection.InsertOne(model);
		}

		public void Update(ArchiveEntryModel model)
		{
			_collection.FindOneAndReplace(item => item.Id == model.Id, model);
		}
	}
}