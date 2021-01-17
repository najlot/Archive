using System;
using Archive.ClientBase.Models;
using Archive.Contracts;

namespace Archive.ClientBase.Messages
{
	public class SaveArchiveEntry
	{
		public ArchiveEntryModel Item { get; }

		public SaveArchiveEntry(ArchiveEntryModel item)
		{
			Item = item;
		}
	}
}