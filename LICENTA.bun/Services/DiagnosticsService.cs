using System.Collections.Generic;
using System.Threading.Tasks;

namespace LICENTA.bun
{
	public class DiagnosticsResult
	{
		public bool Success { get; set; }
		public List<string> ErrorCodes { get; set; } = new List<string>();
		public string RawResponse { get; set; }
	}

	public static class DiagnosticsService
	{
		public static async Task<DiagnosticsResult> ReadErrorsAsync(ScannerOBD scanner)
		{
			DiagnosticsResult result = new DiagnosticsResult();
			result.RawResponse = await scanner.TrimiteComandaLungaAsync("03");
			result.ErrorCodes = scanner.DecodeazaService03_DTC(result.RawResponse);
			result.Success = true;
			return result;
		}

		public static async Task<DiagnosticsResult> ClearErrorsAsync(ScannerOBD scanner)
		{
			DiagnosticsResult result = new DiagnosticsResult();
			result.RawResponse = await scanner.TrimiteComandaLungaAsync("04");

			if (result.RawResponse.Contains("44") || result.RawResponse.Contains("OK"))
			{
				result.Success = true;
			}
			else
			{
				result.Success = false;
			}
			return result;
		}
	}
}