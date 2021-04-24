using Cosei.Client.Base;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Archive.ClientBase.Models;
using Archive.Contracts;

namespace Archive.ClientBase.Services.Implementation
{
	public class ArchiveEntryService : IArchiveEntryService
	{
		private IArchiveEntryStore _store;
		private readonly IMessenger _messenger;
		private readonly IDispatcherHelper _dispatcher;
		private readonly ISubscriber _subscriber;

		public ArchiveEntryService(
			IArchiveEntryStore dataStore,
			IMessenger messenger,
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
				FileSize = "",
			};
		}

		public async Task<bool> AddItemAsync(ArchiveEntryModel item)
		{
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