using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using RevitAddinTest.Helpers;
using RevitAddinTest.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitAddinTest.Services
{
	public class RoomDataExtractionService
	{
		private readonly UIDocument _uidoc;
		private readonly Document _doc;

		public RoomDataExtractionService(ExternalCommandData commandData)
		{
			_uidoc = commandData.Application.ActiveUIDocument;
			_doc = _uidoc.Document;
		}

		public List<RoomModel> ExtractAndProcessRoomData()
		{
			List<RoomModel> roomModels = new List<RoomModel>();
			try
			{
				// Check if volume calculation is enabled
				bool wasCalculatingVolumes = RevitAPIHelper.GetVolumeCalculation(_doc);

				// Enable volume calculation if it is not enabled
                if (!wasCalculatingVolumes)
				{
					RevitAPIHelper.SetVolumeCaculation(_doc);
				}

				// Collect all rooms
				List<Element> roomCollector = new FilteredElementCollector(_doc)
					.OfCategory(BuiltInCategory.OST_Rooms)
					.WhereElementIsNotElementType()
					.ToElements()
					.ToList();

				foreach (Element roomElement in roomCollector)
				{
					Room room = roomElement as Room;
					if (room != null)
					{
						RoomModel roomModel = new RoomModel
						{
							Name = room.Name,
							Number = room.Number,
							Area = UnitUtils.ConvertFromInternalUnits(room.Area, UnitTypeId.SquareMeters),
							Volume = UnitUtils.ConvertFromInternalUnits(room.Volume, UnitTypeId.CubicMeters)
						};

						// Calculate occupied volume
						double occupiedVolume = GeometryHelper.CalculateOccupiedVolume(_doc, room);
						roomModel.OccupiedVolume = occupiedVolume;

						// Compute utilization ratio
						roomModel.UtilizationRatio = occupiedVolume / roomModel.Volume;

						// Categorize utilization
						roomModel.UtilizationCategory = CategorizeUtilization(roomModel.UtilizationRatio);

						roomModels.Add(roomModel);
					}
				}

				// Disable volume calculation if it was not enabled
                if (!wasCalculatingVolumes)
                {
                    RevitAPIHelper.SetVolumeCaculation(_doc, false);
                }
            }
			catch (Exception e)
			{
				TaskDialog taskDialog = new TaskDialog("Error");
				taskDialog.MainInstruction = "Failed to extract room data";
				taskDialog.MainContent = e.Message + "\n" + e.StackTrace;
				taskDialog.Show();
			}
			

			return roomModels;
		}

		private string CategorizeUtilization(double ratio)
		{
			if (ratio < 0.3)
				return "Under-utilized";
			else if (ratio >= 0.3 && ratio <= 0.8)
				return "Well-utilized";
			else
				return "Over-utilized";
		}
	}
}
