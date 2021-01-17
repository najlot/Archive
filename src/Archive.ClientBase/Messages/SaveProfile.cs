using Archive.ClientBase.Models;

namespace Archive.ClientBase.Messages
{
	public class SaveProfile
	{
		public ProfileBase Profile { get; }

		public SaveProfile(ProfileBase profile)
		{
			Profile = profile;
		}
	}
}