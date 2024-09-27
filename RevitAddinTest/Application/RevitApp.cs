using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace RevitAddinTest.Application
{
	public class RevitApp : IExternalApplication
	{
		private string thisAssemblyPath = Assembly.GetExecutingAssembly().Location;

		public Result OnStartup(UIControlledApplication application)
		{
			try
			{
				string tabName = "RevitAddinTest";
				application.CreateRibbonTab(tabName);

				RibbonPanel ribbonPanelRooms = application.CreateRibbonPanel(tabName, "Rooms");

				PushButtonData roomDataButtonData = new PushButtonData(
					"RoomDataExtraction",
					"Room Data\nExtraction",
					thisAssemblyPath,
					"RevitAddinTest.Commands.RoomDataExtractionCommand");

				roomDataButtonData.ToolTip = "Extract room data from the active document.";
				roomDataButtonData.LongDescription = "This command extracts room data from the active document and generates a report.";
				roomDataButtonData.LargeImage = new BitmapImage(new Uri("pack://application:,,,/RevitAddinTest;component/Resources/Icons/room-data-extraction-24.png"));
				roomDataButtonData.Image = new BitmapImage(new Uri("pack://application:,,,/RevitAddinTest;component/Resources/Icons/room-data-extraction-16.png"));

				ribbonPanelRooms.AddItem(roomDataButtonData);


				RibbonPanel ribbonPanelGeometry = application.CreateRibbonPanel(tabName, "Geometry");

				PushButtonData importObjButtonData = new PushButtonData(
					"ImportObjGeometry",
					"Import OBJ\nGeometry",
					thisAssemblyPath,
					"RevitAddinTest.Commands.ImportObjGeometryCommand");

				importObjButtonData.ToolTip = "Import OBJ geometry into the active document.";
				importObjButtonData.LongDescription = "This command imports OBJ geometry into the active document.";
				importObjButtonData.LargeImage = new BitmapImage(new Uri("pack://application:,,,/RevitAddinTest;component/Resources/Icons/import-obj-geometry-24.png"));
				importObjButtonData.Image = new BitmapImage(new Uri("pack://application:,,,/RevitAddinTest;component/Resources/Icons/import-obj-geometry-16.png"));

				ribbonPanelGeometry.AddItem(importObjButtonData);
			}
			catch (Exception e)
			{
				TaskDialog taskDialog = new TaskDialog("Error");
				taskDialog.MainInstruction = e.Message;
				taskDialog.ExpandedContent = e.StackTrace + "\n\n" + e.Source + "\n\n" + e.TargetSite;
				taskDialog.Show();

				return Result.Failed;
			}
			

			return Result.Succeeded;
		}

		public Result OnShutdown(UIControlledApplication application)
		{
			return Result.Succeeded;
		}
	}
}
