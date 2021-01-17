using System;
using Archive.ClientBase.Models;
using Archive.Contracts;

namespace Archive.ClientBase.Messages
{
	public class SaveUser
	{
		public UserModel Item { get; }

		public SaveUser(UserModel item)
		{
			Item = item;
		}
	}
}