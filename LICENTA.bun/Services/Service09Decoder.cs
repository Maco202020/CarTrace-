using System;

namespace LICENTA.bun
{
	public static class Service09Decoder
	{
		public static string DecodeVIN(string raspunsHex)
		{
			if (string.IsNullOrWhiteSpace(raspunsHex) || raspunsHex.Contains("NO DATA") || raspunsHex.Contains("ERROR"))
				return "Unavailable / No data";

			if (raspunsHex.Contains("7F 09") || raspunsHex.Contains("7F09"))
				return "Not Supported by ECU";

			string asciiRezultat = "";
			string[] elemente = raspunsHex.Split(new char[] { ' ', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

			foreach (string el in elemente)
			{
				if (el.Length == 2 && !el.Contains(":"))
				{
					try
					{
						int valoare = Convert.ToInt32(el, 16);
						if ((valoare >= 48 && valoare <= 57) || (valoare >= 65 && valoare <= 90))
						{
							asciiRezultat += (char)valoare;
						}
					}
					catch (Exception ex)
					{
						System.Diagnostics.Debug.WriteLine("HEX->ASCII to VIN conversion error: " + ex.Message);
					}
				}
			}

			if (asciiRezultat.Length >= 17)
			{
				return asciiRezultat.Substring(asciiRezultat.Length - 17, 17);
			}

			return "Unavailable (No valid VIN)";
		}

		public static string DecodeAscii(string raspunsHex)
		{
			if (string.IsNullOrWhiteSpace(raspunsHex) || raspunsHex.Contains("NO DATA") || raspunsHex.Contains("ERROR"))
				return "Unavailable";

			if (raspunsHex.Contains("7F 09") || raspunsHex.Contains("7F09"))
				return "Not Supported";

			string asciiRezultat = "";
			string[] elemente = raspunsHex.Split(new char[] { ' ', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

			bool gasit49 = false;
			foreach (string el in elemente)
			{
				if (el.Length == 2 && !el.Contains(":"))
				{
					if (!gasit49 && el == "49") { gasit49 = true; continue; }
					if (gasit49 && (el == "0A" || el == "04")) continue;

					if (gasit49)
					{
						try
						{
							int valoare = Convert.ToInt32(el, 16);
							if (valoare >= 32 && valoare <= 126)
							{
								asciiRezultat += (char)valoare;
							}
						}
						catch (Exception ex)
						{
							System.Diagnostics.Debug.WriteLine("Conversion error Service 09: " + ex.Message);
						}
					}
				}
			}
			return string.IsNullOrEmpty(asciiRezultat) ? "No data" : asciiRezultat.Trim();
		}

		public static string DecodeCVN(string raspunsHex)
		{
			if (string.IsNullOrWhiteSpace(raspunsHex) || raspunsHex.Contains("NO DATA") || raspunsHex.Contains("ERROR"))
				return "Unavailable";

			if (raspunsHex.Contains("7F 09") || raspunsHex.Contains("7F09"))
				return "Not Supported";

			string hexRezultat = "";
			string[] elemente = raspunsHex.Split(new char[] { ' ', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

			bool gasit49 = false;
			foreach (string el in elemente)
			{
				if (el.Length == 2 && !el.Contains(":"))
				{
					if (!gasit49 && el == "49") { gasit49 = true; continue; }
					if (gasit49 && el == "06") continue;

					if (gasit49)
					{
						hexRezultat += el;
					}
				}
			}
			return string.IsNullOrEmpty(hexRezultat) ? "No data" : hexRezultat;
		}
	}
}