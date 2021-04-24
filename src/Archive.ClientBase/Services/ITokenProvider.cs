using System.Threading.Tasks;

namespace Archive.ClientBase.Services
{
	public interface ITokenProvider
	{
		Task<string> GetToken();
	}
}
