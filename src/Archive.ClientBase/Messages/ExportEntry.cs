using System;

namespace Archive.ClientBase.Messages
{
	public class ExportEntry
	{
		public Guid Id { get; }
		public bool IsFolder { get; }
		public string DestinationPath { get; }

		public ExportEntry(Guid id, bool isFolder, string destinationPath)
		{
			Id = id;
			IsFolder = isFolder;
			DestinationPath = destinationPath;
		}
	}
}