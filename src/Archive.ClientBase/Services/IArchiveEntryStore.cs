using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Archive.ClientBase.Models;

namespace Archive.ClientBase.Services
{
	public interface IArchiveEntryStore : IDisposable
	{
		Task<bool> AddItemAsync(ArchiveEntryModel item);

		Task<bool> UpdateItemAsync(ArchiveEntryModel item);

		Task<bool> DeleteItemAsync(Guid id);

		Task<ArchiveEntryModel> GetItemAsync(Guid id);

		Task<IEnumerable<ArchiveEntryModel>> GetItemsAsync(bool forceRefresh = false);

		Task<long> AddFromPathAsync(Guid id, string path);
		Task<byte[]> GetBytesFromIdAsync(Guid id);
	}
}
