using Cosei.Client.Base;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Archive.ClientBase.Models;
using Archive.Contracts;
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

				items = await _client.GetAsync<List<ArchiveEntryModel>>("api/ArchiveEntry", headers);
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

				return await _client.GetAsync<ArchiveEntryModel>($"api/ArchiveEntry/{id}", headers);
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

			await _client.PostAsync($"api/ArchiveEntry", request, headers);

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

				var request = new AddBase64File(id, str.Base64);
				await _client.PostAsync($"api/ArchiveEntry/AddBase64File", request, headers);
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

			await _client.PutAsync($"api/ArchiveEntry", request, headers);

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