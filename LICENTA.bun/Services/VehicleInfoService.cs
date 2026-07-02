using System;
using System.Threading.Tasks;

namespace LICENTA.bun
{
	public class VehicleInfo
	{
		public string EcuName { get; set; } = "-";
		public string VIN { get; set; } = "-";
		public string CalID { get; set; } = "-";
		public string CVN { get; set; } = "-";
	}

	public static class VehicleInfoService
	{
		public static async Task<VehicleInfo> GetVehicleDataAsync(ScannerOBD scanner)
		{
			VehicleInfo info = new VehicleInfo();

			string raspunsNume = await scanner.TrimiteComandaLungaAsync("090A");
			info.EcuName = Service09Decoder.DecodeAscii(raspunsNume);

			string raspunsVIN = await scanner.TrimiteComandaLungaAsync("0902");
			info.VIN = Service09Decoder.DecodeVIN(raspunsVIN);

			string raspunsCalID = await scanner.TrimiteComandaLungaAsync("0904");
			info.CalID = Service09Decoder.DecodeAscii(raspunsCalID);

			string raspunsCVN = await scanner.TrimiteComandaLungaAsync("0906");
			info.CVN = Service09Decoder.DecodeCVN(raspunsCVN);

			return info;
		}
	}
}