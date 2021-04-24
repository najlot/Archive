using Cosei.Client.Base;
using System.Threading.Tasks;

namespace Archive.ClientBase.ProfileHandler
{
	public interface ILocalSubscriber : ISubscriber
	{
		Task SendAsync<T>(T message) where T : class;
	}
}
