﻿using Microsoft.Extensions.DependencyInjection;
using System;
using Archive.ClientBase;
using Archive.ClientBase.ProfileHandler;
using Archive.ClientBase.Services;
using Archive.ClientBase.Services.Implementation;
using Archive.ClientBase.ViewModel;
using Archive.Mobile.View;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]

namespace Archive.Mobile
{
	public partial class App : Application
	{
		private static NavigationServicePage _navigationPage;

		public App()
		{
			if (_navigationPage == null)
			{
				var loginView = new LoginView();
				_navigationPage = new NavigationServicePage(loginView);

				var messenger = new Messenger();
				var dispatcher = new DispatcherHelper();
				var serviceCollection = new ServiceCollection();
				var errorService = new ErrorService(_navigationPage);

				serviceCollection.AddSingleton<IDispatcherHelper, DispatcherHelper>();

				// Register services
				serviceCollection.AddSingleton<IErrorService>(errorService);
				serviceCollection.AddSingleton<IProfilesService, ProfilesService>();
				serviceCollection.AddSingleton<IMessenger>(messenger);

				var profileHandler = new LocalProfileHandler(messenger, dispatcher);
				profileHandler
					.SetNext(new RestProfileHandler(messenger, dispatcher, errorService))
					.SetNext(new RmqProfileHandler(messenger, dispatcher, errorService));

				serviceCollection.AddSingleton<IProfileHandler>(profileHandler);
				serviceCollection.AddTransient((c) => c.GetRequiredService<IProfileHandler>().GetArchiveEntryService());
				serviceCollection.AddTransient((c) => c.GetRequiredService<IProfileHandler>().GetUserService());

				// Register viewmodels
				serviceCollection.AddSingleton<LoginViewModel>();
				serviceCollection.AddTransient<ProfileViewModel>();
				serviceCollection.AddSingleton<Func<ProfileViewModel>>(c => () => c.GetRequiredService<ProfileViewModel>());
				serviceCollection.AddTransient<LoginProfileViewModel>();
				serviceCollection.AddSingleton<Func<LoginProfileViewModel>>(c => () => c.GetRequiredService<LoginProfileViewModel>());
				serviceCollection.AddScoped<MenuViewModel>();

				serviceCollection.AddScoped<AllArchiveEntriesViewModel>();
				serviceCollection.AddScoped<AllUsersViewModel>();

				serviceCollection.AddTransient<ArchiveEntryViewModel>();
				serviceCollection.AddSingleton<Func<ArchiveEntryViewModel>>(c => () => c.GetRequiredService<ArchiveEntryViewModel>());
				serviceCollection.AddTransient<ArchiveGroupViewModel>();
				serviceCollection.AddSingleton<Func<ArchiveGroupViewModel>>(c => () => c.GetRequiredService<ArchiveGroupViewModel>());
				serviceCollection.AddTransient<UserViewModel>();
				serviceCollection.AddSingleton<Func<UserViewModel>>(c => () => c.GetRequiredService<UserViewModel>());

				serviceCollection.AddSingleton<INavigationService>(_navigationPage);

				var serviceProvider = serviceCollection.BuildServiceProvider();

				loginView.BindingContext = serviceProvider.GetRequiredService<LoginViewModel>();
			}

			MainPage = _navigationPage;
		}

		protected override void OnStart()
		{
			// Handle when your app starts
		}

		protected override void OnSleep()
		{
			// Handle when your app sleeps
		}

		protected override void OnResume()
		{
			// Handle when your app resumes
		}
	}
}
