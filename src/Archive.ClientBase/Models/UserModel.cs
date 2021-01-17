using System;
using System.Collections.Generic;
using Archive.ClientBase.Validation;
using Archive.Contracts;

namespace Archive.ClientBase.Models
{
	public class UserModel : AbstractValidationModel, IUser
	{
		private string _username;
		private string _eMail;
		private string _password;

		public Guid Id { get; set; }

		public string Username { get => _username; set => Set(ref _username, value); }
		public string EMail { get => _eMail; set => Set(ref _eMail, value); }
		public string Password { get => _password; set => Set(ref _password, value); }
	}
}
