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
	public class ImportObjGeometryCommand : IExternalCommand
	{
		public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
		{
			// Initialize ViewModel
			ImportObjGeometryViewModel viewModel = new ImportObjGeometryViewModel(commandData);

			// Initialize View and set DataContext
			ImportObjGeometryView view = new ImportObjGeometryView
			{
				DataContext = viewModel
			};

			WindowHelper.SetRevitAsOwner(view);

			// Show the view as a modal dialog
			view.ShowDialog();

			return Result.Succeeded;
		}
	}
}
