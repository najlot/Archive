using Dapper;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Archive.Contracts;
using Archive.Service.Configuration;
using Archive.Service.Model;

namespace Archive.Service.Query
{
	public class MySqlArchiveEntryQuery : IArchiveEntryQuery
	{
		private readonly string _connectionString;

		public MySqlArchiveEntryQuery(MySqlConfiguration _configuration)
		{
			_connectionString = $"server={_configuration.Host};" +
				$"port={_configuration.Port};" +
				$"database={_configuration.Database};" +
				$"uid={_configuration.User};" +
				$"password={_configuration.Password}";
		}

		public async Task<ArchiveEntryModel> GetAsync(Guid id)
		{
			using var db = new MySqlConnection(_connectionString);
			var item = await db.QueryFirstOrDefaultAsync<ArchiveEntryModel>("SELECT * FROM ArchiveEntries WHERE Id=@id", new { id });
			
			if (item == null)
			{
				return null;
			}

			item.Groups = (await db.QueryAsync<ArchiveGroup>("SELECT * FROM ArchiveEntry_Groups WHERE ArchiveEntryModelId=@id", new { id })).ToList();

			return item;
		}

		public async IAsyncEnumerable<ArchiveEntryModel> GetAllAsync()
		{
			using var db = new MySqlConnection(_connectionString);
			var items = await db.QueryAsync<ArchiveEntryModel>("SELECT * FROM ArchiveEntries");

			foreach (var item in items)
			{
				item.Groups = (await db.QueryAsync<ArchiveGroup>("SELECT * FROM ArchiveEntry_Groups WHERE ArchiveEntryModelId=@id", new { item.Id })).ToList();
				yield return item;
			}
		}

		public async IAsyncEnumerable<ArchiveEntryModel> GetAllAsync(Expression<Func<ArchiveEntryModel, bool>> predicate)
		{
			var check = predicate.Compile();
			
			await foreach (var item in GetAllAsync())
			{
				if (check(item))
				{
					yield return item;
				}
			}
		}
	}
}
