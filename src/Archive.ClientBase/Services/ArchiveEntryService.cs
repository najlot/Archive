using Cosei.Client.Base;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Archive.ClientBase.Models;
using Archive.Contracts;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace Archive.ClientBase.Services
{
	public class ArchiveEntryService : IDisposable
	{
		private IArchiveDataStore<ArchiveEntryModel> _store;
		private readonly Messenger _messenger;
		private readonly IDispatcherHelper _dispatcher;
		private readonly ISubscriber _subscriber;

		public ArchiveEntryService(
			IArchiveDataStore<ArchiveEntryModel> dataStore,
			Messenger messenger,
			IDispatcherHelper dispatcher,
			ISubscriber subscriber)
		{
			_store = dataStore;
			_messenger = messenger;
			_dispatcher = dispatcher;
			_subscriber = subscriber;

			subscriber.Register<ArchiveEntryCreated>(Handle);
			subscriber.Register<ArchiveEntryUpdated>(Handle);
			subscriber.Register<ArchiveEntryDeleted>(Handle);
		}

		private async Task Handle(ArchiveEntryCreated message)
		{
			await _dispatcher.BeginInvokeOnMainThread(async () => await _messenger.SendAsync(message));
		}

		private async Task Handle(ArchiveEntryUpdated message)
		{
			await _dispatcher.BeginInvokeOnMainThread(async () => await _messenger.SendAsync(message));
		}

		private async Task Handle(ArchiveEntryDeleted message)
		{
			await _dispatcher.BeginInvokeOnMainThread(async () => await _messenger.SendAsync(message));
		}

		public ArchiveEntryModel CreateArchiveEntry()
		{
			return new ArchiveEntryModel()
			{
				Id = Guid.NewGuid(),
				Description = "",
				OriginalName = "",
				FileSize = "0 B",
				Date = DateTime.Now
			};
		}

		public async Task<bool> AddItemAsync(ArchiveEntryModel item, string path)
		{
			var size = await _store.AddFromPathAsync(item.Id, path);

			var sizeList = new List<(int MinSize, string Suffix)>
			{
				(0, "B"),
				(1024, "KB"),
				(1024 * 1024, "MB"),
				(1024 * 1024 * 1024, "GB")
			};

			sizeList = sizeList.Where(e => e.MinSize < size).ToList();
			var minSize = sizeList.Max(e => e.MinSize);
			var entry = sizeList.First(e => e.MinSize == minSize);

			if (entry.MinSize != 0)
			{
				size = size / entry.MinSize;
			}

			item.FileSize = size + " " + entry.Suffix;


			return await _store.AddItemAsync(item);
		}

		public async Task<bool> DeleteItemAsync(Guid id)
		{
			return await _store.DeleteItemAsync(id);
		}

		public async Task<ArchiveEntryModel> GetItemAsync(Guid id)
		{
			return await _store.GetItemAsync(id);
		}

		public async Task<IEnumerable<ArchiveEntryModel>> GetItemsAsync(bool forceRefresh = false)
		{
			return await _store.GetItemsAsync(forceRefresh);
		}

		public async Task<bool> UpdateItemAsync(ArchiveEntryModel item)
		{
			return await _store.UpdateItemAsync(item);
		}

		public async Task ExportEntryAsync(Guid id, bool isFolder, string destinationPath)
		{
			var bytes = await _store.GetBytesFromIdAsync(id);

			await Task.Run(() =>
			{
				var tempPath = Path.GetTempFileName();
				File.WriteAllBytes(tempPath, bytes);

				if (isFolder)
				{
					var decompressedTempPath = Path.GetTempFileName();

					using (var input = File.OpenRead(tempPath))
					{
						using (var output = File.OpenWrite(decompressedTempPath))
						{
							using (var dstream = new DeflateStream(input, CompressionMode.Decompress))
							{
								dstream.CopyTo(output);
							}
						}
					}

					using (var input = File.OpenRead(decompressedTempPath))
					{
						using (var za = new ZipArchive(input))
						{
							var destFolder = destinationPath;

							long folderNr = 0;

							while (Directory.Exists(destFolder))
							{
								destFolder = destinationPath + " " + ++folderNr;
							}

							Directory.CreateDirectory(destFolder);

							za.ExtractToDirectory(destFolder);
						}
					}

					File.Delete(decompressedTempPath);
				}
				else
				{
					using (var input = File.OpenRead(tempPath))
					{
						using (var output = File.OpenWrite(destinationPath))
						{
							using (var dstream = new DeflateStream(input, CompressionMode.Decompress))
							{
								dstream.CopyTo(output);
							}
						}
					}
				}

				File.Delete(tempPath);
			});
		}

		#region IDisposable Support

		private bool disposedValue = false; // To detect redundant calls

		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				disposedValue = true;

				if (disposing)
				{
					_subscriber.Unregister(this);
					_store?.Dispose();
					_store = null;
				}
			}
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		#endregion IDisposable Support
	}
}