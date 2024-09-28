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
		private readonly ImportObjGeometryService _importObjService;
        private readonly ExternalCommandData _commandData;

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
			_commandData = commandData;

            // Initialize service
            _importObjService = new ImportObjGeometryService(commandData);

			// Initialize commands
			BrowseCommand = new RelayCommand(ExecuteBrowseCommand);
			ImportCommand = new RelayCommand(ExecuteImportCommand, CanExecuteImportCommand);
		}

		// Command to browse for an OBJ file
		private void ExecuteBrowseCommand(object parameter)
		{
			string filePath = FileHelper.GetFilePath("Select an OBJ File", "OBJ Files (*.obj)|*.obj");
			if (!string.IsNullOrEmpty(filePath))
			{
				OBJFilePath = filePath;
			}
		}

		// Command to check if the path is valid for importing the OBJ file
		private bool CanExecuteImportCommand(object parameter)
		{
			return !string.IsNullOrEmpty(OBJFilePath);
		}

		// Command to import the OBJ file
		private void ExecuteImportCommand(object parameter)
		{
			// Call the service to import the OBJ file
			_importObjService.ImportOBJFile(OBJFilePath);
		}
	}
}
