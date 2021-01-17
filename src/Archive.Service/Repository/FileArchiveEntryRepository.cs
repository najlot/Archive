using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;
using Archive.Service.Configuration;
using Archive.Service.Model;

namespace Archive.Service.Repository
{
	public class FileArchiveEntryRepository : IArchiveEntryRepository
	{
		private readonly string _storagePath;

		public FileArchiveEntryRepository(FileConfiguration configuration)
		{
			_storagePath = configuration.ArchiveEntriesPath;
			Directory.CreateDirectory(_storagePath);
		}

		public void Delete(Guid id)
		{
			var path = Path.Combine(_storagePath, id.ToString());
			File.Delete(path);
		}

		public ArchiveEntryModel Get(Guid id)
		{
			var path = Path.Combine(_storagePath, id.ToString());

			if (!File.Exists(path))
			{
				return null;
			}

			var bytes = File.ReadAllBytes(path);
			var text = Encoding.UTF8.GetString(bytes);
			var item = JsonConvert.DeserializeObject<ArchiveEntryModel>(text);

			return item;
		}

		public void Insert(ArchiveEntryModel model)
		{
			Update(model);
		}

		public void Update(ArchiveEntryModel model)
		{
			var path = Path.Combine(_storagePath, model.Id.ToString());
			var str = JsonConvert.SerializeObject(model);
			var bytes = Encoding.UTF8.GetBytes(str);
			File.WriteAllBytes(path, bytes);
		}
	}
}