using System;

namespace LICENTA.bun
{
	public class PerformanceResult
	{
		public double Time50 { get; set; }
		public double Time100 { get; set; }
		public double Time130 { get; set; }
		public bool Completed50 { get; set; }
		public bool Completed100 { get; set; }
		public bool Completed130 { get; set; }
	}

	public static class PerformanceService
	{
		public static double InterpoleazaTimp(double v1, double v2, double t1, double t2, double tinta)
		{
			if (Math.Abs(v2 - v1) < 0.0001) return t2;
			return t1 + (tinta - v1) * (t2 - t1) / (v2 - v1);
		}
	}
}