using Cosei.Client.Base;
using Cosei.Client.Http;
using System;
using System.Threading.Tasks;
using Archive.ClientBase.Models;
using Archive.ClientBase.Services;
using Archive.ClientBase.Services.Implementation;

namespace Archive.ClientBase.ProfileHandler
{
	public sealed class RestProfileHandler : AbstractProfileHandler
	{
		private RestProfile _profile;
		private readonly IMessenger _messenger;
		private readonly IDispatcherHelper _dispatcher;
		private readonly IErrorService _errorService;

		public RestProfileHandler(IMessenger messenger, IDispatcherHelper dispatcher, IErrorService errorService)
		{
			_messenger = messenger;
			_dispatcher = dispatcher;
			_errorService = errorService;
		}

		private IRequestClient CreateRequestClient()
		{
			return new HttpRequestClient(_profile.ServerName);
		}

		protected override async Task ApplyProfile(ProfileBase profile)
		{
			if (profile is RestProfile restProfile)
			{
				_profile = restProfile;

				var requestClient = CreateRequestClient();
				var tokenProvider = new TokenProvider(CreateRequestClient, _profile.ServerUser, _profile.ServerPassword);
				
				var token = await tokenProvider.GetToken();

				var serverUri = new Uri(_profile.ServerName);
				var signalRUri = new Uri(serverUri, "/cosei");

				var subscriber = new SignalRSubscriber(signalRUri.AbsoluteUri, 
					options =>
					{
						options.Headers.Add("Authorization", $"Bearer {token}");
					},
					exception =>
					{
						_dispatcher.BeginInvokeOnMainThread(async () => await _errorService.ShowAlertAsync(exception));
					});

				var archiveEntryStore = new ArchiveEntryStore(requestClient, tokenProvider);
				ArchiveEntryService = new ArchiveEntryService(archiveEntryStore, _messenger, _dispatcher, subscriber);
				var userStore = new UserStore(requestClient, tokenProvider);
				UserService = new UserService(userStore, _messenger, _dispatcher, subscriber);

				await subscriber.StartAsync();

				Subscriber = subscriber;
			}
		}
	}
}
