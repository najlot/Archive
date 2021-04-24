using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Archive.ClientBase.Models;

namespace Archive.ClientBase.Services
{
	public interface IUserStore : IDisposable
	{
		Task<bool> AddItemAsync(UserModel item);

		Task<bool> UpdateItemAsync(UserModel item);

		Task<bool> DeleteItemAsync(Guid id);

		Task<UserModel> GetItemAsync(Guid id);

		Task<IEnumerable<UserModel>> GetItemsAsync(bool forceRefresh = false);
	}
}
