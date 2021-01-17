using System;
using System.Collections.Generic;

namespace Archive.Contracts
{
	public class ArchiveEntryDeleted
	{
		public Guid Id { get; set; }

		private ArchiveEntryDeleted(){}

		public ArchiveEntryDeleted(Guid id)
		{
			Id = id;
		}
	}
}
