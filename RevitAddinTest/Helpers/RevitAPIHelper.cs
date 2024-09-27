using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;

namespace RevitAddinTest.Helpers
{
	public static class RevitAPIHelper
	{
		public static bool GetVolumeCalculationSettingsDefinition(Document doc)
		{
			AreaVolumeSettings settings = AreaVolumeSettings.GetAreaVolumeSettings(doc);
			return settings.ComputeVolumes;
		}

		public static void SetVolumeCaculation(Document doc, bool enabled = true)
		{
			try
			{
				using (Transaction transaction = new Transaction(doc, $"Turn { (enabled ? "on" : "off") } volume calculation"))
				{
					transaction.Start();
					AreaVolumeSettings settings = AreaVolumeSettings.GetAreaVolumeSettings(doc);
					settings.ComputeVolumes = enabled;

					if (enabled)
					{
						doc.Regenerate();
                    }
					transaction.Commit();
				}
			}
			catch (Exception e)
			{
				TaskDialog taskDialog = new TaskDialog("Error");
				taskDialog.MainInstruction = "Error setting volume calculation";
				taskDialog.ExpandedContent = e.Message + "\n" + e.StackTrace;
				taskDialog.Show();
			}
		}


		public static void ZoomToElements(ExternalCommandData commandData, List<Element> elements)
		{
			try
			{
                if (commandData == null || elements == null || !elements.Any())
                {
                    return;
                }
				commandData.Application.ActiveUIDocument.ShowElements(elements.Select(e => e.Id).ToList());
            }
			catch
			{
                return;
            }
		}


		public static List<FamilyInstance> GetElementsInsideRoom(Document doc, Room room, double tolerance = 0)
		{
            // Get all family instances in the room, excluding doors/windows/voids
            BoundingBoxXYZ roomBBox = room.get_BoundingBox(null);

            // Subtract tolerance to the bounding box
            XYZ vectorTolerance = roomBBox.Max.Subtract(roomBBox.Min).Normalize().Multiply(tolerance);
            XYZ min = roomBBox.Min.Add(vectorTolerance);
            XYZ max = roomBBox.Max.Add(vectorTolerance.Negate());

            Outline roomOutline = new Outline(min, max);
            // I decided to use BoundingBoxIntersectsFilter instead of BoundingBoxIsInsideFilter
            // because some host based items seem to be outside the room's bounding box
            // and setting a tolerance let me include them and remove other elements like electrical fixtures
            // that are inside the walls, not the room itself
            BoundingBoxIntersectsFilter bboxFilter = new BoundingBoxIntersectsFilter(roomOutline);
            //BoundingBoxIsInsideFilter bboxFilter = new BoundingBoxIsInsideFilter(roomOutline);

            List<FamilyInstance> familyInstances = new List<FamilyInstance>();
            // So I use the room's bounding box to get the elements that are inside it
            familyInstances = new FilteredElementCollector(doc)
                .WherePasses(bboxFilter)
                .OfClass(typeof(FamilyInstance))
                .WhereElementIsNotElementType()
                .ToElements()
                .Cast<FamilyInstance>()
                .ToList();

			return familyInstances;
        }
    }
}
