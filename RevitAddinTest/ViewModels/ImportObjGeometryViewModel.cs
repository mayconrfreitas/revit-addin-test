using Autodesk.Revit.UI;
using RevitAddinTest.Commands;
using RevitAddinTest.Helpers;
using RevitAddinTest.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace RevitAddinTest.ViewModels
{
	public class ImportObjGeometryViewModel : BaseViewModel
	{
		private readonly ImportObjGeometryService _service;

		private string _objFilePath;
		public string OBJFilePath
		{
			get { return _objFilePath; }
			set
			{
				_objFilePath = value;
				OnPropertyChanged(nameof(OBJFilePath));
			}
		}

		public ICommand BrowseCommand { get; }
		public ICommand ImportCommand { get; }

		public ImportObjGeometryViewModel(ExternalCommandData commandData)
		{
			_service = new ImportObjGeometryService(commandData);

			// Initialize commands
			BrowseCommand = new RelayCommand(ExecuteBrowseCommand);
			ImportCommand = new RelayCommand(ExecuteImportCommand, CanExecuteImportCommand);
		}

		private void ExecuteBrowseCommand(object parameter)
		{
			string filePath = FileHelper.GetFilePath("Select an OBJ File", "OBJ Files (*.obj)|*.obj");
			if (!string.IsNullOrEmpty(filePath))
			{
				OBJFilePath = filePath;
			}
		}

		private bool CanExecuteImportCommand(object parameter)
		{
			return !string.IsNullOrEmpty(OBJFilePath);
		}

		private void ExecuteImportCommand(object parameter)
		{
			_service.ImportOBJFile(OBJFilePath);
		}
	}
}
