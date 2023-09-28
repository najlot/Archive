using System;
using System.Collections.Generic;
using Archive.Service.Model;

namespace Archive.Service.Query
{
	public interface IArchiveEntryQuery : IAsyncQuery<Guid, ArchiveEntryModel>
	{
		IAsyncEnumerable<ArchiveEntryModel> GetAllOrderedByDateAsync();
	}
}
