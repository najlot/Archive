using System;
using Archive.Contracts;
using Archive.Service.Model;

namespace Archive.Service.Query
{
	public interface IUserQuery : IAsyncQuery<Guid, UserModel>
	{
	}
}
