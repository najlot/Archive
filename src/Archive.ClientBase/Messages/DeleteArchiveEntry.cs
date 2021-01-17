using System;

namespace Archive.ClientBase.Messages
{
	public class DeleteArchiveEntry
	{
		public Guid Id { get; }

		public DeleteArchiveEntry(Guid id)
		{
			Id = id;
		}
	}
}