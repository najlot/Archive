using System.Threading.Tasks;

namespace Archive.ClientBase.Services
{
	public interface INavigationService
	{
		Task NavigateBack();

		Task NavigateForward(AbstractViewModel newViewModel);
	}
}