using System;
using Archive.Service.Model;

namespace Archive.Service.Repository
{
	public interface IArchiveEntryRepository : IRepository<Guid, ArchiveEntryModel>
	{
	}
}