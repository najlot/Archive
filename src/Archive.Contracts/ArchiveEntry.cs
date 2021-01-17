using System;
using System.Collections.Generic;

namespace Archive.Contracts
{
	public class ArchiveEntry : IArchiveEntry<ArchiveGroup>
	{
		public Guid Id { get; set; }
		public DateTime Date { get; set; }
		public string Description { get; set; }
		public List<ArchiveGroup> Groups { get; set; }
		public string OriginalName { get; set; }
		public bool IsFolder { get; set; }
		public string FileSize { get; set; }
	}
}
