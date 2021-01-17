using System.Threading.Tasks;

namespace Archive.ClientBase
{
	public interface IDiskSearcher
	{
		Task<string> SelectFolderAsync();
		Task<string> SelectFileAsync();
	}
}