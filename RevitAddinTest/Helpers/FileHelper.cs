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
        public enum FileDialogType
        {
            Open,
            Save
        }

        // A File Dialog Helper to make it easier to handle file paths
        public static string GetFilePath(string title, string filter = "All Files (*.*)|*.*", FileDialogType fileDialogType = FileDialogType.Open)
        {
			FileDialog fileDialog = null;
            if (fileDialogType == FileDialogType.Open)
            {
                fileDialog = new OpenFileDialog();
            }
            else if (fileDialogType == FileDialogType.Save)
            {
                fileDialog = new SaveFileDialog();
            }

            fileDialog.Filter = filter;
            fileDialog.Title = title;

            if (fileDialog.ShowDialog() == DialogResult.OK)
			{
				return fileDialog.FileName;
			}

			return null;
		}
	}
}
