using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Archive.ClientBase.Models;

namespace Archive.ClientBase.Services
{
	public interface IArchiveEntryService : IDisposable
	{
		ArchiveEntryModel CreateArchiveEntry();
		Task<bool> AddItemAsync(ArchiveEntryModel item, string path);
		Task<IEnumerable<ArchiveEntryModel>> GetItemsAsync(bool forceRefresh = false);
		Task<ArchiveEntryModel> GetItemAsync(Guid id);
		Task<bool> UpdateItemAsync(ArchiveEntryModel item);
		Task<bool> DeleteItemAsync(Guid id);
		Task ExportEntryAsync(Guid id, bool isFolder, string destinationPath);
	}
}
