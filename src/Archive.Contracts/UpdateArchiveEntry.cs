using System;
using System.Collections.Generic;

namespace Archive.Contracts
{
	public class UpdateArchiveEntry
	{
		public Guid Id { get; set; }
		public DateTime Date { get; set; }
		public string Description { get; set; }
		public List<ArchiveGroup> Groups { get; set; }
		public string OriginalName { get; set; }
		public bool IsFolder { get; set; }
		public string FileSize { get; set; }

		private UpdateArchiveEntry(){}

		public UpdateArchiveEntry(
			Guid id,
			DateTime date,
			string description,
			List<ArchiveGroup> groups,
			string originalName,
			bool isFolder,
			string fileSize)
		{
			Id = id;
			Date = date;
			Description = description;
			Groups = groups;
			OriginalName = originalName;
			IsFolder = isFolder;
			FileSize = fileSize;
		}
	}
}
