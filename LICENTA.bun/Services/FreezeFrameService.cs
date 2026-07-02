using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LICENTA.bun
{
	public static class FreezeFrameService
	{
		public static async Task<List<string>> ScanAsync(ScannerOBD scanner)
		{
			var rezultate = new List<string>();
			var pidsToScan = new List<string>();

			string r100 = await scanner.TrimiteComandaAsync("0100");
			pidsToScan.AddRange(ExtrageBytiDinHarta(r100, 0));

			if (pidsToScan.Contains("20"))
			{
				string r120 = await scanner.TrimiteComandaAsync("0120");
				pidsToScan.AddRange(ExtrageBytiDinHarta(r120, 32));
			}

			if (pidsToScan.Contains("40"))
			{
				string r140 = await scanner.TrimiteComandaAsync("0140");
				pidsToScan.AddRange(ExtrageBytiDinHarta(r140, 64));
			}

			pidsToScan.Remove("20");
			pidsToScan.Remove("40");
			pidsToScan.Remove("60");

			foreach (string pid in pidsToScan)
			{
				string comanda = "02" + pid + "00";
				string raspunsSenzor = await scanner.TrimiteComandaAsync(comanda);
				string decodat = scanner.DecodeazaRaspuns("02" + pid, raspunsSenzor);

				if (!decodat.Contains("NO DATA") &&	!decodat.Contains("NODATA") && !decodat.Contains("N/A") && !decodat.Contains("Missing") &&
					!decodat.Contains("Error") && !decodat.Contains("Unknown") && !decodat.Contains("Raw response") && !decodat.Contains("?"))
				{
					rezultate.Add("PID " + pid + " : " + decodat);
				}
			}

			return rezultate;
		}

		private static List<string> ExtrageBytiDinHarta(string raspunsHex, int startPid)
		{
			var pids = new List<string>();
			string curat = raspunsHex.Replace(" ", "").Replace("\r", "").Replace("\n", "");

			int index41 = curat.IndexOf("41");
			if (index41 != -1 && curat.Length >= index41 + 12)
			{
				string utilHex = curat.Substring(index41 + 4, 8);
				try
				{
					uint mask = Convert.ToUInt32(utilHex, 16);
					for (int i = 0; i < 32; i++)
					{
						if ((mask & (1U << (31 - i))) != 0)
						{
							int pidNumeric = startPid + i + 1;
							pids.Add(pidNumeric.ToString("X2"));
						}
					}
				}
				catch (Exception ex)
				{
					System.Diagnostics.Debug.WriteLine("PID map decoding error (Bitmask): " + ex.Message);
				}
			}
			return pids;
		}
	}
}