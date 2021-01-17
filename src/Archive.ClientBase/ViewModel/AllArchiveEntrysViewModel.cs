using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Archive.Contracts;
using Archive.ClientBase.Messages;
using Archive.ClientBase.Services;
using Archive.ClientBase.Validation;

namespace Archive.ClientBase.ViewModel
{
	public class AllArchiveEntriesViewModel : AbstractViewModel, IDisposable
	{
		private readonly ArchiveEntryService _archiveEntryService;
		private readonly INavigationService _navigationService;
		private readonly Messenger _messenger;
		private readonly ErrorService _errorService;

		private bool _isBusy;
		private ObservableCollection<ArchiveEntryViewModel> _archiveEntries = new ObservableCollection<ArchiveEntryViewModel>();

		public bool IsBusy
		{
			get => _isBusy;
			private set => Set(nameof(IsBusy), ref _isBusy, value);
		}

		public ObservableCollection<ArchiveEntryViewModel> ArchiveEntries
		{
			get => _archiveEntries;
			private set => Set(nameof(ArchiveEntries), ref _archiveEntries, value);
		}

		public AllArchiveEntriesViewModel(ErrorService errorService,
			ArchiveEntryService archiveEntryService,
			INavigationService navigationService,
			Messenger messenger)
		{
			_errorService = errorService;
			_archiveEntryService = archiveEntryService;
			_navigationService = navigationService;
			_messenger = messenger;

			_messenger.Register<SaveArchiveEntry>(Handle);
			_messenger.Register<EditArchiveEntry>(Handle);
			_messenger.Register<DeleteArchiveEntry>(Handle);

			_messenger.Register<ArchiveEntryCreated>(Handle);
			_messenger.Register<ArchiveEntryUpdated>(Handle);
			_messenger.Register<ArchiveEntryDeleted>(Handle);

			AddArchiveEntryCommand = new AsyncCommand(AddArchiveEntryAsync, DisplayError);
			RefreshArchiveEntriesCommand = new AsyncCommand(RefreshArchiveEntriesAsync, DisplayError);
		}

		private async Task DisplayError(Task task)
		{
			await _errorService.ShowAlert("Error...", task.Exception);
		}

		private void Handle(ArchiveEntryCreated obj)
		{

			ArchiveEntries.Insert(0, new ArchiveEntryViewModel(
				_errorService,
				new Models.ArchiveEntryModel()
				{
					Id = obj.Id,
					Date = obj.Date,
					Description = obj.Description,
					Groups = obj.Groups,
					OriginalName = obj.OriginalName,
					IsFolder = obj.IsFolder,
					FileSize = obj.FileSize,
				},
				_navigationService,
				_messenger));
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


			ArchiveEntries.Insert(index, new ArchiveEntryViewModel(
				_errorService,
				new Models.ArchiveEntryModel()
				{
					Id = obj.Id,
					Date = obj.Date,
					Description = obj.Description,
					Groups = obj.Groups,
					OriginalName = obj.OriginalName,
					IsFolder = obj.IsFolder,
					FileSize = obj.FileSize,
				},
				_navigationService,
				_messenger));
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
				await _errorService.ShowAlert("Error saving...", ex);
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


				var vm = new ArchiveEntryViewModel(
					_errorService,
					item,
					_navigationService,
					_messenger);

				_messenger.Register<EditArchiveGroup>(vm.Handle);
				_messenger.Register<DeleteArchiveGroup>(vm.Handle);
				_messenger.Register<SaveArchiveGroup>(vm.Handle);

				_messenger.Register<ArchiveEntryUpdated>(vm.Handle);

				await _navigationService.NavigateForward(vm);
			}
			catch (Exception ex)
			{
				await _errorService.ShowAlert("Error loading...", ex);
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
				await _errorService.ShowAlert("Error saving...", ex);
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

				var itemVm = new ArchiveEntryViewModel(
					_errorService,
					item,
					_navigationService,
					_messenger);

				_messenger.Register<EditArchiveGroup>(itemVm.Handle);
				_messenger.Register<DeleteArchiveGroup>(itemVm.Handle);
				_messenger.Register<SaveArchiveGroup>(itemVm.Handle);

				await _navigationService.NavigateForward(itemVm);
			}
			catch (Exception ex)
			{
				await _errorService.ShowAlert("Error adding...", ex);
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
				ArchiveEntries.Clear();

				var archiveEntries = await _archiveEntryService.GetItemsAsync(true);

				ArchiveEntries = new ObservableCollection<ArchiveEntryViewModel>(archiveEntries
					.Select(item => new ArchiveEntryViewModel(
						_errorService,
						item,
						_navigationService,
						_messenger)));
			}
			catch (Exception ex)
			{
				await _errorService.ShowAlert("Error loading data...", ex);
			}
			finally
			{
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