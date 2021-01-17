using Archive.ClientBase.Models;

namespace Archive.ClientBase.Messages
{
	public class DeleteProfile
	{
		public ProfileBase Profile { get; }

		public DeleteProfile(ProfileBase profile)
		{
			Profile = profile;
		}
	}
}