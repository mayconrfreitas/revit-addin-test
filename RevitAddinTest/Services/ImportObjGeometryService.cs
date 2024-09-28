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
	// As in the Room data extraction I separated the logic of importing OBJ geometry
	// into a separate service class to make the code more modular and easier to maintain
	public class ImportObjGeometryService
	{
		private readonly ExternalCommandData _commandData;
        private readonly UIDocument _uidoc;
		private readonly Document _doc;

		public ImportObjGeometryService(ExternalCommandData commandData)
		{
            _commandData = commandData;
            _uidoc = commandData.Application.ActiveUIDocument;
			_doc = _uidoc.Document;
		}

		// Method to import OBJ geometry into Revit
		public void ImportOBJFile(string objFilePath)
		{
			if (string.IsNullOrEmpty(objFilePath))
			{
				TaskDialog.Show("Error", "OBJ file path is invalid.");
				return;
			}

			// Parse OBJ file
			List<OBJGeometryModel> objModels = GeometryHelper.ParseOBJFile(objFilePath);

			if (objModels == null || !objModels.Any())
			{
				TaskDialog.Show("Error", "Failed to parse OBJ file.");
				return;
			}

            List<Element> directShapes = new List<Element>();

            using (Transaction trans = new Transaction(_doc, "Import OBJ Geometry"))
			{
				trans.Start();

				foreach (OBJGeometryModel objModel in objModels)
                {
                    // Create DirectShape to hold the geometry
                    DirectShape directShape = DirectShape.CreateElement(_doc, new ElementId(BuiltInCategory.OST_GenericModel));

                    // Create the Revit Geometry Objects from the OBJ data parsed
                    IList<GeometryObject> geometryObjects = GeometryHelper.CreateRevitGeometryObjectFromOBJData(objModel);

                    if (geometryObjects != null && geometryObjects.Any())
                    {
                        // Set the shape of the DirectShape using the Solid geometry
                        directShape.SetShape(geometryObjects);
                        directShape.Name = objModel.Name;

						directShapes.Add(directShape);
                    }
                    else
                    {
                        TaskDialog.Show("Error", "Failed to create solid geometry from OBJ data.");
                    }
                }

				trans.Commit();
			}

			if (directShapes.Any())
            {
				RevitAPIHelper.ZoomToElements(_commandData, directShapes);
                TaskDialog.Show("Success", "OBJ geometry imported successfully.");
            }
            else
            {
                TaskDialog.Show("Error", "Failed to import OBJ geometry.");
            }
		}
	}
}
