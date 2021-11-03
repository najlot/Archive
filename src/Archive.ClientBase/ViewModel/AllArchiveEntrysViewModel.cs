using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Archive.Contracts;
using Archive.ClientBase.Messages;
using Archive.ClientBase.Services;
using Archive.ClientBase.Validation;
using Archive.ClientBase.Localisation;

namespace Archive.ClientBase.ViewModel
{
	public class AllArchiveEntriesViewModel : AbstractViewModel, IDisposable
	{
		private readonly Func<ArchiveEntryViewModel> _archiveEntryViewModelFactory;
		private readonly IArchiveEntryService _archiveEntryService;
		private readonly INavigationService _navigationService;
		private readonly IMessenger _messenger;
		private readonly IErrorService _errorService;

		private bool _isBusy;
		private string _filter;

		public bool IsBusy
		{
			get => _isBusy;
			private set => Set(nameof(IsBusy), ref _isBusy, value);
		}

		public string Filter
		{
			get => _filter;
			set
			{
				Set(nameof(Filter), ref _filter, value);
				ArchiveEntriesView.Refresh();
			}
		}

		public ObservableCollectionView<ArchiveEntryViewModel> ArchiveEntriesView { get; }
		public ObservableCollection<ArchiveEntryViewModel> ArchiveEntries { get; } = new ObservableCollection<ArchiveEntryViewModel>();

		public AllArchiveEntriesViewModel(
			Func<ArchiveEntryViewModel> archiveEntryViewModelFactory,
			IErrorService errorService,
			IArchiveEntryService archiveEntryService,
			INavigationService navigationService,
			IMessenger messenger)
		{
			_archiveEntryViewModelFactory = archiveEntryViewModelFactory;
			_errorService = errorService;
			_archiveEntryService = archiveEntryService;
			_navigationService = navigationService;
			_messenger = messenger;

			ArchiveEntriesView = new ObservableCollectionView<ArchiveEntryViewModel>(ArchiveEntries, FilterArchiveEntry);

			_messenger.Register<SaveArchiveEntry>(Handle);
			_messenger.Register<EditArchiveEntry>(Handle);
			_messenger.Register<DeleteArchiveEntry>(Handle);

			_messenger.Register<ArchiveEntryCreated>(Handle);
			_messenger.Register<ArchiveEntryUpdated>(Handle);
			_messenger.Register<ArchiveEntryDeleted>(Handle);

			AddArchiveEntryCommand = new AsyncCommand(AddArchiveEntryAsync, DisplayError);
			RefreshArchiveEntriesCommand = new AsyncCommand(RefreshArchiveEntriesAsync, DisplayError);
		}

		private bool FilterArchiveEntry(ArchiveEntryViewModel arg)
		{
			if (string.IsNullOrEmpty(Filter))
			{
				return true;
			}

			var item = arg.Item;

			var date = item.Date.ToString();
			if (!string.IsNullOrEmpty(date) && date.IndexOf(Filter, StringComparison.OrdinalIgnoreCase) != -1)
			{
				return true;
			}

			var description = item.Description;
			if (!string.IsNullOrEmpty(description) && description.IndexOf(Filter, StringComparison.OrdinalIgnoreCase) != -1)
			{
				return true;
			}

			var originalName = item.OriginalName;
			if (!string.IsNullOrEmpty(originalName) && originalName.IndexOf(Filter, StringComparison.OrdinalIgnoreCase) != -1)
			{
				return true;
			}

			var isFolder = item.IsFolder.ToString();
			if (!string.IsNullOrEmpty(isFolder) && isFolder.IndexOf(Filter, StringComparison.OrdinalIgnoreCase) != -1)
			{
				return true;
			}

			var fileSize = item.FileSize;
			if (!string.IsNullOrEmpty(fileSize) && fileSize.IndexOf(Filter, StringComparison.OrdinalIgnoreCase) != -1)
			{
				return true;
			}

			return false;
		}

		private async Task DisplayError(Task task)
		{
			await _errorService.ShowAlertAsync(CommonLoc.Error, task.Exception);
		}

		private void Handle(ArchiveEntryCreated obj)
		{
			var viewModel = _archiveEntryViewModelFactory();


			viewModel.Item = new Models.ArchiveEntryModel()
			{
				Id = obj.Id,
				Date = obj.Date,
				Description = obj.Description,
				Groups = obj.Groups,
				OriginalName = obj.OriginalName,
				IsFolder = obj.IsFolder,
				FileSize = obj.FileSize,
			};

			ArchiveEntries.Insert(0, viewModel);
		}

		private void Handle(ArchiveEntryUpdated obj)
		{
			var oldItem = ArchiveEntries.FirstOrDefault(i => i.Item.Id == obj.Id);
			var index = -1;

			if (oldItem != null)
			{
				index = ArchiveEntries.IndexOf(oldItem);

				if (index != -1)
				{
					ArchiveEntries.RemoveAt(index);
				}
			}

			if (index == -1)
			{
				index = 0;
			}

			var viewModel = _archiveEntryViewModelFactory();


			viewModel.Item = new Models.ArchiveEntryModel()
			{
				Id = obj.Id,
				Date = obj.Date,
				Description = obj.Description,
				Groups = obj.Groups,
				OriginalName = obj.OriginalName,
				IsFolder = obj.IsFolder,
				FileSize = obj.FileSize,
			};

			ArchiveEntries.Insert(index, viewModel);
		}

		private void Handle(ArchiveEntryDeleted obj)
		{
			var oldItem = ArchiveEntries.FirstOrDefault(i => i.Item.Id == obj.Id);

			if (oldItem != null)
			{
				ArchiveEntries.Remove(oldItem);
			}
		}

		private async Task Handle(DeleteArchiveEntry obj)
		{
			try
			{
				await _archiveEntryService.DeleteItemAsync(obj.Id);
			}
			catch (Exception ex)
			{
				await _errorService.ShowAlertAsync("Error saving...", ex);
			}
		}

		private async Task Handle(EditArchiveEntry obj)
		{
			if (IsBusy)
			{
				return;
			}

			try
			{
				IsBusy = true;

				var item = await _archiveEntryService.GetItemAsync(obj.Id);

				// Prevalidate
				item.SetValidation(new ArchiveEntryValidationList(), true);

				var viewModel = _archiveEntryViewModelFactory();


				viewModel.Item = item;

				_messenger.Register<EditArchiveGroup>(viewModel.Handle);
				_messenger.Register<DeleteArchiveGroup>(viewModel.Handle);
				_messenger.Register<SaveArchiveGroup>(viewModel.Handle);

				_messenger.Register<ArchiveEntryUpdated>(viewModel.Handle);

				await _navigationService.NavigateForward(viewModel);
			}
			catch (Exception ex)
			{
				await _errorService.ShowAlertAsync("Error loading...", ex);
			}
			finally
			{
				IsBusy = false;
			}
		}

		private async Task Handle(SaveArchiveEntry obj)
		{
			try
			{
				if (ArchiveEntries.Any(i => i.Item.Id == obj.Item.Id))
				{
					await _archiveEntryService.UpdateItemAsync(obj.Item);
				}
				else
				{
					await _archiveEntryService.AddItemAsync(obj.Item);
				}
			}
			catch (Exception ex)
			{
				await _errorService.ShowAlertAsync("Error saving...", ex);
			}
		}

		public AsyncCommand AddArchiveEntryCommand { get; }
		public async Task AddArchiveEntryAsync()
		{
			if (IsBusy)
			{
				return;
			}

			try
			{
				IsBusy = true;

				var item = _archiveEntryService.CreateArchiveEntry();

				// Prevalidate
				item.SetValidation(new ArchiveEntryValidationList(), true);

				var viewModel = _archiveEntryViewModelFactory();


				viewModel.Item = item;

				_messenger.Register<EditArchiveGroup>(viewModel.Handle);
				_messenger.Register<DeleteArchiveGroup>(viewModel.Handle);
				_messenger.Register<SaveArchiveGroup>(viewModel.Handle);

				await _navigationService.NavigateForward(viewModel);
			}
			catch (Exception ex)
			{
				await _errorService.ShowAlertAsync("Error adding...", ex);
			}
			finally
			{
				IsBusy = false;
			}
		}

		public AsyncCommand RefreshArchiveEntriesCommand { get; }
		public async Task RefreshArchiveEntriesAsync()
		{
			if (IsBusy)
			{
				return;
			}

			try
			{
				IsBusy = true;
				ArchiveEntriesView.Disable();
				Filter = "";

				ArchiveEntries.Clear();

				var archiveEntries = await _archiveEntryService.GetItemsAsync(true);

				foreach (var item in archiveEntries)
				{
					var viewModel = _archiveEntryViewModelFactory();


					viewModel.Item = item;

					ArchiveEntries.Add(viewModel);
				}
			}
			catch (Exception ex)
			{
				await _errorService.ShowAlertAsync("Error loading data...", ex);
			}
			finally
			{
				ArchiveEntriesView.Enable();
				IsBusy = false;
			}
		}

		#region IDisposable Support

		private bool disposedValue = false; // To detect redundant calls

		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				disposedValue = true;

				if (disposing)
				{
					_messenger.Unregister(this);
				}
			}
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		#endregion IDisposable Support
	}
}