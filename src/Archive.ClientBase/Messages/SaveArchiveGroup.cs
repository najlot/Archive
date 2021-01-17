using System;
using Archive.ClientBase.Models;
using Archive.Contracts;

namespace Archive.ClientBase.Messages
{
	public class SaveArchiveGroup
	{
		public Guid ParentId { get; }
		public ArchiveGroupModel Item { get; }

		public SaveArchiveGroup(Guid parentId, ArchiveGroupModel item)
		{
			ParentId = parentId;
			Item = item;
		}
	}
}