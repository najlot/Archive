using System.Collections.Generic;
using Archive.ClientBase.Models;

namespace Archive.ClientBase.Services
{
	public interface IProfilesService
	{
		List<ProfileBase> Load();
		void Remove(ProfileBase profile);
		void Save(List<ProfileBase> profiles);
	}
}
