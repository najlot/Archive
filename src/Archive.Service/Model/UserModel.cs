using MongoDB.Bson.Serialization.Attributes;
using Archive.Contracts;
using System;
using System.Collections.Generic;

namespace Archive.Service.Model
{
	[BsonIgnoreExtraElements]
	public class UserModel
	{
		[BsonId]
		public Guid Id { get; set; }
		public string Username { get; set; }
		public string EMail { get; set; }
		public byte[] PasswordHash { get; set; }
		public bool IsActive { get; set; }
	}
}
