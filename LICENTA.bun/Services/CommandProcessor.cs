using System.Threading.Tasks;

namespace LICENTA.bun
{
	public class CommandResult
	{
		public string RawHex { get; set; }
		public string ConsoleMessage { get; set; }
		public bool HasHistory { get; set; }
		public string HistoryCategory { get; set; }
		public string HistoryDetail { get; set; }
	}

	public static class CommandProcessor
	{
		public static async Task<CommandResult> ExecuteAsync(string comanda, ScannerOBD scanner)
		{
			CommandResult rezultat = new CommandResult();

			if (comanda.StartsWith("AT"))
			{
				rezultat.RawHex = await scanner.TrimiteComandaAsync(comanda);
				rezultat.ConsoleMessage = "[AT COMMAND RESPONSE] " + rezultat.RawHex;
				rezultat.HasHistory = true;
				rezultat.HistoryCategory = "SYSTEM";
				rezultat.HistoryDetail = "Sent AT command: " + comanda;

				return rezultat;
			}
			else
			{
				rezultat.RawHex = await scanner.TrimiteComandaAsync(comanda);
				string valoareDecodata = scanner.DecodeazaRaspuns(comanda, rezultat.RawHex);

				rezultat.ConsoleMessage = "[DECODED] " + valoareDecodata;
				rezultat.HasHistory = true;
				rezultat.HistoryCategory = "EXTRACT";
				rezultat.HistoryDetail = $"{comanda} : {valoareDecodata}";

				return rezultat;
			}
		}
	}
}