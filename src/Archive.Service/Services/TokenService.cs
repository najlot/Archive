﻿using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Archive.Service.Configuration;
using System.Security.Cryptography;
using System.Linq;

namespace Archive.Service.Services
{
	public class TokenService
	{
		private readonly UserService _userService;
		private readonly ServiceConfiguration _serviceConfiguration;

		public TokenService(UserService userService,
			ServiceConfiguration serviceConfiguration)
		{
			_userService = userService;
			_serviceConfiguration = serviceConfiguration;
		}

		public static TokenValidationParameters GetValidationParameters(string secret)
		{
			var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));

			return new TokenValidationParameters
			{
				ValidateLifetime = true,
				LifetimeValidator = (before, expires, token, param) =>
				{
					return expires > DateTime.UtcNow;
				},
				ValidateAudience = false,
				ValidateIssuer = false,
				ValidateActor = false,
				ValidateIssuerSigningKey = true,
				IssuerSigningKey = key
			};
		}

		public TokenValidationParameters GetValidationParameters()
		{
			return GetValidationParameters(_serviceConfiguration.Secret);
		}

		public string GetToken(string username, string password)
		{
			var user = _userService.GetUserModelFromName(username);

			if (user == null)
			{
				return null;
			}

			using (var sha = SHA256.Create())
			{
				var bytes = Encoding.UTF8.GetBytes(password);
				var userUasswordHash = sha.ComputeHash(bytes);

				if (!Enumerable.SequenceEqual(user.PasswordHash, userUasswordHash))
				{
					return null;
				}
			}

			var claim = new[]
			{
				new Claim(ClaimTypes.Name, username)
			};

			var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_serviceConfiguration.Secret));
			var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature);

			var jwtToken = new JwtSecurityToken(
				issuer: "Archive.Service",
				audience: "Archive.Service",
				claims: claim,
				expires: DateTime.UtcNow.AddMinutes(120),
				signingCredentials: credentials
			);

			var token = new JwtSecurityTokenHandler().WriteToken(jwtToken);

			return token;
		}
	}
}