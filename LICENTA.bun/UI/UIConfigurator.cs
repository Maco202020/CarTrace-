using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Drawing;
using System.Windows.Forms.DataVisualization.Charting;

namespace LICENTA.bun
{
	public static class UIConfigurator
	{
		public static void ConfigureChart(Chart chartSenzor)
		{
			if (chartSenzor == null) return;

			chartSenzor.BackColor = Color.FromArgb(24, 24, 28);
			chartSenzor.ChartAreas[0].BackColor = Color.FromArgb(30, 30, 36);

			chartSenzor.ChartAreas[0].AxisX.LabelStyle.ForeColor = Color.White;
			chartSenzor.ChartAreas[0].AxisY.LabelStyle.ForeColor = Color.White;

			chartSenzor.ChartAreas[0].AxisX.LineColor = Color.Gray;
			chartSenzor.ChartAreas[0].AxisY.LineColor = Color.Gray;

			chartSenzor.ChartAreas[0].AxisX.MajorGrid.LineColor = Color.FromArgb(50, 50, 55);
			chartSenzor.ChartAreas[0].AxisY.MajorGrid.LineColor = Color.FromArgb(50, 50, 55);

			chartSenzor.BorderlineColor = Color.White;
			chartSenzor.BorderlineWidth = 1;
			chartSenzor.BorderlineDashStyle = ChartDashStyle.Solid;

			if (chartSenzor.Series.Count > 0)
			{
				chartSenzor.Series[0].Color = Color.Cyan;
				chartSenzor.Series[0].BorderWidth = 3;
			}
		}
	}
}