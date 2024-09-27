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
using RevitAddinTest.Helpers;
using Autodesk.Revit.DB;

namespace RevitAddinTest.ViewModels
{
	public class RoomDataExtractionViewModel : BaseViewModel
	{
		private readonly RoomDataExtractionService _dataExtractService;
		private readonly ReportService _reportService;
		private readonly ExternalCommandData _commandData;
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

		public ICommand ZoomToRoomCommand { get; }
		public ICommand ExportCsvCommand { get; }
		//public ICommand ExportExcelCommand { get; }

		public RoomDataExtractionViewModel(ExternalCommandData commandData)
		{
			try
			{
                _commandData = commandData;

                // Initialize service
                _dataExtractService = new RoomDataExtractionService(commandData);
                _reportService = new ReportService();

                // Initialize commands
                ZoomToRoomCommand = new RelayCommand(ExecuteZoomToRoomCommand);
                ExportCsvCommand = new RelayCommand(ExecuteExportCsvCommand);

                // Initialize room data collection
                this.Rooms = new ObservableCollection<RoomModel>();

                // Extract room data and update the Rooms collection
                List<RoomModel> roomData = _dataExtractService.ExtractAndProcessRoomData();
                this.Rooms.Clear();
                foreach (RoomModel room in roomData)
                {
                    this.Rooms.Add(room);
                }
            }
			catch (Exception e)
			{
				TaskDialog taskDialog = new TaskDialog("Error");
				taskDialog.MainInstruction = "Error initializing Room Data Extraction";
				taskDialog.ExpandedContent = e.Message + "\n" + e.StackTrace;
				taskDialog.Show();
			}
			
		}

		private void ExecuteZoomToRoomCommand(object parameter)
        {
            RoomModel room = parameter as RoomModel;
            if (room != null)
            {
                RevitAPIHelper.ZoomToElements(_commandData, new List<Element> { room.RoomElement });
            }
        }

        private void ExecuteExportCsvCommand(object parameter)
		{
			_reportService.GenerateRoomDataReport(this.Rooms.ToList());
		}
	}
}
