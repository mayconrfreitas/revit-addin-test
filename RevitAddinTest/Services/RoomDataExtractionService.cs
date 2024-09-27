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
				bool wasCalculatingVolumes = RevitAPIHelper.GetVolumeCalculationSettingsDefinition(_doc);

				// Enable volume calculation if it is not enabled
                if (!wasCalculatingVolumes)
				{
					TaskDialog taskDialog = new TaskDialog("Volume Calculation");
					taskDialog.MainInstruction = "Volume calculation is not enabled. Do you want to enable it?";
					taskDialog.CommonButtons = TaskDialogCommonButtons.Yes | TaskDialogCommonButtons.No;
					TaskDialogResult result = taskDialog.Show();

					if (result == TaskDialogResult.Yes)
                    {
                        RevitAPIHelper.SetVolumeCaculation(_doc);
                    }
					else
                    {
                        throw new Exception("Volume calculation is required to calculate occupied volume.");
                    }
				}

                List<Element> rooms = new List<Element>();

                List <ElementId> selectedRooms = _uidoc.Selection
					.GetElementIds()
					.Where(id => _doc.GetElement(id) is Room)
					.ToList();
                // Check if there's any room selected
                if (selectedRooms.Count > 0)
				{
					rooms = selectedRooms.Select(id => _doc.GetElement(id)).ToList();
				}
				else
				{
                    // Collect all rooms
                    rooms = new FilteredElementCollector(_doc)
                        .OfCategory(BuiltInCategory.OST_Rooms)
                        .WhereElementIsNotElementType()
                        .ToElements()
                        .ToList();
                }

				foreach (Element roomElement in rooms)
				{
					Room room = roomElement as Room;
					// As I already turned on the volume calculation,
					// I can safely assume that the room has volume and area,
					// otherwise, it's not placed, so I'm skipping it for this exercise
					// I could also set an error message to the user
					if (room != null && room.Volume > 0 && room.Area > 0)
					{

						// I noticed in the model that most rooms have the same Level and Upper Limit (level)
						// and Limit Offset is heigher than the ceiling height
						// here I would check this and maybe warn the user about it, asking if he wants to proceed
						// Or fix it automatically
						// But for this exercise, I'm skipping this part

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

						// Store the room element
						roomModel.RoomElement = room;

						roomModels.Add(roomModel);
					}
				}
            }
			catch (Exception e)
			{
				TaskDialog taskDialog = new TaskDialog("Error");
				taskDialog.MainInstruction = "Failed to extract room data";
				taskDialog.MainContent = e.Message + "\n" + e.StackTrace;
				taskDialog.Show();
			}

			// Create the Total Room Model
			RoomModel totalRoomModel = new RoomModel
            {
                Name = "TOTAL",
                Number = "-",
                Area = roomModels.Sum(r => r.Area),
                Volume = roomModels.Sum(r => r.Volume),
                OccupiedVolume = roomModels.Sum(r => r.OccupiedVolume),
                UtilizationRatio = roomModels.Sum(r => r.OccupiedVolume) / roomModels.Sum(r => r.Volume),
                UtilizationCategory = CategorizeUtilization(roomModels.Sum(r => r.OccupiedVolume) / roomModels.Sum(r => r.Volume))
            };

			roomModels.Add(totalRoomModel);
			

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
