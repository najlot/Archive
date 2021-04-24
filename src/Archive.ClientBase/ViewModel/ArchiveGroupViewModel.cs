using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Archive.ClientBase.Messages;
using Archive.ClientBase.Models;
using Archive.ClientBase.Services;
using Archive.ClientBase.Validation;
using Archive.Contracts;

namespace Archive.ClientBase.ViewModel
{
	public class ArchiveGroupViewModel : AbstractViewModel
	{
		private bool _isBusy;
		private ArchiveGroupModel _item;

		private readonly IErrorService _errorService;
		private readonly INavigationService _navigationService;
		private readonly IMessenger _messenger;
		private readonly Guid _parentId;

		public ArchiveGroupModel Item { get => _item; private set => Set(nameof(Item), ref _item, value); }
		public bool IsBusy { get => _isBusy; private set => Set(nameof(IsBusy), ref _isBusy, value); }

		public ArchiveGroupViewModel(
			IErrorService errorService,
			ArchiveGroupModel archiveGroupModel,
			INavigationService navigationService,
			IMessenger messenger,
			Guid parentId)
		{
			Item = archiveGroupModel;
			_errorService = errorService;
			_navigationService = navigationService;
			_messenger = messenger;
			_parentId = parentId;

			SaveCommand = new AsyncCommand(SaveAsync, DisplayError);
			DeleteCommand = new AsyncCommand<bool>(DeleteAsync, DisplayError);
			EditArchiveGroupCommand = new AsyncCommand(EditArchiveGroupAsync, DisplayError, () => !IsBusy);
		}

		private async Task DisplayError(Task task)
		{
			await _errorService.ShowAlertAsync("Error...", task.Exception);
		}

		public AsyncCommand SaveCommand { get; }
		public async Task SaveAsync()
		{
			if (IsBusy)
			{
				return;
			}

			try
			{
				IsBusy = true;

				var errors = Item.Errors
					.Where(err => err.Severity > ValidationSeverity.Info)
					.Select(e => e.Text);

				if (errors.Any())
				{
					var message = "There are some validation errors:";
					message += Environment.NewLine + Environment.NewLine;
					message += string.Join(Environment.NewLine, errors);
					message += Environment.NewLine + Environment.NewLine;
					message += "Do you want to continue?";

					var vm = new YesNoPageViewModel()
					{
						Title = "Validation",
						Message = message
					};

					var selection = await _navigationService.RequestInputAsync(vm);

					if (!selection)
					{
						return;
					}
				}

				await _navigationService.NavigateBack();
				await _messenger.SendAsync(new SaveArchiveGroup(_parentId, Item));
			}
			catch (Exception ex)
			{
				await _errorService.ShowAlertAsync("Error saving...", ex);
			}
			finally
			{
				IsBusy = false;
			}
		}

		public AsyncCommand<bool> DeleteCommand { get; }
		public async Task DeleteAsync(bool navBack)
		{
			if (IsBusy)
			{
				return;
			}

			try
			{
				IsBusy = true;

				var vm = new YesNoPageViewModel()
				{
					Title = "Delete?",
					Message = "Should the item be deleted?"
				};

				var selection = await _navigationService.RequestInputAsync(vm);

				if (selection)
				{
					if (navBack)
					{
						await _navigationService.NavigateBack();
					}

					await _messenger.SendAsync(new DeleteArchiveGroup(_parentId, Item.Id));
				}
			}
			catch (Exception ex)
			{
				await _errorService.ShowAlertAsync("Error deleting...", ex);
			}
			finally
			{
				IsBusy = false;
			}
		}

		public AsyncCommand EditArchiveGroupCommand { get; }
		public async Task EditArchiveGroupAsync()
		{
			if (IsBusy)
			{
				return;
			}

			try
			{
				IsBusy = true;
				await _messenger.SendAsync(new EditArchiveGroup(_parentId, Item.Id));
			}
			catch (Exception ex)
			{
				await _errorService.ShowAlertAsync("Error starting edit...", ex);
			}
			finally
			{
				IsBusy = false;
			}
		}
	}
}