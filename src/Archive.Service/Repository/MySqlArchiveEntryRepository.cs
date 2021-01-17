using System;
using System.Linq;
using Archive.Service.Model;

namespace Archive.Service.Repository
{
	public class MySqlArchiveEntryRepository : IArchiveEntryRepository, IDisposable
	{
		private readonly MySqlDbContext _context;

		public MySqlArchiveEntryRepository(MySqlDbContext context)
		{
			_context = context;
		}

		public void Delete(Guid id)
		{
			var model = _context.ArchiveEntries.FirstOrDefault(i => i.Id == id);

			if (model != null)
			{
				_context.ArchiveEntries.Remove(model);
				_context.SaveChanges();
			}
		}

		public ArchiveEntryModel Get(Guid id)
		{
			var e = _context.ArchiveEntries.FirstOrDefault(i => i.Id == id);

			if (e == null)
			{
				return null;
			}


			return e;
		}

		public void Insert(ArchiveEntryModel model)
		{

			foreach (var entry in model.Groups)
			{
				entry.Id = 0;
			}

			_context.ArchiveEntries.Add(model);
			_context.SaveChanges();
		}

		public void Update(ArchiveEntryModel model)
		{
			_context.ArchiveEntries.Update(model);
			_context.SaveChanges();
		}

		#region IDisposable Support

		private bool disposedValue = false; // To detect redundant calls

		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				disposedValue = true;

				if (disposing)
				{
					_context.Dispose();
				}
			}
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		#endregion IDisposable Support
	}
}