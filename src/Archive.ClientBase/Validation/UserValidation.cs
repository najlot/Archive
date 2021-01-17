using System;
using System.Collections.Generic;
using Archive.ClientBase.Models;

namespace Archive.ClientBase.Validation
{
	public class UserValidation : ValidationBase<UserModel>
	{
		public override IEnumerable<ValidationResult> Validate(UserModel o)
		{
			return Array.Empty<ValidationResult>();
		}
	}
}
