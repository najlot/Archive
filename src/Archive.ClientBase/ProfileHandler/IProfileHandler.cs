using Archive.ClientBase.Models;
using Archive.ClientBase.Services;
using System.Threading.Tasks;

namespace Archive.ClientBase.ProfileHandler
{
	public interface IProfileHandler
	{
		ArchiveEntryService GetArchiveEntryService();
		UserService GetUserService();

		IProfileHandler SetNext(IProfileHandler handler);

		Task SetProfile(ProfileBase profile);
	}
}
