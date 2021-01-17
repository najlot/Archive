using System;

namespace Archive.ClientBase.Messages
{
	public class EditArchiveGroup
	{
		public Guid ParentId { get; }
		public int Id { get; }

		public EditArchiveGroup(Guid parentId, int id)
		{
			ParentId = parentId;
			Id = id;
		}
	}
}