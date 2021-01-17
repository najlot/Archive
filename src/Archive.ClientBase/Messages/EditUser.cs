using System;

namespace Archive.ClientBase.Messages
{
	public class EditUser
	{
		public Guid Id { get; }

		public EditUser(Guid id)
		{
			Id = id;
		}
	}
}