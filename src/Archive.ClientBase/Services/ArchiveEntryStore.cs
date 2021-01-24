using Cosei.Client.RabbitMq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Archive.ClientBase.Models;
using Archive.Contracts;

namespace Archive.ClientBase.Services
{
	public class ArchiveEntryStore : IDataStore<ArchiveEntryModel>
	{
		private readonly IRequestClient _client;
		private readonly TokenProvider _tokenProvider;
		private IEnumerable<ArchiveEntryModel> items;

		public ArchiveEntryStore(IRequestClient client, TokenProvider tokenProvider)
		{
			_tokenProvider = tokenProvider;
			_client = client;
			items = new List<ArchiveEntryModel>();
		}

		public async Task<IEnumerable<ArchiveEntryModel>> GetItemsAsync(bool forceRefresh = false)
		{
			if (forceRefresh)
			{
				var token = await _tokenProvider.GetToken();

				var headers = new Dictionary<string, string>
				{
					{ "Authorization", $"Bearer {token}" }
				};

				items = await _client.GetAsync<List<ArchiveEntryModel>>("api/ArchiveEntry", headers);
			}

			return items;
		}

		public async Task<ArchiveEntryModel> GetItemAsync(Guid id)
		{
			if (id != Guid.Empty)
			{
				var token = await _tokenProvider.GetToken();

				var headers = new Dictionary<string, string>
				{
					{ "Authorization", $"Bearer {token}" }
				};

				return await _client.GetAsync<ArchiveEntryModel>($"api/ArchiveEntry/{id}", headers);
			}

			return null;
		}

		public async Task<bool> AddItemAsync(ArchiveEntryModel item)
		{
			if (item == null)
			{
				return false;
			}

			var token = await _tokenProvider.GetToken();

			var headers = new Dictionary<string, string>
			{
				{ "Authorization", $"Bearer {token}" }
			};

			var request = new CreateArchiveEntry(item.Id,
				item.Date,
				item.Description,
				item.Groups,
				item.OriginalName,
				item.IsFolder,
				item.FileSize);

			await _client.PostAsync($"api/ArchiveEntry", request, headers);

			return true;
		}

		public async Task<bool> UpdateItemAsync(ArchiveEntryModel item)
		{
			if (item == null || item.Id == Guid.Empty)
			{
				return false;
			}

			var token = await _tokenProvider.GetToken();

			var headers = new Dictionary<string, string>
			{
				{ "Authorization", $"Bearer {token}" }
			};

			var request = new UpdateArchiveEntry(item.Id,
				item.Date,
				item.Description,
				item.Groups,
				item.OriginalName,
				item.IsFolder,
				item.FileSize);

			await _client.PutAsync($"api/ArchiveEntry", request, headers);

			return true;
		}

		public async Task<bool> DeleteItemAsync(Guid id)
		{
			if (id == Guid.Empty)
			{
				return false;
			}

			var token = await _tokenProvider.GetToken();

			var headers = new Dictionary<string, string>
			{
				{ "Authorization", $"Bearer {token}" }
			};

			var response = await _client.DeleteAsync($"api/ArchiveEntry/{id}", headers);
			response.EnsureSuccessStatusCode();

			return true;
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
					_client.Dispose();
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