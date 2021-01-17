using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Archive.ClientBase.Models;
using Archive.ClientBase.ProfileHandler;
using Archive.Contracts;

namespace Archive.ClientBase.Services
{
	public sealed class LocalArchiveEntryStore : IDataStore<ArchiveEntryModel>
	{
		private readonly string _dataPath;
		private readonly LocalSubscriber _subscriber;
		private List<ArchiveEntryModel> _items = null;

		public LocalArchiveEntryStore(string folderName, LocalSubscriber localSubscriber)
		{
			var appdataDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Archive");
			appdataDir = Path.Combine(appdataDir, folderName);
			Directory.CreateDirectory(appdataDir);

			_dataPath = Path.Combine(appdataDir, "ArchiveEntries.json");
			_items = GetItems();
			_subscriber = localSubscriber;
		}

		private List<ArchiveEntryModel> GetItems()
		{
			List<ArchiveEntryModel> items;
			if (File.Exists(_dataPath))
			{
				var data = File.ReadAllText(_dataPath);
				items = Newtonsoft.Json.JsonConvert.DeserializeObject<List<ArchiveEntryModel>>(data);
			}
			else
			{
				items = new List<ArchiveEntryModel>();
			}

			return items;
		}

		public async Task<bool> AddItemAsync(ArchiveEntryModel item)
		{
			_items.Insert(0, item);

			SaveItems();

			await _subscriber.SendAsync(new ArchiveEntryCreated(
				item.Id,
				item.Date,
				item.Description,
				item.Groups,
				item.OriginalName,
				item.IsFolder,
				item.FileSize));

			return await Task.FromResult(true);
		}

		private void SaveItems()
		{
			var text = Newtonsoft.Json.JsonConvert.SerializeObject(_items);
			File.WriteAllText(_dataPath, text);
		}

		public async Task<bool> UpdateItemAsync(ArchiveEntryModel item)
		{
			int index = 0;
			var oldItem = _items.FirstOrDefault(i => i.Id == item.Id);

			if (oldItem != null)
			{
				index = _items.IndexOf(oldItem);

				if (index != -1)
				{
					_items.RemoveAt(index);
				}
				else
				{
					index = 0;
				}
			}

			_items.Insert(index, item);

			SaveItems();

			await _subscriber.SendAsync(new ArchiveEntryUpdated(
				item.Id,
				item.Date,
				item.Description,
				item.Groups,
				item.OriginalName,
				item.IsFolder,
				item.FileSize));

			return await Task.FromResult(true);
		}

		public async Task<bool> DeleteItemAsync(Guid id)
		{
			var oldItem = _items.FirstOrDefault(arg => arg.Id == id);
			_items.Remove(oldItem);

			SaveItems();

			await _subscriber.SendAsync(new ArchiveEntryDeleted(id));

			return await Task.FromResult(true);
		}

		public async Task<ArchiveEntryModel> GetItemAsync(Guid id)
		{
			return await Task.FromResult(_items.FirstOrDefault(s => s.Id == id));
		}

		public async Task<IEnumerable<ArchiveEntryModel>> GetItemsAsync(bool forceRefresh = false)
		{
			_items = GetItems();

			return await Task.FromResult(_items);
		}

		public void Dispose()
		{
			// Nothing to do
		}
	}
}