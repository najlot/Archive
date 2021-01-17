﻿using Cosei.Client.RabbitMq;
using System.Threading.Tasks;
using Archive.ClientBase.Models;
using Archive.ClientBase.Services;

namespace Archive.ClientBase.ProfileHandler
{
	public sealed class RmqProfileHandler : AbstractProfileHandler
	{
		private RmqProfile _profile;
		private RabbitMqSubscriber _subscriber;
		private readonly Messenger _messenger;
		private readonly IDispatcherHelper _dispatcher;
		private readonly ErrorService _errorService;

		public RmqProfileHandler(Messenger messenger, IDispatcherHelper dispatcher, ErrorService errorService)
		{
			_messenger = messenger;
			_dispatcher = dispatcher;
			_errorService = errorService;
		}

		private IRequestClient CreateRequestClient()
		{
			return new RabbitMqClient(
				_profile.RabbitMqHost,
				_profile.RabbitMqVirtualHost,
				_profile.RabbitMqUser,
				_profile.RabbitMqPassword,
				"Archive.Service");
		}

		protected override async Task ApplyProfile(ProfileBase profile)
		{
			if (_subscriber != null)
			{
				await _subscriber.DisposeAsync();
				_subscriber = null;
			}

			if (profile is RmqProfile rmqProfile)
			{
				_profile = rmqProfile;

				var requestClient = CreateRequestClient();
				var tokenProvider = new TokenProvider(CreateRequestClient, _profile.ServerUser, _profile.ServerPassword);
				var subscriber = new RabbitMqSubscriber(
					_profile.RabbitMqHost,
					_profile.RabbitMqVirtualHost,
					_profile.RabbitMqUser,
					_profile.RabbitMqPassword,
					exception =>
					{
						_dispatcher.BeginInvokeOnMainThread(async () => await _errorService.ShowAlert(exception));
					});

				var archiveEntryStore = new ArchiveEntryStore(requestClient, tokenProvider);
				ArchiveEntryService = new ArchiveEntryService(archiveEntryStore, _messenger, _dispatcher, subscriber);
				var userStore = new UserStore(requestClient, tokenProvider);
				UserService = new UserService(userStore, _messenger, _dispatcher, subscriber);

				await subscriber.StartAsync();

				_subscriber = subscriber;
			}
		}
	}
}