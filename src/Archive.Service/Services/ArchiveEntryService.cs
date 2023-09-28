using Cosei.Service.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Archive.Contracts;
using Archive.Service.Model;
using Archive.Service.Query;
using Archive.Service.Repository;
using System.IO;
using Archive.Service.Configuration;

namespace Archive.Service.Services
{
	public class ArchiveEntryService : IDisposable
	{
		private readonly IArchiveEntryRepository _archiveEntryRepository;
		private readonly IArchiveEntryQuery _archiveEntryQuery;
		private readonly BlobConfiguration _blobConfiguration;
		private readonly IPublisher _publisher;

		public ArchiveEntryService(IArchiveEntryRepository archiveEntryRepository,
			IArchiveEntryQuery archiveEntryQuery,
			BlobConfiguration blobConfiguration,
			IPublisher publisher)
		{
			_archiveEntryRepository = archiveEntryRepository;
			_archiveEntryQuery = archiveEntryQuery;
			_blobConfiguration = blobConfiguration;
			_publisher = publisher;
		}

		public void CreateArchiveEntry(CreateArchiveEntry command, string userName)
		{
			var item = new ArchiveEntryModel()
			{
				Id = command.Id,
				Date = command.Date,
				Description = command.Description,
				Groups = command.Groups,
				OriginalName = command.OriginalName,
				IsFolder = command.IsFolder,
				FileSize = command.FileSize,
			};

			_archiveEntryRepository.Insert(item);

			_publisher.PublishAsync(new ArchiveEntryCreated(
				command.Id,
				command.Date,
				command.Description,
				command.Groups,
				command.OriginalName,
				command.IsFolder,
				command.FileSize));
		}

		public async Task SaveBytesAsync(Guid id, byte[] bytes)
		{
			var targetFilePath = Path.Combine(_blobConfiguration.BlobsPath, id.ToString());
			await File.WriteAllBytesAsync(targetFilePath, bytes);
		}

		public Stream GetBlobStream(Guid id)
		{
			return File.OpenRead(Path.Combine(_blobConfiguration.BlobsPath, id.ToString()));
		}

		public async Task<byte[]> GetBytesAsync(Guid id)
		{
			return await File.ReadAllBytesAsync(Path.Combine(_blobConfiguration.BlobsPath, id.ToString()));
		}

		public void UpdateArchiveEntry(UpdateArchiveEntry command, string userName)
		{
			var item = _archiveEntryRepository.Get(command.Id);
			
			// item.Date = command.Date;
			item.Description = command.Description;
			item.OriginalName = command.OriginalName;
			// item.IsFolder = command.IsFolder;
			// item.FileSize = command.FileSize;

			while (item.Groups.Count > command.Groups.Count)
			{
				item.Groups.RemoveAt(item.Groups.Count - 1);
			}

			while (item.Groups.Count < command.Groups.Count)
			{
				item.Groups.Add(new ArchiveGroup());
			}

			for (int i = 0; i < item.Groups.Count; i++)
			{
				item.Groups[i].GroupName = command.Groups[i].GroupName;
			}

			_archiveEntryRepository.Update(item);

			_publisher.PublishAsync(new ArchiveEntryUpdated(
				command.Id,
				item.Date,
				command.Description,
				command.Groups,
				command.OriginalName,
				item.IsFolder,
				item.FileSize));
		}

		public void DeleteArchiveEntry(Guid id, string userName)
		{
			_archiveEntryRepository.Delete(id);
			File.Delete(Path.Combine(_blobConfiguration.BlobsPath, id.ToString()));

			_publisher.PublishAsync(new ArchiveEntryDeleted(id));
		}

		public async Task<ArchiveEntry> GetItemAsync(Guid id)
		{
			var item = await _archiveEntryQuery.GetAsync(id);

			if (item == null)
			{
				return null;
			}

			return new ArchiveEntry
			{
				Id = item.Id,
				Date = item.Date,
				Description = item.Description,
				Groups = item.Groups,
				OriginalName = item.OriginalName,
				IsFolder = item.IsFolder,
				FileSize = item.FileSize,
			};
		}

		public async IAsyncEnumerable<ArchiveEntry> GetItemsForUserAsync(string userName)
		{
			await foreach (var item in _archiveEntryQuery.GetAllOrderedByDateAsync())
			{
				yield return new ArchiveEntry
				{
					Id = item.Id,
					Date = item.Date,
					Description = item.Description,
					Groups = item.Groups,
					OriginalName = item.OriginalName,
					IsFolder = item.IsFolder,
					FileSize = item.FileSize,
				};
			}
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
					(_archiveEntryRepository as IDisposable)?.Dispose();
					(_archiveEntryQuery as IDisposable)?.Dispose();
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