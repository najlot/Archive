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
	public partial class ArchiveEntryViewModel
	{
		private ObservableCollection<ArchiveGroupViewModel> _groups = new ObservableCollection<ArchiveGroupViewModel>();
		public ObservableCollection<ArchiveGroupViewModel> Groups { get => _groups; set => Set(nameof(Groups), ref _groups, value); }

		public RelayCommand AddArchiveGroupCommand => new RelayCommand(() =>
		{
			var max = 0;

			if (Groups.Count > 0)
			{
				max = Groups.Max(e => e.Item.Id) + 1;
			}

			var model = new ArchiveGroupModel() { Id = max };

			var vm = new ArchiveGroupViewModel(_errorService, model, _navigationService, _messenger, Item.Id);

			Groups.Add(vm);
		});

		public async Task Handle(DeleteArchiveGroup obj)
		{
			if (Item.Id != obj.ParentId)
			{
				return;
			}

			try
			{
				var oldItem = Groups.FirstOrDefault(i => i.Item.Id == obj.Id);

				if (oldItem != null)
				{
					var index = Groups.IndexOf(oldItem);

					if (index != -1)
					{
						Groups.RemoveAt(index);
					}
				}
			}
			catch (Exception ex)
			{
				await _errorService.ShowAlert("Error saving...", ex);
			}
		}

		public async Task Handle(EditArchiveGroup obj)
		{
			if (IsBusy)
			{
				return;
			}

			if (Item.Id != obj.ParentId)
			{
				return;
			}

			try
			{
				IsBusy = true;

				var vm = Groups.FirstOrDefault(e => e.Item.Id == obj.Id);

				// Prevalidate
				vm.Item.SetValidation(new ArchiveGroupValidationList(), true);

				vm = new ArchiveGroupViewModel(
					_errorService,
					vm.Item,
					_navigationService,
					_messenger,
					Item.Id);

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

		public async Task Handle(SaveArchiveGroup obj)
		{
			if (Item.Id != obj.ParentId)
			{
				return;
			}

			try
			{
				int index = -1;
				var oldItem = Groups.FirstOrDefault(i => i.Item.Id == obj.Item.Id);

				if (oldItem != null)
				{
					index = Groups.IndexOf(oldItem);

					if (index != -1)
					{
						Groups.RemoveAt(index);
					}
				}

				if (index == -1)
				{
					Groups.Insert(0, new ArchiveGroupViewModel(
						_errorService,
						obj.Item,
						_navigationService,
						_messenger,
						Item.Id));
				}
				else
				{
					Groups.Insert(index, new ArchiveGroupViewModel(
						_errorService,
						obj.Item,
						_navigationService,
						_messenger,
						Item.Id));
				}
			}
			catch (Exception ex)
			{
				await _errorService.ShowAlert("Error saving...", ex);
			}
		}
	}
}
