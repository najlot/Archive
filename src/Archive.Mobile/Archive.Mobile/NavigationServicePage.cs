using System.Threading.Tasks;
using Archive.ClientBase;
using Archive.ClientBase.Services;
using Archive.ClientBase.ViewModel;
using Archive.Mobile.View;
using Xamarin.Forms;

namespace Archive.Mobile
{
	public class NavigationServicePage : NavigationPage, INavigationService
	{
		public NavigationServicePage(Page root) : base(root)
		{
		}

		public async Task NavigateBack()
		{
			if (Navigation.ModalStack.Count > 0)
			{
				await Navigation.PopModalAsync();
			}
			else
			{
				await Navigation.PopAsync();
			}
		}

		public async Task NavigateForward(AbstractViewModel vm)
		{
			ContentPage cp = null;
			bool isPopup = vm is IPopupViewModel;

			if (vm is LoginViewModel)
			{
				cp = new LoginView();
			}
			else if (vm is AllArchiveEntriesViewModel)
			{
				cp = new AllArchiveEntriesView();
			}
			else if (vm is AllUsersViewModel)
			{
				cp = new AllUsersView();
			}
			else if (vm is YesNoPageViewModel)
			{
				cp = new YesNoPageView();
			}
			else if (vm is ArchiveEntryViewModel)
			{
				cp = new ArchiveEntryView();
			}
			else if (vm is ArchiveGroupViewModel)
			{
				cp = new ArchiveGroupView();
			}
			else if (vm is UserViewModel)
			{
				cp = new UserView();
			}
			else if (vm is MenuViewModel)
			{
				cp = new MenuView();
			}
			else if (vm is AlertViewModel)
			{
				cp = new AlertView();
			}
			else if (vm is ProfileViewModel)
			{
				cp = new ProfileView();
			}

			cp.BindingContext = vm;

			await Task.Delay(100);

			if (isPopup)
			{
				SetHasBackButton(cp, false);
				await Navigation.PushModalAsync(cp);
			}
			else
			{
				await Navigation.PushAsync(cp);
			}
		}
	}
}