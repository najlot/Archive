using Archive.ClientBase.Models;
using Archive.ClientBase.Services;
using System.Threading.Tasks;

namespace Archive.ClientBase.ProfileHandler
{
	public interface IProfileHandler
	{
		IArchiveEntryService GetArchiveEntryService();
		IUserService GetUserService();

		IProfileHandler SetNext(IProfileHandler handler);

		Task SetProfile(ProfileBase profile);
	}
}
