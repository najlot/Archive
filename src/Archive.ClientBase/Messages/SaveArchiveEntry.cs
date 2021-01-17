using Archive.ClientBase.Models;
using Archive.Contracts;

namespace Archive.ClientBase.Messages
{
	public class SaveArchiveEntry
	{
		public ArchiveEntryModel Item { get; }
		public string Path { get; }

		public SaveArchiveEntry(ArchiveEntryModel item, string path)
		{
			Item = item;
			Path = path;
		}
	}
}