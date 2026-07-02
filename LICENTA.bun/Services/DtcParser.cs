using System;
using System.Collections.Generic;

namespace LICENTA.bun
{
	public static class DtcParser
	{
		public static List<string> Parse(string raspunsHex)
		{
			List<string> coduriEroare = new List<string>();

			if (string.IsNullOrEmpty(raspunsHex) || raspunsHex.Contains("NO DATA") || raspunsHex.Contains("ERROR"))
				return coduriEroare;

			string dateCurate = raspunsHex.Replace("03", "").Trim();
			string[] partiECU = dateCurate.Split(new string[] { "43" }, StringSplitOptions.RemoveEmptyEntries);

			foreach (string bucata in partiECU)
			{
				string[] elemente = bucata.Split(new char[] { ' ', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

				for (int i = 0; i < elemente.Length - 1; i += 2)
				{
					string byteA_Hex = elemente[i];
					string byteB_Hex = elemente[i + 1];

					if (byteA_Hex == "00" && byteB_Hex == "00") continue;

					try
					{
						int byteA = Convert.ToInt32(byteA_Hex, 16);
						int byteB = Convert.ToInt32(byteB_Hex, 16);

						int categorie = (byteA >> 6) & 0x03;
						string litera = "P";
						if (categorie == 1) litera = "C";
						else if (categorie == 2) litera = "B";
						else if (categorie == 3) litera = "U";

						int cifra2 = (byteA >> 4) & 0x03;
						int cifra3 = byteA & 0x0F;

						string codFinal = string.Format("{0}{1}{2:X1}{3:X2}", litera, cifra2, cifra3, byteB);

						if (!coduriEroare.Contains(codFinal))
						{
							coduriEroare.Add(codFinal);
						}
					}
					catch (Exception ex)
					{
						System.Diagnostics.Debug.WriteLine("DTC parsing error: " + ex.Message);
					}
				}
			}
			return coduriEroare;
		}
	}
}