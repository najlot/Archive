using System;
using System.Threading.Tasks;

namespace Archive.ClientBase.Services
{
	public interface IArchiveDataStore<TModel> : IDataStore<TModel>
	{
		Task<long> AddFromPathAsync(Guid id, string path);
		Task<byte[]> GetBytesFromIdAsync(Guid id);
	}
}