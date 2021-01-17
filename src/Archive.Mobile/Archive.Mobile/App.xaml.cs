﻿using Microsoft.Extensions.DependencyInjection;
using System;
using Archive.ClientBase;
using Archive.ClientBase.ProfileHandler;
using Archive.ClientBase.Services;
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
				serviceCollection.AddSingleton(errorService);
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