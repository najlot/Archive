using System;
using System.Threading.Tasks;
using Archive.ClientBase.Services;

namespace Archive.ClientBase.ViewModel
{
	public class MenuViewModel : AbstractViewModel
	{
		private readonly IErrorService _errorService;
		private readonly INavigationService _navigationService;
		private bool _isBusy = false;

		private readonly AllArchiveEntriesViewModel _allArchiveEntriesViewModel;
		private readonly AllUsersViewModel _allUsersViewModel;

		public AsyncCommand NavigateToArchiveEntries { get; }
		public async Task NavigateToArchiveEntriesAsync()
		{
			if (_isBusy)
			{
				return;
			}

			try
			{
				_isBusy = true;

				var refreshTask = _allArchiveEntriesViewModel.RefreshArchiveEntriesAsync();
				await _navigationService.NavigateForward(_allArchiveEntriesViewModel);
				await refreshTask;
			}
			catch (Exception ex)
			{
				await _errorService.ShowAlertAsync("Could not load...", ex);
			}
			finally
			{
				_isBusy = false;
			}
		}

		public AsyncCommand NavigateToUsers { get; }
		public async Task NavigateToUsersAsync()
		{
			if (_isBusy)
			{
				return;
			}

			try
			{
				_isBusy = true;

				var refreshTask = _allUsersViewModel.RefreshUsersAsync();
				await _navigationService.NavigateForward(_allUsersViewModel);
				await refreshTask;
			}
			catch (Exception ex)
			{
				await _errorService.ShowAlertAsync("Could not load...", ex);
			}
			finally
			{
				_isBusy = false;
			}
		}

		public MenuViewModel(IErrorService errorService,
			AllArchiveEntriesViewModel allArchiveEntriesViewModel,
			AllUsersViewModel allUsersViewModel,
			INavigationService navigationService)
		{
			_errorService = errorService;
			_allArchiveEntriesViewModel = allArchiveEntriesViewModel;
			_allUsersViewModel = allUsersViewModel;
			_navigationService = navigationService;

			NavigateToArchiveEntries = new AsyncCommand(NavigateToArchiveEntriesAsync, DisplayError);
			NavigateToUsers = new AsyncCommand(NavigateToUsersAsync, DisplayError);
		}

		private async Task DisplayError(Task task)
		{
			await _errorService.ShowAlertAsync("Error...", task.Exception);
		}
	}
}