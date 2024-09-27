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

namespace RevitAddinTest.Helpers
{
	public static class GeometryHelper
	{
		public static double CalculateOccupiedVolume(Document doc, Room room)
		{
			double occupiedVolume = 0.0;

			// Get all family instances in the room, excluding doors/windows/voids
			BoundingBoxXYZ roomBBox = room.get_BoundingBox(null);

			Outline roomOutline = new Outline(roomBBox.Min, roomBBox.Max);
			BoundingBoxIntersectsFilter bboxFilter = new BoundingBoxIntersectsFilter(roomOutline);

			FilteredElementCollector collector = new FilteredElementCollector(doc)
				.WherePasses(bboxFilter)
				.OfClass(typeof(FamilyInstance))
				.WhereElementIsNotElementType();

			foreach (FamilyInstance instance in collector)
			{
				// Exclude doors, windows, and voids
				if (instance.Category.Id.Value == (int)BuiltInCategory.OST_Doors ||
					instance.Category.Id.Value == (int)BuiltInCategory.OST_Windows ||
					instance.Symbol.FamilyName.Contains("Void"))
				{
					continue;
				}

				// Calculate volume
				double volume = 0.0;
				Parameter volumeParam = instance.get_Parameter(BuiltInParameter.HOST_VOLUME_COMPUTED);

				if (volumeParam != null && volumeParam.HasValue)
				{
					volume = volumeParam.AsDouble(); // Volume in cubic feet
					occupiedVolume += volume;
				}
			}

			// Convert volume to cubic meters
			return UnitUtils.ConvertFromInternalUnits(occupiedVolume, UnitTypeId.CubicMeters);
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
