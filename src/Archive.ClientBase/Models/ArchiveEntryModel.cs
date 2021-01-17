using System;
using System.Collections.Generic;
using Archive.ClientBase.Validation;
using Archive.Contracts;

namespace Archive.ClientBase.Models
{
	public class ArchiveEntryModel : AbstractValidationModel, IArchiveEntry<ArchiveGroup>
	{
		private DateTime _date;
		private string _description;
		private List<ArchiveGroup> _groups;
		private string _originalName;
		private bool _isFolder;
		private string _fileSize;

		public Guid Id { get; set; }

		public DateTime Date { get => _date; set => Set(ref _date, value); }
		public string Description { get => _description; set => Set(ref _description, value); }
		public List<ArchiveGroup> Groups { get => _groups; set => Set(ref _groups, value); }
		public string OriginalName { get => _originalName; set => Set(ref _originalName, value); }
		public bool IsFolder { get => _isFolder; set => Set(ref _isFolder, value); }
		public string FileSize { get => _fileSize; set => Set(ref _fileSize, value); }
	}
}
