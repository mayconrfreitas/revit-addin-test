using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitAddinTest.Models
{
	public class OBJGeometryModel
	{
		public List<XYZ> Vertices { get; set; }
		public List<int[]> Faces { get; set; }

		public OBJGeometryModel()
		{
			Vertices = new List<XYZ>();
			Faces = new List<int[]>();
		}
	}
}
