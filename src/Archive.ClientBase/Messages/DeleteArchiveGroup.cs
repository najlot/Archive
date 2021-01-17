using System;

namespace Archive.ClientBase.Messages
{
	public class DeleteArchiveGroup
	{
		public Guid ParentId { get; }
		public int Id { get; }

		public DeleteArchiveGroup(Guid parentId, int id)
		{
			ParentId = parentId;
			Id = id;
		}
	}
}