using MongoDB.Driver.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Archive.Contracts;
using Archive.Service.Configuration;
using Archive.Service.Model;

namespace Archive.Service.Query
{
	public class FileArchiveEntryQuery : IArchiveEntryQuery
	{
		private readonly string _storagePath;

		public FileArchiveEntryQuery(FileConfiguration configuration)
		{
			_storagePath = configuration.ArchiveEntriesPath;
			Directory.CreateDirectory(_storagePath);
		}

		public async Task<ArchiveEntryModel> GetAsync(Guid id)
		{
			var path = Path.Combine(_storagePath, id.ToString());

			if (!File.Exists(path))
			{
				return null;
			}

			var bytes = await File.ReadAllBytesAsync(path);
			var text = Encoding.UTF8.GetString(bytes);
			var item = JsonConvert.DeserializeObject<ArchiveEntryModel>(text);

			return item;
		}

		public async IAsyncEnumerable<ArchiveEntryModel> GetAllAsync()
		{
			foreach (var path in Directory.GetFiles(_storagePath))
			{
				var bytes = await File.ReadAllBytesAsync(path);
				var text = Encoding.UTF8.GetString(bytes);
				var item = JsonConvert.DeserializeObject<ArchiveEntryModel>(text);
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
