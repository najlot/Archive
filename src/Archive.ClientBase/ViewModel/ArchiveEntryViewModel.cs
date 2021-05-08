using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Archive.ClientBase.Messages;
using Archive.ClientBase.Models;
using Archive.ClientBase.Services;
using Archive.ClientBase.Validation;
using Archive.Contracts;

namespace Archive.ClientBase.ViewModel
{
	public partial class ArchiveEntryViewModel : AbstractViewModel
	{
		private bool _isBusy;
		private ArchiveEntryModel _item;
		private string _path;
		private string _groupToAdd;


		private readonly Func<ArchiveGroupViewModel> _archiveGroupViewModelFactory;
		private readonly IErrorService _errorService;
		private readonly INavigationService _navigationService;
		private readonly IDiskSearcher _diskSearcher;
		private readonly IMessenger _messenger;

		public ArchiveEntryModel Item
		{
			get => _item;
			set
			{
				Set(nameof(Item), ref _item, value);

				if (Item.Groups == null)
				{
					Groups = new ObservableCollection<ArchiveGroupViewModel>();
				}
				else
				{
					Groups = new ObservableCollection<ArchiveGroupViewModel>(Item.Groups.Select(e =>
					{
						var model = new ArchiveGroupModel()
						{
							Id = e.Id,
							GroupName = e.GroupName,
						};

						var viewModel = _archiveGroupViewModelFactory();
						viewModel.ParentId = Item.Id;
						viewModel.Item = model;
						return viewModel;
					}));
				}

				Path = Item.OriginalName;
			}
		}

		public bool IsBusy { get => _isBusy; private set => Set(nameof(IsBusy), ref _isBusy, value); }

		public ObservableCollection<string> AvailableGroups { get; } = new ObservableCollection<string>();
		public string Path { get => _path; set => Set(nameof(Path), ref _path, value); }
		public string GroupToAdd { get => _groupToAdd; set => Set(nameof(GroupToAdd), ref _groupToAdd, value); }

		public ArchiveEntryViewModel(
			Func<ArchiveGroupViewModel> archiveGroupViewModelFactory,
			IErrorService errorService,
			INavigationService navigationService,
			IDiskSearcher diskSearcher,
			IMessenger messenger)
		{
			_archiveGroupViewModelFactory = archiveGroupViewModelFactory;

			_errorService = errorService;
			_navigationService = navigationService;
			_messenger = messenger;
			_diskSearcher = diskSearcher;

			SaveCommand = new AsyncCommand(SaveAsync, DisplayError);
			DeleteCommand = new AsyncCommand(DeleteAsync, DisplayError);
			EditArchiveEntryCommand = new AsyncCommand(EditArchiveEntryAsync, DisplayError, () => !IsBusy);

			SelectFileCommand = new AsyncCommand(SelectFileAsync, DisplayError);
			SelectFolderCommand = new AsyncCommand(SelectFolderAsync, DisplayError);

			AddGroupCommand = new AsyncCommand(AddGroupAsync, DisplayError);
			RemoveGroupCommand = new AsyncCommand<string>(RemoveGroupAsync, DisplayError);
			ExportCommand = new AsyncCommand(ExportAsync, DisplayError);
		}

		private async Task DisplayError(Task task)
		{
			await _errorService.ShowAlertAsync("Error...", task.Exception);
		}

		public bool _canExport = true;

		public AsyncCommand ExportCommand { get; }
		private async Task ExportAsync()
		{
			_canExport = false;
			ExportCommand.RaiseCanExecuteChanged();

			try
			{
				var folder = await _diskSearcher.SelectFolderAsync();

				if (!string.IsNullOrWhiteSpace(folder))
				{
					var destinationPath = System.IO.Path.Combine(folder, Item.OriginalName);
					await _messenger.SendAsync(new ExportEntry(Item.Id, Item.IsFolder, destinationPath));
				}
			}
			finally
			{
				_canExport = true;
				ExportCommand.RaiseCanExecuteChanged();
			}
		}

		public AsyncCommand AddGroupCommand { get; }
		private Task AddGroupAsync()
		{
			if (!string.IsNullOrWhiteSpace(GroupToAdd) && Groups.All(g => g.Item.GroupName != GroupToAdd))
			{
				var viewModel = _archiveGroupViewModelFactory();

				viewModel.ParentId = Item.Id;
				viewModel.Item = new ArchiveGroupModel()
				{
					Id = Groups.Count + 1,
					GroupName = GroupToAdd
				};

				Groups.Add(viewModel);
			}

			GroupToAdd = "";

			return Task.CompletedTask;
		}

		public AsyncCommand<string> RemoveGroupCommand { get; }
		private Task RemoveGroupAsync(string name)
		{
			if (!string.IsNullOrWhiteSpace(name))
			{
				var item = Groups.FirstOrDefault(i => i.Item.GroupName == name);

				if (item != null)
				{
					Groups.Remove(item);
				}
			}

			GroupToAdd = "";

			return Task.CompletedTask;
		}

		public AsyncCommand SelectFileCommand { get; }
		private async Task SelectFileAsync()
		{
			var path = await _diskSearcher.SelectFileAsync();

			if (!string.IsNullOrWhiteSpace(path))
			{
				Path = path;
			}
		}

		public AsyncCommand SelectFolderCommand { get; }
		private async Task SelectFolderAsync()
		{
			var path = await _diskSearcher.SelectFolderAsync();

			if (!string.IsNullOrWhiteSpace(path))
			{
				Path = path;
			}
		}

		public void Handle(ArchiveEntryUpdated obj)
		{
			if (Item.Id != obj.Id)
			{
				return;
			}

			Item = new ArchiveEntryModel()
			{
				Id = obj.Id,
				Date = obj.Date,
				Description = obj.Description,
				Groups = obj.Groups,
				OriginalName = obj.OriginalName,
				IsFolder = obj.IsFolder,
				FileSize = obj.FileSize,
			};

			Groups = new ObservableCollection<ArchiveGroupViewModel>(Item.Groups.Select(e =>
			{
				var model = new ArchiveGroupModel()
				{
					Id = e.Id,
					GroupName = e.GroupName,
				};

				var viewModel = _archiveGroupViewModelFactory();
				viewModel.ParentId = Item.Id;
				viewModel.Item = model;
				return viewModel;
			}));
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

				Item.Groups = Groups.Select(e => new ArchiveGroup()
				{
					Id = e.Item.Id,
					GroupName = e.Item.GroupName,
				}).ToList();

				if (!string.IsNullOrWhiteSpace(GroupToAdd))
				{
					Item.Groups.Add(new ArchiveGroup()
					{
						Id = Item.Groups.Count + 1,
						GroupName = GroupToAdd,
					});
				}

				var errors = Item.Errors
					.Where(err => err.Severity > ValidationSeverity.Info)
					.Select(e => e.Text);

				Item.IsFolder = Directory.Exists(Path);
				Item.OriginalName = System.IO.Path.GetFileName(Path);
				
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
				await _messenger.SendAsync(new SaveArchiveEntry(Item, Path));
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

		public AsyncCommand DeleteCommand { get; }
		public async Task DeleteAsync()
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
					await _navigationService.NavigateBack();
					await _messenger.SendAsync(new DeleteArchiveEntry(Item.Id));
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

		public AsyncCommand EditArchiveEntryCommand { get; }
		public async Task EditArchiveEntryAsync()
		{
			if (IsBusy)
			{
				return;
			}

			try
			{
				IsBusy = true;
				await _messenger.SendAsync(new EditArchiveEntry(Item.Id));
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