using System.Collections.Generic;
using Archive.ClientBase.Models;

namespace Archive.ClientBase.Validation
{
	public class UserValidationList : ValidationList<UserModel>
	{
		public UserValidationList()
		{
			Add(new UserValidation());
		}
	}
}
