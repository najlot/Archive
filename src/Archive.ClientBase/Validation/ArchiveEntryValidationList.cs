using System.Collections.Generic;
using Archive.ClientBase.Models;

namespace Archive.ClientBase.Validation
{
	public class ArchiveEntryValidationList : ValidationList<ArchiveEntryModel>
	{
		public ArchiveEntryValidationList()
		{
			Add(new ArchiveEntryValidation());
		}
	}
}
