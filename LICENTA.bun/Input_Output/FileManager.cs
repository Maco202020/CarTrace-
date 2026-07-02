using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace LICENTA.bun
{
	public static class FileManager
	{
		public static string ExportConsole(string[] liniiConsola)
		{
			string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
			string caleFisier = desktopPath + "\\CarTrace_Console_" + DateTime.Now.ToString("yyyy.MM.dd_HH.mm.ss") + ".csv";

			using (StreamWriter fisierCSV = new StreamWriter(caleFisier, false, System.Text.Encoding.UTF8))
			{
				fisierCSV.WriteLine("Message Type;Details");

				foreach (string linie in liniiConsola)
				{
					if (string.IsNullOrWhiteSpace(linie) || linie.Contains("---"))
						continue;

					string linieCurata = linie.Replace(";", "_").Trim();

					if (linieCurata.StartsWith("> SENT:"))
						fisierCSV.WriteLine($"COMMAND;{linieCurata.Replace("> SENT:", "").Trim()}");
					else if (linieCurata.StartsWith("[RAW HEX]"))
						fisierCSV.WriteLine($"RAW HEX;{linieCurata.Replace("[RAW HEX]", "").Trim()}");
					else if (linieCurata.StartsWith("[DECODED]"))
						fisierCSV.WriteLine($"DECODED;{linieCurata.Replace("[DECODED]", "").Trim()}");
					else
						fisierCSV.WriteLine($"INFO;{linieCurata}");
				}
			}
			return caleFisier;
		}

		public static string ExportHistory(List<string> liniiIstoric, string vin)
		{
			string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
			string caleFisier = desktopPath + "\\CarTrace_Session_" + DateTime.Now.ToString("yyyy.MM.dd_HH.mm.ss") + ".txt";

			using (StreamWriter fisier = new StreamWriter(caleFisier, false, System.Text.Encoding.UTF8))
			{
				fisier.WriteLine("--- DIAGNOSIS SESSION LOG ---");
				fisier.WriteLine("Date: " + DateTime.Now.ToString("dd/MM/yyyy HH:mm"));
				fisier.WriteLine("Vehicle VIN: " + vin);
				fisier.WriteLine("-------------------------------");

				foreach (string linie in liniiIstoric)
				{
					fisier.WriteLine(linie);
				}
			}
			return caleFisier;
		}

		public static string ExportPerformance(PerformanceResult res, string modelAuto)
		{
			string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
			string numeFisier = $"CarTrace_Performance_{DateTime.Now:yyyy.MM.dd_HH.mm.ss}.txt";
			string caleCompleta = System.IO.Path.Combine(desktopPath, numeFisier);

			StringBuilder sb = new StringBuilder();
			sb.AppendLine("========================================");
			sb.AppendLine("    CarTrace - PERFORMANCE     ");
			sb.AppendLine("========================================");
			sb.AppendLine($"Date: {DateTime.Now:dd/MM/yyyy HH:mm:ss}");
			sb.AppendLine($"Vehicle: {modelAuto}");
			sb.AppendLine("----------------------------------------");
			sb.AppendLine($"0 - 50 km/h:  {(res.Completed50 ? res.Time50.ToString("F2") + " s" : "N/A")}");
			sb.AppendLine($"0 - 100 km/h: {(res.Completed100 ? res.Time100.ToString("F2") + " s" : "N/A")}");
			sb.AppendLine($"0 - 130 km/h: {(res.Completed130 ? res.Time130.ToString("F2") + " s" : "N/A")}");
			sb.AppendLine("========================================");

			System.IO.File.WriteAllText(caleCompleta, sb.ToString());
			return caleCompleta;
		}
	}
}