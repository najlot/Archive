using Archive.ClientBase.Models;
using Archive.ClientBase.Services;
using System.Threading.Tasks;

namespace Archive.ClientBase.ProfileHandler
{
	public abstract class AbstractProfileHandler : IProfileHandler
	{
		private IProfileHandler _handler = null;

		protected ArchiveEntryService ArchiveEntryService { get; set; }
		protected UserService UserService { get; set; }

		public ArchiveEntryService GetArchiveEntryService() => ArchiveEntryService ?? _handler?.GetArchiveEntryService();
		public UserService GetUserService() => UserService ?? _handler?.GetUserService();

		public IProfileHandler SetNext(IProfileHandler handler) => _handler = handler;

		public async Task SetProfile(ProfileBase profile)
		{
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
