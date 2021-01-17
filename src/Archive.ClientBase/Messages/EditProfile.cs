using Archive.ClientBase.Models;

namespace Archive.ClientBase.Messages
{
	public class EditProfile
	{
		public ProfileBase Profile { get; }

		public EditProfile(ProfileBase profile)
		{
			Profile = profile;
		}
	}
}