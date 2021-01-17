using System;

namespace Archive.Contracts
{
	public class AddBase64File
	{
		public Guid Id { get; set; }
		public string Base64Content { get; set; }

		public AddBase64File(Guid id, string base64Content)
		{
			Id = id;
			Base64Content = base64Content;
		}
	}
}
