using System;
using System.Collections.Generic;

namespace Archive.Contracts
{
	public interface IArchiveGroup
	{
		int Id { get; set; }
		string GroupName { get; set; }
	}
}
