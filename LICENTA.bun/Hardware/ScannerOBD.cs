using System;
using System.IO.Ports;
using System.Linq;
using System.Threading.Tasks;

namespace LICENTA.bun
{
	public class ScannerOBD
	{
		private readonly SerialPort port;

		public ScannerOBD()
		{
			port = new SerialPort
			{
				BaudRate = 115200,
				DataBits = 8,
				Parity = Parity.None,
				StopBits = StopBits.One,
				ReadTimeout = 5000,
				WriteTimeout = 5000,
				Handshake = Handshake.None,
				DtrEnable = false,
				RtsEnable = false
			};
		}
		public async Task<bool> ConectareAsync(string numePort)
		{
			try
			{
				if (port.IsOpen) port.Close();
				port.PortName = numePort;
				port.Open();

				await Task.Delay(1000);
				port.Write("ATZ\r");
				await Task.Delay(1000);
				string gunoi = port.ReadExisting();

				return true;
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine("Connection error: " + ex.Message);
				return false;
			}
		}
		public void Deconectare()
		{
			if (port != null && port.IsOpen)
			{
				try
				{
					port.Close();
				}
				catch (Exception ex)
				{
					System.Diagnostics.Debug.WriteLine("Error closing port: " + ex.Message);
				}
			}
		}
		public async Task<string> TrimiteComandaAsync(string comanda)
		{
			try
			{
				if (!port.IsOpen) return "Error: Port closed";
				port.DiscardInBuffer();
				port.DiscardOutBuffer();

				port.Write(comanda + "\r");

				string raspuns = "";
				DateTime startTime = DateTime.Now;

				while (true)
				{
					if ((DateTime.Now - startTime).TotalMilliseconds > 2500)
					{
						return "TIMEOUT: ECU did not respond / NO DATA";
					}

					if (port.BytesToRead > 0)
					{
						raspuns += port.ReadExisting();
						if (raspuns.Contains(">"))
						{
							break;
						}
					}
					await Task.Delay(50);
				}
				return raspuns.Replace(">", "").Replace("\r", " ").Replace("\n", " ").Trim();
			}
			catch (Exception ex)
			{
				return "HARDWARE ERROR: " + ex.Message;
			}
		}

		public async Task<string> TrimiteComandaLungaAsync(string comanda)
		{
			try
			{
				if (!port.IsOpen) return "Error: Port closed";

				port.DiscardInBuffer();
				port.DiscardOutBuffer();

				port.Write(comanda + "\r");

				string raspuns = "";
				DateTime startTime = DateTime.Now;

				while (true)
				{
					if ((DateTime.Now - startTime).TotalMilliseconds > 6000)
					{
						return "TIMEOUT: ECU did not respond / NO DATA";
					}

					if (port.BytesToRead > 0)
					{
						raspuns += port.ReadExisting();
						if (raspuns.Contains(">"))
						{
							break;
						}
					}
					await Task.Delay(50);
				}

				return raspuns.Replace(">", "").Replace("\r", " ").Replace("\n", " ").Trim();
			}
			catch (Exception ex)
			{
				return "HARDWARE ERROR: " + ex.Message;
			}
		}
		public string DecodeazaRaspuns(string comandaPID, string raspunsHex)
		{
			if (raspunsHex.Contains("NO DATA") || raspunsHex.Contains("ERROR") || raspunsHex.Contains("?") || raspunsHex.Contains("7F"))
				return "No data / ECU Error";

			string hexCurat = raspunsHex.Replace(" ", "").Replace("\r", "").Replace("\n", "");

			string mode = comandaPID.Substring(0, 2);
			string pidPortion = comandaPID.Substring(2, 2);

			string prefixAsteptat = (mode == "01" ? "41" : "42") + pidPortion;

			if (hexCurat.Contains(prefixAsteptat))
			{
				int startPos = hexCurat.IndexOf(prefixAsteptat);
				int offsetDate = (mode == "02") ? 6 : 4;
				int index = startPos + offsetDate;

				try
				{
					int A = 0, B = 0, C = 0, D = 0;
					if (hexCurat.Length >= index + 2) A = Convert.ToInt32(hexCurat.Substring(index, 2), 16);
					if (hexCurat.Length >= index + 4) B = Convert.ToInt32(hexCurat.Substring(index + 2, 2), 16);
					if (hexCurat.Length >= index + 6) C = Convert.ToInt32(hexCurat.Substring(index + 4, 2), 16);
					if (hexCurat.Length >= index + 8) D = Convert.ToInt32(hexCurat.Substring(index + 6, 2), 16);

					return ObdTranslator.GetDecodedValue(pidPortion, A, B, C, D);
				}
				catch (Exception ex)
				{
					System.Diagnostics.Debug.WriteLine("Mathematical error when decoding PID: " + ex.Message);
					return "Math calculation error.";
				}
			}
			return "Raw response: " + raspunsHex;
		}

		public string DecodeazaVIN(string raspunsHex)
		{
			return Service09Decoder.DecodeVIN(raspunsHex);
		}

		public string DecodeazaService09_ASCII(string raspunsHex)
		{
			return Service09Decoder.DecodeAscii(raspunsHex);
		}

		public string DecodeazaService09_CVN(string raspunsHex)
		{
			return Service09Decoder.DecodeCVN(raspunsHex);
		}

		public System.Collections.Generic.List<string> DecodeazaService03_DTC(string raspunsHex)
		{
			return DtcParser.Parse(raspunsHex);
		}
	}
}