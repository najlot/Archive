using System;
using System.Collections.Generic;

namespace Archive.Contracts
{
	public interface IArchiveEntry<TArchiveGroup>
		where TArchiveGroup : IArchiveGroup
	{
		Guid Id { get; set; }
		DateTime Date { get; set; }
		string Description { get; set; }
		List<TArchiveGroup> Groups { get; set; }
		string OriginalName { get; set; }
		bool IsFolder { get; set; }
		string FileSize { get; set; }
	}
}
