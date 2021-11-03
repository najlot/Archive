using System.Collections.Generic;
using System.Threading.Tasks;
using Archive.ClientBase.Models;

namespace Archive.ClientBase.Services
{
	public interface IProfilesService
	{
		Task<List<ProfileBase>> LoadAsync();
		Task RemoveAsync(ProfileBase profile);
		Task SaveAsync(List<ProfileBase> profiles);
	}
}
