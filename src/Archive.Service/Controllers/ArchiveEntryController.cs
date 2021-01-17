using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Archive.Contracts;
using Archive.Service.Services;

namespace Archive.Service.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	[Authorize]
	public class ArchiveEntryController : ControllerBase
	{
		private readonly ArchiveEntryService _archiveEntryService;

		public ArchiveEntryController(ArchiveEntryService archiveEntryService)
		{
			_archiveEntryService = archiveEntryService;
		}

		[HttpGet]
		public async Task<ActionResult<List<ArchiveEntry>>> List()
		{
			var items = await _archiveEntryService.GetItemsForUserAsync(User.Identity.Name).ToListAsync();
			return Ok(items);
		}

		[HttpGet("{id}")]
		public async Task<ActionResult<ArchiveEntry>> GetItem(Guid id)
		{
			var item = await _archiveEntryService.GetItemAsync(id);
			if (item == null)
			{
				return NotFound();
			}

			return Ok(item);
		}

		[HttpPost]
		public ActionResult Create([FromBody] CreateArchiveEntry command)
		{
			_archiveEntryService.CreateArchiveEntry(command, User.Identity.Name);
			return Ok();
		}

		[HttpPut]
		public ActionResult Update([FromBody] UpdateArchiveEntry command)
		{
			_archiveEntryService.UpdateArchiveEntry(command, User.Identity.Name);
			return Ok();
		}

		[HttpDelete("{id}")]
		public ActionResult Delete(Guid id)
		{
			_archiveEntryService.DeleteArchiveEntry(id, User.Identity.Name);
			return Ok();
		}
	}
}