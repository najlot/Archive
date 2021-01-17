using System;
using System.Collections.Generic;
using Archive.ClientBase.Models;

namespace Archive.ClientBase.Validation
{
	public class ArchiveEntryValidation : ValidationBase<ArchiveEntryModel>
	{
		public override IEnumerable<ValidationResult> Validate(ArchiveEntryModel o)
		{
			return Array.Empty<ValidationResult>();
		}
	}
}
