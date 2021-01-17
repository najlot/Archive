using System;
using System.Collections.Generic;
using Archive.ClientBase.Validation;
using Archive.Contracts;

namespace Archive.ClientBase.Models
{
	public class ArchiveGroupModel : AbstractValidationModel, IArchiveGroup
	{
		private string _groupName;

		public int Id { get; set; }

		public string GroupName { get => _groupName; set => Set(ref _groupName, value); }
	}
}
