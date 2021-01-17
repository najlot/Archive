using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Archive.Contracts;
using Archive.Service.Services;

namespace Archive.Service.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class AuthController : ControllerBase
	{
		private readonly TokenService _tokenService;

		public AuthController(TokenService tokenService)
		{
			_tokenService = tokenService;
		}

		[AllowAnonymous]
		[HttpPost()]
		public ActionResult<string> Auth([FromBody] AuthRequest request)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest();
			}

			var token = _tokenService.GetToken(request.Username, request.Password);

			if (token == null)
			{
				return Forbid();
			}

			return Ok(token);
		}
	}
}