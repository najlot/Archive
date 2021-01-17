using System;

namespace Archive.ClientBase.Messages
{
	public class DeleteUser
	{
		public Guid Id { get; }

		public DeleteUser(Guid id)
		{
			Id = id;
		}
	}
}