using System;
using System.IO;

namespace LICENTA.bun
{
	public class CsvLogger
	{
		private StreamWriter fisierCSV;
		private string caleFisierCSV = "";
		public string StartNewLog(string vin)
		{
			try
			{
				string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
				caleFisierCSV = desktopPath + "\\CarTrace_Log_" + DateTime.Now.ToString("yyyy.MM.dd_HH.mm.ss") + ".csv";

				fisierCSV = new StreamWriter(caleFisierCSV, true);

				fisierCSV.WriteLine($"Vehicle VIN:;{vin};");
				fisierCSV.WriteLine("Time(hh:mm:ss.ms);Sensor;Value");

				return caleFisierCSV;
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine("Error creating CSV file: " + ex.Message);
				throw;
			}
		}

		public bool LogData(string numeSenzor, double valoare)
		{
			if (fisierCSV != null)
			{
				try
				{
					string timpCurent = DateTime.Now.ToString("HH:mm:ss.fff");
					string senzorCurat = numeSenzor.Replace(";", "_");
					fisierCSV.WriteLine($"{timpCurent};{senzorCurat};{valoare.ToString(System.Globalization.CultureInfo.InvariantCulture)}");
					return true; 
				}
				catch (Exception ex)
				{
					System.Diagnostics.Debug.WriteLine("CSV writing error: " + ex.Message);
					return false; 
				}
			}
			return false;
		}

		public string StopLog()
		{
			if (fisierCSV != null)
			{
				try
				{
					fisierCSV.Close();
					fisierCSV.Dispose();
					fisierCSV = null;
					return caleFisierCSV;
				}
				catch (Exception ex)
				{
					System.Diagnostics.Debug.WriteLine("CSV closing error: " + ex.Message);
				}
			}
			return string.Empty;
		}
	}
}