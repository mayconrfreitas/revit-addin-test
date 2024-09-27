using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using RevitAddinTest.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;

namespace RevitAddinTest.Helpers
{
	public static class GeometryHelper
	{
		public static double CalculateOccupiedVolume(Document doc, Room room)
		{
			double occupiedVolume = 0.0;

            List<FamilyInstance> allInstancesInsideRoom = RevitAPIHelper.GetElementsInsideRoom(doc, room, 1);

			// Variable just to check the categories of the elements that were found in the room
			List<FamilyInstance> familyInstances = new List<FamilyInstance>();
			foreach (FamilyInstance instance in allInstancesInsideRoom)
			{
				// Exclude doors, windows, and voids
				// I could also create a setup window listing all the categories that
				// are inside rooms so the user can select which categories to include/exclude
				// But for now, I'm just excluding doors and windows
				// The car, for example, is hidden but it's still inside the Garage and
				// the script is computing its volume
				if (instance.Category.Id.Value == (int)BuiltInCategory.OST_Doors ||
					instance.Category.Id.Value == (int)BuiltInCategory.OST_Windows)
				{
					continue;
				}
				familyInstances.Add(instance); // Add the instance to the list after filtering


				// To calculate to volume I can simply get the HOST_VOLUME_COMPUTED built-in parameter as commented below
				//double volume = 0.0;
				//Parameter volumeParam = instance.get_Parameter(BuiltInParameter.HOST_VOLUME_COMPUTED);

				//if (volumeParam != null && volumeParam.HasValue)
				//{
				//	volume = volumeParam.AsDouble();
				//	occupiedVolume += volume;
				//}



				// But I decided to use the geometry instead, to be more precise

				// Get the geometry of the family instance
				GeometryElement geometryElement = instance.get_Geometry(new Options());

				foreach (GeometryObject geometryObject in geometryElement)
                {
                    if (geometryObject is Solid solid)
                    {
						if (solid.IsVoid()) continue;
                        occupiedVolume += solid.Volume;
                    }
					else if (geometryObject is GeometryInstance geometryInstance)
					{
						foreach (GeometryObject instanceGeometryObject in geometryInstance.GetInstanceGeometry())
                        {
                            if (instanceGeometryObject is Solid instanceSolid)
                            {
								if (instanceSolid.IsVoid()) continue;
                                occupiedVolume += instanceSolid.Volume;
                            }
                        }
					}
                }
			}

			// I made this code for debugging purposes, to check the categories of the elements that were found in the room
			// If the user holds the CTRL key while clicking the button, a message box will show the categories
			if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
			{
				TaskDialog.Show("Info", $"Found {familyInstances.Count()} elements in the room.\n" + String.Join("\n", familyInstances.Select(fi => $"• Category: {fi.Category.Name}, Family: {fi.Symbol.Family.Name}, Type: {fi.Symbol.Name}, Id: {fi.Id};").ToList()));

            }

            // Convert volume to cubic meters
            return UnitUtils.ConvertFromInternalUnits(occupiedVolume, UnitTypeId.CubicMeters);
		}


		public static bool IsVoid(this Solid solid)
        {
			// I didn't find a way to check if a solid is a void or not, other than
			// checking it's volume. It looks like the volume of a void is always 0.
			// If so, void are skipped in the calculation without doing anything else
			// If not, I found a solution at the link below that uses the GraphicsStyleCategory
			// to check if the solid is a void or not
			// I didn't notice any difference during my tests, but I'm keeping it here for reference
            // Solution found at: https://forums.autodesk.com/t5/revit-api-forum/how-to-know-the-selected-element-is-a-void-or-solid/m-p/7416552#M25473

            if (solid == null) return false;

			GeometryObject geometryObject = solid as GeometryObject;

			if (geometryObject == null) return false;

            // Get the GraphicsStyleCategory from the solid
            ElementId graphicsStyleId = geometryObject.GraphicsStyleId;
			if (graphicsStyleId == new ElementId(BuiltInCategory.OST_IOSCuttingGeometry))
			{
				return true;
			}
			else
			{
				return false;
			}
        }






		public static OBJGeometryModel ParseOBJFile(string filePath)
		{
			OBJGeometryModel objModel = new OBJGeometryModel();

			try
			{
				using (StreamReader reader = new StreamReader(filePath))
				{
					string line;
					Regex vertexPattern = new Regex(@"^v\s");
					Regex facePattern = new Regex(@"^f\s");

					while ((line = reader.ReadLine()) != null)
					{
						if (vertexPattern.IsMatch(line))
						{
							// Parse vertex
							string[] parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
							if (parts.Length >= 4)
							{
								double x = double.Parse(parts[1]);
								double y = double.Parse(parts[2]);
								double z = double.Parse(parts[3]);
								objModel.Vertices.Add(new XYZ(x, y, z));
							}
						}
						else if (facePattern.IsMatch(line))
						{
							// Parse face
							string[] parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
							if (parts.Length >= 4)
							{
								int v1 = int.Parse(parts[1].Split('/')[0]) - 1;
								int v2 = int.Parse(parts[2].Split('/')[0]) - 1;
								int v3 = int.Parse(parts[3].Split('/')[0]) - 1;
								objModel.Faces.Add(new int[] { v1, v2, v3 });
							}
						}
					}
				}

				return objModel;
			}
			catch (Exception ex)
			{
				TaskDialog.Show("Error", $"Failed to parse OBJ file: {ex.Message}");
				return null;
			}
		}

		public static Solid CreateSolidFromOBJData(OBJGeometryModel objModel)
		{
			//try
			//{
			//	// Create a list of triangles
			//	List<GeometryObject> triangles = new List<GeometryObject>();

			//	foreach (var face in objModel.Faces)
			//	{
			//		XYZ p1 = objModel.Vertices[face[0]];
			//		XYZ p2 = objModel.Vertices[face[1]];
			//		XYZ p3 = objModel.Vertices[face[2]];

			//		// Create a triangle
			//		List<Curve> curveLoop = new List<Curve>
			//{
			//	Line.CreateBound(p1, p2),
			//	Line.CreateBound(p2, p3),
			//	Line.CreateBound(p3, p1)
			//};

			//		CurveLoop loop = CurveLoop.Create(curveLoop);

			//		// Create a planar face
			//		PlanarFace planarFace = PlanarFace.Create(loop);
			//		triangles.Add(planarFace);
			//	}

			//	// Combine triangles into a solid
			//	Solid solid = BooleanOperationsUtils.ExecuteBooleanOperationMultipleSolids(
			//		new List<Solid>(), // Start with an empty list
			//		triangles,
			//		BooleanOperationsType.Union);

			//	return solid;
			//}
			//catch (Exception ex)
			//{
			//	TaskDialog.Show("Error", $"Failed to create solid: {ex.Message}");
			//	return null;
			//}
			return null;
		}
	}
}
