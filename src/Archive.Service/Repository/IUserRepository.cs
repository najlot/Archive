using System;
using Archive.Service.Model;

namespace Archive.Service.Repository
{
	public interface IUserRepository : IRepository<Guid, UserModel>
	{
		UserModel Get(string username);
	}
}