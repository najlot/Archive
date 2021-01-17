using System;

namespace Archive.ClientBase.Messages
{
	public class EditArchiveEntry
	{
		public Guid Id { get; }

		public EditArchiveEntry(Guid id)
		{
			Id = id;
		}
	}
}