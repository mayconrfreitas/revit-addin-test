using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace RevitAddinTest.Helpers
{
	public static class RevitAPIHelper
	{
		public static bool GetVolumeCalculation(Document doc)
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
	}
}
