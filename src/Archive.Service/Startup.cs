using Cosei.Service.Base;
using Cosei.Service.Http;
using Cosei.Service.RabbitMq;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using Archive.Service.Configuration;
using Archive.Service.Query;
using Archive.Service.Repository;
using Archive.Service.Services;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using System.IO;

namespace Archive.Service
{
	public class Startup
	{
		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			services.Configure<KestrelServerOptions>(options =>
			{
				options.Limits.MaxRequestBodySize = int.MaxValue; // Default value is 30 MB
			});

			var rmqConfig = ConfigurationReader.ReadConfiguration<RabbitMqConfiguration>();
			var fileConfig = ConfigurationReader.ReadConfiguration<FileConfiguration>();
			var blobConfig = ConfigurationReader.ReadConfiguration<BlobConfiguration>();
			var mysqlConfig = ConfigurationReader.ReadConfiguration<MySqlConfiguration>();
			var mongoDbConfig = ConfigurationReader.ReadConfiguration<MongoDbConfiguration>();
			var serviceConfig = ConfigurationReader.ReadConfiguration<ServiceConfiguration>();

			if (blobConfig == null)
			{
				blobConfig = new BlobConfiguration();
			}

			Directory.CreateDirectory(blobConfig.BlobsPath);
			services.AddSingleton(blobConfig);

			if (string.IsNullOrWhiteSpace(serviceConfig?.Secret))
			{
				throw new Exception($"Please set {nameof(ServiceConfiguration.Secret)} in the {nameof(ServiceConfiguration)}!");
			}

			services.AddSingleton(serviceConfig);

			if (mongoDbConfig != null)
			{
				string connectionString;

				if (mongoDbConfig.UseDnsSrv)
				{
					connectionString = $"mongodb+srv://{mongoDbConfig.User}:{mongoDbConfig.Password}" +
						$"@{mongoDbConfig.Host}/{mongoDbConfig.Database}";
				}
				else
				{
					connectionString = $"mongodb://{mongoDbConfig.User}:{mongoDbConfig.Password}" +
						$"@{mongoDbConfig.Host}:{mongoDbConfig.Port}/{mongoDbConfig.Database}";
				}

				var client = new MongoDB.Driver.MongoClient(connectionString);
				var db = client.GetDatabase(mongoDbConfig.Database);

				services.AddSingleton(db);
				services.AddScoped<IArchiveEntryRepository, MongoDbArchiveEntryRepository>();
				services.AddScoped<IUserRepository, MongoDbUserRepository>();
				services.AddScoped<IArchiveEntryQuery, MongoDbArchiveEntryQuery>();
				services.AddScoped<IUserQuery, MongoDbUserQuery>();
			}
			else if (mysqlConfig != null)
			{
				services.AddSingleton(mysqlConfig);
				services.AddScoped<IArchiveEntryRepository, MySqlArchiveEntryRepository>();
				services.AddScoped<IUserRepository, MySqlUserRepository>();
				services.AddScoped<IArchiveEntryQuery, MySqlArchiveEntryQuery>();
				services.AddScoped<IUserQuery, MySqlUserQuery>();
				services.AddScoped<MySqlDbContext>();
			}
			else
			{
				if (fileConfig == null) fileConfig = new FileConfiguration();

				services.AddSingleton(fileConfig);
				services.AddScoped<IArchiveEntryRepository, FileArchiveEntryRepository>();
				services.AddScoped<IUserRepository, FileUserRepository>();
				services.AddScoped<IArchiveEntryQuery, FileArchiveEntryQuery>();
				services.AddScoped<IUserQuery, FileUserQuery>();
			}

			if (rmqConfig != null)
			{
				rmqConfig.QueueName = "Archive.Service";
				services.AddCoseiRabbitMq(rmqConfig);
			}

			services.AddCoseiHttp();

			services.AddScoped<ArchiveEntryService>();
			services.AddScoped<UserService>();
			services.AddScoped<TokenService>();
			services.AddSignalR();

			var validationParameters = TokenService.GetValidationParameters(serviceConfig.Secret);

			services.AddAuthentication(x =>
			{
				x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
				x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
			})
			.AddJwtBearer(x =>
			{
				x.RequireHttpsMetadata = false;
				x.TokenValidationParameters = validationParameters;
			});

			services.AddControllers();
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app)
		{
			app.UseCors(c =>
			{
				c.AllowAnyOrigin();
				c.AllowAnyMethod();
				c.AllowAnyHeader();
			});

			app.UseAuthentication();
			// app.UseHttpsRedirection();
			app.UseRouting();
			app.UseAuthorization();
			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllers();
				endpoints.MapHub<CoseiHub>("/cosei");
			});

			app.UseCosei();

			using var scope = app.ApplicationServices.CreateScope();
			scope.ServiceProvider.GetService<MySqlDbContext>()?.Database?.EnsureCreated();
		}
	}
}
