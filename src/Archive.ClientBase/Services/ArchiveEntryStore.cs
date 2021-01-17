using Cosei.Client.RabbitMq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Archive.ClientBase.Models;
using Archive.Contracts;
using System.IO;
using System.Buffers.Text;
using System.IO.Compression;

namespace Archive.ClientBase.Services
{
	public partial class ArchiveEntryStore : IArchiveDataStore<ArchiveEntryModel>
	{
		private readonly IRequestClient _client;
		private readonly TokenProvider _tokenProvider;
		private IEnumerable<ArchiveEntryModel> items;

		public ArchiveEntryStore(IRequestClient client, TokenProvider tokenProvider)
		{
			_tokenProvider = tokenProvider;
			_client = client;
			items = new List<ArchiveEntryModel>();
		}

		public async Task<IEnumerable<ArchiveEntryModel>> GetItemsAsync(bool forceRefresh = false)
		{
			if (forceRefresh)
			{
				var token = await _tokenProvider.GetToken();

				var headers = new Dictionary<string, string>
				{
					{ "Authorization", $"Bearer {token}" }
				};

				var response = await _client.GetAsync("api/ArchiveEntry", headers);
				response = response.EnsureSuccessStatusCode();
				var responseString = Encoding.UTF8.GetString(response.Body.ToArray());

				items = JsonConvert.DeserializeObject<List<ArchiveEntryModel>>(responseString);
			}

			return items;
		}

		public async Task<ArchiveEntryModel> GetItemAsync(Guid id)
		{
			if (id != Guid.Empty)
			{
				var token = await _tokenProvider.GetToken();

				var headers = new Dictionary<string, string>
				{
					{ "Authorization", $"Bearer {token}" }
				};

				var response = await _client.GetAsync($"api/ArchiveEntry/{id}", headers);
				response = response.EnsureSuccessStatusCode();
				var responseString = Encoding.UTF8.GetString(response.Body.ToArray());

				return JsonConvert.DeserializeObject<ArchiveEntryModel>(responseString);
			}

			return null;
		}

		public async Task<bool> AddItemAsync(ArchiveEntryModel item)
		{
			if (item == null)
			{
				return false;
			}

			var token = await _tokenProvider.GetToken();

			var headers = new Dictionary<string, string>
			{
				{ "Authorization", $"Bearer {token}" }
			};

			var request = new CreateArchiveEntry(item.Id,
				item.Date,
				item.Description,
				item.Groups,
				item.OriginalName,
				item.IsFolder,
				item.FileSize);

			var response = await _client.PostAsync($"api/ArchiveEntry", JsonConvert.SerializeObject(request), "application/json", headers);
			response.EnsureSuccessStatusCode();

			return true;
		}

		private static void FolderToArchive(ZipArchive archive, string dirPath, int dirLen)
		{
			var paths = Directory.GetFiles(dirPath);
			var dirs = Directory.GetDirectories(dirPath);

			foreach (var dir in dirs)
			{
				FolderToArchive(archive, dir, dirLen);
			}

			foreach (var path in paths)
			{
				var newName = path.Substring(dirLen);

				var zipArchiveEntry = archive.CreateEntry(newName, CompressionLevel.NoCompression);

				using (var entryStream = zipArchiveEntry.Open())
				{
					using (var memstr = new MemoryStream(File.ReadAllBytes(path)))
					{
						memstr.CopyTo(entryStream);
					}
				}
			}
		}

		public async Task<long> AddFromPathAsync(Guid id, string path)
		{
			var idStr = id.ToString();
			var token = await _tokenProvider.GetToken();

			var headers = new Dictionary<string, string>
			{
				{ "Authorization", $"Bearer {token}" }
			};

			if (Directory.Exists(path))
			{
				var newPath = Path.Combine(Path.GetTempPath(), idStr);
				Directory.CreateDirectory(newPath);
				newPath = Path.Combine(newPath, Path.GetFileName(path));

				using (var stream = File.OpenWrite(newPath))
				{
					using (var archive = new ZipArchive(stream, ZipArchiveMode.Create, true))
					{
						var dirLen = path.Length + 1;
						FolderToArchive(archive, path, dirLen);
					}
				}

				path = newPath;
			}

			if (File.Exists(path))
			{
				var str = await Task.Run(() =>
				{
					var tempPath = Path.GetTempFileName();
					
					using (var output = File.OpenWrite(tempPath))
					{
						using (var dstream = new DeflateStream(output, CompressionLevel.Optimal))
						{
							var originalBytes = File.ReadAllBytes(path);
							dstream.Write(originalBytes, 0, originalBytes.Length);
						}
					}

					var compressedBytes = File.ReadAllBytes(tempPath);

					File.Delete(tempPath);

					return (Base64: Convert.ToBase64String(compressedBytes), Length: compressedBytes.Length);
				});

				var commandString = JsonConvert.SerializeObject(new AddBase64File(id, str.Base64));
				var response = await _client.PostAsync($"api/ArchiveEntry/AddBase64File", commandString, "application/json", headers);
				response.EnsureSuccessStatusCode();
				return str.Length;
			}

			return 0;
		}

		public async Task<byte[]> GetBytesFromIdAsync(Guid id)
		{
			var token = await _tokenProvider.GetToken();

			var headers = new Dictionary<string, string>
			{
				{ "Authorization", $"Bearer {token}" }
			};

			var response = await _client.GetAsync($"api/ArchiveEntry/GetFile/" + id.ToString(), headers);
			response.EnsureSuccessStatusCode();

			return response.Body.ToArray();
		}

		public async Task<bool> UpdateItemAsync(ArchiveEntryModel item)
		{
			if (item == null || item.Id == Guid.Empty)
			{
				return false;
			}

			var token = await _tokenProvider.GetToken();

			var headers = new Dictionary<string, string>
			{
				{ "Authorization", $"Bearer {token}" }
			};

			var request = new UpdateArchiveEntry(item.Id,
				item.Date,
				item.Description,
				item.Groups,
				item.OriginalName,
				item.IsFolder,
				item.FileSize);

			var response = await _client.PutAsync($"api/ArchiveEntry", JsonConvert.SerializeObject(request), "application/json", headers);
			response.EnsureSuccessStatusCode();

			return true;
		}

		public async Task<bool> DeleteItemAsync(Guid id)
		{
			if (id == Guid.Empty)
			{
				return false;
			}

			var token = await _tokenProvider.GetToken();

			var headers = new Dictionary<string, string>
			{
				{ "Authorization", $"Bearer {token}" }
			};

			var response = await _client.DeleteAsync($"api/ArchiveEntry/{id}", headers);
			response.EnsureSuccessStatusCode();

			return true;
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
					_client.Dispose();
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