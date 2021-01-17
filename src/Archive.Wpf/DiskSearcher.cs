using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using Archive.ClientBase;

namespace Archive.Wpf
{
	public class DiskSearcher : IDiskSearcher
	{
		public Task<string> SelectFileAsync()
		{
			var openFileDialog = new Microsoft.Win32.OpenFileDialog
			{
				InitialDirectory = "C:\\",
				Filter = "All files (*.*)|*.*",
				FilterIndex = 1,
				RestoreDirectory = true
			};

			if (openFileDialog.ShowDialog() ?? false)
            {
                return Task.FromResult(openFileDialog.FileName);
            }

            return Task.FromResult("");
        }

		public Task<string> SelectFolderAsync()
		{
            var dir = Directory.GetCurrentDirectory();

			try
			{
                using var openFileDialog = new FolderBrowserDialog();

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    return Task.FromResult(openFileDialog.SelectedPath);
                }

                return Task.FromResult("");
            }
			finally
			{
                Directory.SetCurrentDirectory(dir);
			}
        }
	}
}