using Archive.ClientBase.Models;

namespace Archive.ClientBase.Messages
{
	public class LoginProfile
	{
		public ProfileBase Profile { get; }

		public LoginProfile(ProfileBase profile)
		{
			Profile = profile;
		}
	}
}