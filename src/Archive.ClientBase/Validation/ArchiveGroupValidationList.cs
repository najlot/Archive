using System.Collections.Generic;
using Archive.ClientBase.Models;

namespace Archive.ClientBase.Validation
{
	public class ArchiveGroupValidationList : ValidationList<ArchiveGroupModel>
	{
		public ArchiveGroupValidationList()
		{
			Add(new ArchiveGroupValidation());
		}
	}
}
