using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Archive.ClientBase.Models;
using Archive.ClientBase.ProfileHandler;
using Archive.Contracts;

namespace Archive.ClientBase.Services.Implementation
{
	public sealed class LocalArchiveEntryStore : IArchiveEntryStore
	{
		private readonly string _appdataDir;
		private readonly string _dataPath;
		private readonly ILocalSubscriber _subscriber;
		private List<ArchiveEntryModel> _items = null;

		public LocalArchiveEntryStore(string folderName, ILocalSubscriber localSubscriber)
		{
			_appdataDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Archive");
			_appdataDir = Path.Combine(_appdataDir, folderName);
			Directory.CreateDirectory(_appdataDir);

			_dataPath = Path.Combine(_appdataDir, "ArchiveEntries.json");
			_items = GetItems();
			_subscriber = localSubscriber;
		}

		private List<ArchiveEntryModel> GetItems()
		{
			List<ArchiveEntryModel> items;
			if (File.Exists(_dataPath))
			{
				var data = File.ReadAllText(_dataPath);
				items = JsonSerializer.Deserialize<List<ArchiveEntryModel>>(data, new JsonSerializerOptions() { PropertyNameCaseInsensitive = true });
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
			var text = JsonSerializer.Serialize(_items);
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

			File.Delete(Path.Combine(_appdataDir, id.ToString()));

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

		private static void FolderToArchive(ZipArchive archive, string dirPath, int dirLen)
		{
			var paths = Directory.GetFiles(dirPath);
			var dirs = Directory.GetDirectories(dirPath);

			foreach (var dir in dirs)
			{
				FolderToArchive(archive, dir, dirLen);
			}

			foreach (var path in paths)
			{
				var newName = path.Substring(dirLen);

				var zipArchiveEntry = archive.CreateEntry(newName, CompressionLevel.NoCompression);

				using (var entryStream = zipArchiveEntry.Open())
				{
					using (var memstr = new MemoryStream(File.ReadAllBytes(path)))
					{
						memstr.CopyTo(entryStream);
					}
				}
			}
		}

		public async Task<long> AddFromPathAsync(Guid id, string path)
		{
			var idStr = id.ToString();

			if (Directory.Exists(path))
			{
				var newPath = Path.Combine(Path.GetTempPath(), idStr);
				Directory.CreateDirectory(newPath);
				newPath = Path.Combine(newPath, Path.GetFileName(path));

				using (var stream = File.OpenWrite(newPath))
				{
					using (var archive = new ZipArchive(stream, ZipArchiveMode.Create, true))
					{
						var dirLen = path.Length + 1;
						FolderToArchive(archive, path, dirLen);
					}
				}

				path = newPath;
			}

			if (File.Exists(path))
			{
				await Task.Run(() =>
				{
					var tempPath = Path.GetTempFileName();

					using (var output = File.OpenWrite(tempPath))
					{
						using (var dstream = new DeflateStream(output, CompressionLevel.Optimal))
						{
							var originalBytes = File.ReadAllBytes(path);
							dstream.Write(originalBytes, 0, originalBytes.Length);
						}
					}

					File.Move(tempPath, Path.Combine(_appdataDir, idStr));
				});

				var info = new FileInfo(Path.Combine(_appdataDir, idStr));
				return info.Length;
			}

			return 0;
		}

		public async Task<byte[]> GetBytesFromIdAsync(Guid id)
		{
			return await Task.Run(() =>
			{
				return File.ReadAllBytes(Path.Combine(_appdataDir, id.ToString()));
			});
		}

		public void Dispose()
		{
			// Nothing to do
		}
	}
}