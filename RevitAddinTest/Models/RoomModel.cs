using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitAddinTest.Models
{
	public class RoomModel
	{
		public string Name { get; set; }
		public string Number { get; set; }
		public double Area { get; set; }
		public double Volume { get; set; }
		public double OccupiedVolume { get; set; }
		public double UtilizationRatio { get; set; }
		public string UtilizationCategory { get; set; }
		public Element RoomElement { get; set; }
	}
}
