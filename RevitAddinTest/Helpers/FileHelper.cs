using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RevitAddinTest.Helpers
{
	public static class FileHelper
	{
		public static string GetFilePath(string title, string filter = "All Files (*.*)|*.*")
		{
			SaveFileDialog saveFileDialog = new SaveFileDialog
			{
				Filter = filter,
				Title = title
			};

			if (saveFileDialog.ShowDialog() == DialogResult.OK)
			{
				return saveFileDialog.FileName;
			}

			return null;
		}
	}
}
