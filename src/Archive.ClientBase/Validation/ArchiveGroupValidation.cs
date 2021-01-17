using System;
using System.Collections.Generic;
using Archive.ClientBase.Models;

namespace Archive.ClientBase.Validation
{
	public class ArchiveGroupValidation : ValidationBase<ArchiveGroupModel>
	{
		public override IEnumerable<ValidationResult> Validate(ArchiveGroupModel o)
		{
			return Array.Empty<ValidationResult>();
		}
	}
}
