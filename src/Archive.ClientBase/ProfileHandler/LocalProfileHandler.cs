using System.Threading.Tasks;
using Archive.ClientBase.Models;
using Archive.ClientBase.Services.Implementation;

namespace Archive.ClientBase.ProfileHandler
{
	public sealed class LocalProfileHandler : AbstractProfileHandler
	{
		private readonly IMessenger _messenger;
		private readonly IDispatcherHelper _dispatcher;

		public LocalProfileHandler(IMessenger messenger, IDispatcherHelper dispatcher)
		{
			_messenger = messenger;
			_dispatcher = dispatcher;
		}

		protected override async Task ApplyProfile(ProfileBase profile)
		{
			if (profile is LocalProfile localProfile)
			{
				var subscriber = new LocalSubscriber();
				var archiveEntryStore = new LocalArchiveEntryStore(localProfile.FolderName, subscriber);
				ArchiveEntryService = new ArchiveEntryService(archiveEntryStore, _messenger, _dispatcher, subscriber);
				var userStore = new LocalUserStore(localProfile.FolderName, subscriber);
				UserService = new UserService(userStore, _messenger, _dispatcher, subscriber);

				await subscriber.StartAsync();

				Subscriber = subscriber;
			}
		}
	}
}
