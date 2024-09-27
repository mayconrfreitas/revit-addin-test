using Autodesk.Revit.UI;
using RevitAddinTest.Helpers;
using RevitAddinTest.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitAddinTest.Services
{
	public class ReportService
	{
		public void GenerateRoomDataReport(List<RoomModel> rooms)
		{
            string filePath = "";
            try
			{
                filePath = FileHelper.GetFilePath("CSV File", "CSV Files (*.csv)|*.csv");
                if (string.IsNullOrEmpty(filePath))
                {
                    throw new Exception("Invalid file path");
                }

                StringBuilder csvContent = new StringBuilder();
                csvContent.AppendLine("Room Name;Room Number;Area (sqm);Volume (cubic m);Occupied Volume (cubic m);Utilization Ratio (%);Utilization Category");

                foreach (var room in rooms)
                {
                    string line = $"{room.Name};{room.Number};{room.Area:F2};{room.Volume:F2};{room.OccupiedVolume:F2};{room.UtilizationRatio * 100:F2};{room.UtilizationCategory}";
                    csvContent.AppendLine(line);
                }

                if (!filePath.EndsWith(".csv"))
                {
                    filePath += ".csv";
                }

                File.WriteAllText(filePath, csvContent.ToString());
            }
			catch (Exception e)
			{
				TaskDialog taskDialog = new TaskDialog("Error");
				taskDialog.MainInstruction = "Error generating room data report";
				taskDialog.ExpandedContent = e.Message + "\n" + e.StackTrace;
				taskDialog.Show();
			}

			TaskDialog.Show("Report Generated", $"Room data report has been generated at:\n{filePath}");
		}
	}
}
