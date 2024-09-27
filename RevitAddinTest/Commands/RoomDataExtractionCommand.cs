using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using RevitAddinTest.Helpers;
using RevitAddinTest.ViewModels;
using RevitAddinTest.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Interop;

namespace RevitAddinTest.Commands
{
	[Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
	public class RoomDataExtractionCommand : IExternalCommand
	{
		public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
		{
			// Initialize ViewModel
			RoomDataExtractionViewModel viewModel = new RoomDataExtractionViewModel(commandData);

			// Initialize View and set DataContext
			RoomDataExtractionView view = new RoomDataExtractionView
			{
				DataContext = viewModel
			};

			// Set the Revit window as the owner of the WPF window
			WindowHelper.SetRevitAsOwner(view);

			// Show the view as a modal dialog
			view.ShowDialog();

			return Result.Succeeded;
		}
	}
}
