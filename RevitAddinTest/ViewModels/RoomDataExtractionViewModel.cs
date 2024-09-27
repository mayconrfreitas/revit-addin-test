using Autodesk.Revit.UI;
using RevitAddinTest.Services;
using RevitAddinTest.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Autodesk.Revit.DB.Architecture;
using RevitAddinTest.Models;
using System.Collections.ObjectModel;
using System.Windows.Threading;

namespace RevitAddinTest.ViewModels
{
	public class RoomDataExtractionViewModel : BaseViewModel
	{
		private readonly RoomDataExtractionService _dataExtractService;
		private readonly ReportService _reportService;
		private ObservableCollection<RoomModel> _rooms;

		public ObservableCollection<RoomModel> Rooms
		{
			get { return _rooms; }
			set
			{
				_rooms = value;
				OnPropertyChanged(nameof(Rooms));
			}
		}

		public ICommand ExtractCommand { get; }
		public ICommand ExportCsvCommand { get; }
		//public ICommand ExportExcelCommand { get; }

		public RoomDataExtractionViewModel(ExternalCommandData commandData)
		{
			// Initialize service
			_dataExtractService = new RoomDataExtractionService(commandData);

			try
			{
				//// Extract room data and update the Rooms collection
				//List<RoomModel> roomData = _dataExtractService.ExtractAndProcessRoomData();
				//this.Rooms = new ObservableCollection<RoomModel>();
				//foreach (RoomModel room in roomData)
				//{
				//	this.Rooms.Add(room);
				//}

				// Invoke the ExtractCommand
				ExecuteExtractCommand();
            }
			catch (Exception e)
			{
				TaskDialog taskDialog = new TaskDialog("Error");
				taskDialog.MainInstruction = "Error extracting room data";
				taskDialog.ExpandedContent = e.Message + "\n" + e.StackTrace;
				taskDialog.Show();
			}

			

			// Initialize commands
			ExportCsvCommand = new RelayCommand(ExecuteExportCsvCommand);
			//ExportExcelCommand = new RelayCommand(ExecuteExportExcelCommand);

			// Initialize room data collection
			this.Rooms = new ObservableCollection<RoomModel>();
		}

		private void ExecuteExtractCommand()
		{
            // Extract room data and update the Rooms collection
            List<RoomModel> roomData = _dataExtractService.ExtractAndProcessRoomData();
            this.Rooms = new ObservableCollection<RoomModel>();
            foreach (RoomModel room in roomData)
            {
                this.Rooms.Add(room);
            }
        }

		private void ExecuteExportCsvCommand(object parameter)
		{
			_reportService.GenerateRoomDataReport(this.Rooms.ToList());
		}



		//private void ExecuteExportExcelCommand(object parameter)
		//{
		//	_dataExtractService.ExportToExcel(Rooms.ToList());
		//}
	}
}
