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





        // I used chatGPT to help me to build this parse method
        // because I have never converted an OBJ file to a Revit geometry before
        // and this documentation: https://en.wikipedia.org/wiki/Wavefront_.obj_file
        public static List<OBJGeometryModel> ParseOBJFile(string filePath)
		{
			// Instantiate a list of OBJ model
			List<OBJGeometryModel> objModels = new List<OBJGeometryModel>();

			try
			{
				using (StreamReader reader = new StreamReader(filePath))
				{
					string line;
					// ChatGPT suggested me to use Regex to parse the OBJ file
					// and I found this a great idea because it's more readable and maintainable
					Regex objectPattern = new Regex(@"^o\s");
					Regex vertexPattern = new Regex(@"^v\s");
					//Regex vertexPattern = new Regex(@"^v");
                    Regex facePattern = new Regex(@"^f\s");

					OBJGeometryModel objModel = new OBJGeometryModel();
					while ((line = reader.ReadLine()) != null)
					{
						// Same as the previous method, I'm showing a message box with the object info
						if ((Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)) && (objectPattern.IsMatch(line) || vertexPattern.IsMatch(line) || facePattern.IsMatch(line)))
						{
							TaskDialog.Show("Info", $"Parsing new line: {line}\n\nObjectInfo so far:\nVertices: {objModel.Vertices.Count}\nFaces: {objModel.Faces.Count}\nName: {objModel.Name}\n\nObjects so far: {objModels.Count}");
						}

						// If the line starts with "o " it means that it's a new object
						if (objectPattern.IsMatch(line))
                        {
                            // Start a new object
                            if (objModel.Vertices.Count > 0 && objModel.Faces.Count > 0)
                            {
								// Add the current object to the list and start a new one
                                objModels.Add(objModel);
                                objModel = new OBJGeometryModel();
                            }
                            objModel.Name = line.Substring(2).Trim();
                        }
						// If the line starts with "v " it means that it's a vertex
						else if (vertexPattern.IsMatch(line))
						{
                            // One this that I learn about the OBJ coordinates system
                            // is that it's different from Revit's, the Y axis points up,
                            // while Revit's Z axis points up
                            // So I need to swap the Y and Z coordinates
                            // Also, the vertex is made of 4 coordinates (x,y,z[,w])
							// but I'm only interested in the first 3
                            string[] parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
							if (parts.Length >= 4)
							{
								// Here I'm starting with index 1 because the first element is the "v" character
								double x = double.Parse(parts[1]);
								double y = double.Parse(parts[2]);
								double z = double.Parse(parts[3]);
								objModel.Vertices.Add(new XYZ(x, z, y));
                            }
						}
						// If the line starts with "f " it means that it's a face
						else if (facePattern.IsMatch(line))
						{
							// Parse face
							string[] parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
							if (parts.Length >= 4)
							{
                                // The face structure is f v1/vt1/vn1 v2/vt2/vn2 v3/vt3/vn3 ...
								// So I just need the first item of each part
								List<int> face = new List<int>();
                                for (int i = 1; i < parts.Length; i++)
								{
                                    int v = int.Parse(parts[i].Split('/')[0]);
                                    face.Add(v);
                                }
								objModel.Faces.Add(face);
                            }
						}

						// Another important thing that I learned is that the OBJ file
						// doesn't have a unit system, so I need to scale the geometry
						// to fit the Revit's unit system
						// I could add a dropdown to the user to select the unit system
						// or I could add a text box to the user to input the scale factor
						// but, for now, I'm just skipping this part
					}

                    // Add the last object to the list
                    if (objModel.Vertices.Count > 0 && objModel.Faces.Count > 0)
                    {
                        objModels.Add(objModel);
                    }
                }

				return objModels;
			}
			catch (Exception ex)
			{
				TaskDialog taskDialog = new TaskDialog("Error");
				taskDialog.MainInstruction = "Failed to parse OBJ file.";
				taskDialog.ExpandedContent = ex.Message + "\n" + ex.StackTrace;
				taskDialog.Show();
				return null;
			}
		}

		// ChatGPT also helped me to build this method
		// I'm using the TessellatedShapeBuilder to create a mesh
		// and extracting it as GeometryObject to be used in the DirectShape
		public static IList<GeometryObject> CreateRevitGeometryObjectFromOBJData(OBJGeometryModel objModel)
		{
			try
			{
				// Create the geometry using the Revit API
				TessellatedShapeBuilder builder = new TessellatedShapeBuilder();
				builder.OpenConnectedFaceSet(false);

				// After a couple of hours trying to understand WHY THE ICOSPHERE WASN'T WORKING?!?!?!?!
				// I found that the lowest vertex index for this element wasn't 1, but 9 (???)
				// I still don't know why it happened (file error?? But the file worked on web OBJ viewer)
				// Anyway, testing I discovered that if I subtract the lowest vertex index instead of 1
				// from all the vertices, the geometry is created correctly
				// So, assuming the lowest vertex usually will be 1
				// This should work for most cases, at least for the OBJ you guys sent worked fine
				int min = objModel.Faces.SelectMany(f => f).Min();

				foreach (List<int> face in objModel.Faces)
				{
					List<XYZ> vertices = new List<XYZ>();
					foreach (int vertexIndex in face)
                    {
                        XYZ vertex = objModel.Vertices[vertexIndex - min];
                        vertices.Add(vertex);
                    }

					// Here I'm supposed to set the material of the face
					// but as the task doesn't specify adding materials to the faces
					// I'm skipping this part
					TessellatedFace tessellatedFace = new TessellatedFace(vertices, ElementId.InvalidElementId);
					builder.AddFace(tessellatedFace);
				}

				builder.CloseConnectedFaceSet();
				builder.Build();

                // Extract the result from the builder
                TessellatedShapeBuilderResult result = builder.GetBuildResult();
				IList<GeometryObject> geoObjects = new List<GeometryObject>();
				geoObjects = result.GetGeometricalObjects().ToList();

                return geoObjects;
            }
			catch (Exception e)
			{
				TaskDialog taskDialog = new TaskDialog("Error");
				taskDialog.MainInstruction = "Failed to create solid geometry from OBJ data.";
				taskDialog.ExpandedContent = e.Message + "\n" + e.StackTrace;
				taskDialog.Show();
				return null;
			}
		}
	}
}
