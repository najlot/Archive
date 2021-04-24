using Archive.ClientBase.Models;
using Archive.ClientBase.Services;
using System.Threading.Tasks;
using Cosei.Client.Base;

namespace Archive.ClientBase.ProfileHandler
{
	public abstract class AbstractProfileHandler : IProfileHandler
	{
		private IProfileHandler _handler = null;

		protected ISubscriber Subscriber { get; set; }

		protected IArchiveEntryService ArchiveEntryService { get; set; }
		protected IUserService UserService { get; set; }

		public IArchiveEntryService GetArchiveEntryService() => ArchiveEntryService ?? _handler?.GetArchiveEntryService();
		public IUserService GetUserService() => UserService ?? _handler?.GetUserService();

		public IProfileHandler SetNext(IProfileHandler handler) => _handler = handler;

		public async Task SetProfile(ProfileBase profile)
		{
			if (Subscriber != null)
			{
				await Subscriber.DisposeAsync();
				Subscriber = null;
			}

			ArchiveEntryService?.Dispose();
			ArchiveEntryService = null;
			UserService?.Dispose();
			UserService = null;

			await ApplyProfile(profile);

			if (_handler != null)
			{
				await _handler.SetProfile(profile);
			}
		}

		protected abstract Task ApplyProfile(ProfileBase profile);
	}
}
