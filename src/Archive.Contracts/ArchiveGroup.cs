using System;
using System.Collections.Generic;

namespace Archive.Contracts
{
	public class ArchiveGroup : IArchiveGroup
	{
		public int Id { get; set; }
		public string GroupName { get; set; }
	}
}
