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
	public class ImportObjGeometryService
	{
		private readonly UIDocument _uidoc;
		private readonly Document _doc;

		public ImportObjGeometryService(ExternalCommandData commandData)
		{
			_uidoc = commandData.Application.ActiveUIDocument;
			_doc = _uidoc.Document;
		}

		public void ImportOBJFile(string objFilePath)
		{
			if (string.IsNullOrEmpty(objFilePath))
			{
				TaskDialog.Show("Error", "OBJ file path is invalid.");
				return;
			}

			OBJGeometryModel objModel = GeometryHelper.ParseOBJFile(objFilePath);

			if (objModel == null)
			{
				TaskDialog.Show("Error", "Failed to parse OBJ file.");
				return;
			}

			using (Transaction trans = new Transaction(_doc, "Import OBJ Geometry"))
			{
				trans.Start();

				// Create DirectShape to hold the geometry
				DirectShape ds = DirectShape.CreateElement(_doc, new ElementId(BuiltInCategory.OST_GenericModel));

				// Build Solid geometry from Mesh
				Solid solid = GeometryHelper.CreateSolidFromOBJData(objModel);

				if (solid != null)
				{
					ds.SetShape(new GeometryObject[] { solid });
				}
				else
				{
					TaskDialog.Show("Error", "Failed to create solid geometry from OBJ data.");
				}

				trans.Commit();
			}
		}
	}
}
