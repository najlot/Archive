﻿using Microsoft.Extensions.DependencyInjection;
using System;
using Archive.ClientBase;
using Archive.ClientBase.ProfileHandler;
using Archive.ClientBase.Services;
using Archive.ClientBase.ViewModel;

namespace Archive.Wpf.ViewModel
{
	public class ViewModelLocator
	{
		/// <summary>
		/// Initializes a new instance of the ViewModelLocator class.
		/// </summary>
		public ViewModelLocator()
		{
			var messenger = new Messenger();
			var dispatcher = new DispatcherHelper();
			var serviceCollection = new ServiceCollection();
			Main = new MainViewModel();
			var errorService = new ErrorService(Main);

			serviceCollection.AddSingleton<IDispatcherHelper, DispatcherHelper>();

			// Register services
			serviceCollection.AddSingleton<ErrorService>();
			serviceCollection.AddSingleton<ProfilesService>();
			serviceCollection.AddSingleton(messenger);

			var profileHandler = new LocalProfileHandler(messenger, dispatcher);
			profileHandler
				.SetNext(new RestProfileHandler(messenger, dispatcher, errorService))
				.SetNext(new RmqProfileHandler(messenger, dispatcher, errorService));

			serviceCollection.AddSingleton<IProfileHandler>(profileHandler);
			serviceCollection.AddTransient((c) => c.GetRequiredService<IProfileHandler>().GetArchiveEntryService());
			serviceCollection.AddTransient((c) => c.GetRequiredService<IProfileHandler>().GetUserService());

			// Register viewmodels
			serviceCollection.AddSingleton<LoginViewModel>();
			serviceCollection.AddTransient<LoginProfileViewModel>();
			serviceCollection.AddSingleton<Func<LoginProfileViewModel>>(c => () => c.GetRequiredService<LoginProfileViewModel>());
			serviceCollection.AddScoped<MenuViewModel>();

			serviceCollection.AddScoped<AllArchiveEntriesViewModel>();
			serviceCollection.AddScoped<AllUsersViewModel>();

			serviceCollection.AddSingleton<INavigationService>(Main);

			var serviceProvider = serviceCollection.BuildServiceProvider();

			Main.NavigateForward(serviceProvider.GetRequiredService<LoginViewModel>());
		}

		public MainViewModel Main { get; }
	}
}
