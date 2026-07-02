using Postgrest.Attributes;
using Postgrest.Models;
using Supabase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Configuration;

namespace LICENTA.bun
{
	[Table("history_logs")]
	public class HistoryLog : BaseModel
	{
		[Column("category")]
		public string Category { get; set; }

		[Column("details")]
		public string Details { get; set; }

		[Column("vin")]
		public string VIN { get; set; }

		[Column("created_at")]
		public DateTime CreatedAt { get; set; }
	}

	public static class SupabaseService
	{
		private static readonly string url = ConfigurationManager.AppSettings["SupabaseUrl"];
		private static readonly string key = ConfigurationManager.AppSettings["SupabaseKey"];
		private static Client supabase;

		public static async Task InitializeAsync()
		{
			if (supabase == null)
			{
				if (string.IsNullOrEmpty(url) || string.IsNullOrEmpty(key))
				{
					throw new InvalidOperationException("CRITICAL EXCEPTION: Supabase credentials are missing from App.config!");
				}

				var options = new SupabaseOptions { AutoConnectRealtime = true };
				supabase = new Client(url, key, options);
				await supabase.InitializeAsync();
			}
		}

		public static async Task InsertLogAsync(string categorie, string detalii, string vin = "-")
		{
			try
			{
				await InitializeAsync();
				var newLog = new HistoryLog
				{
					Category = categorie,
					Details = detalii,
					VIN = vin,
					CreatedAt = DateTime.UtcNow
				};
				await supabase.From<HistoryLog>().Insert(newLog);
			}
			catch (Exception ex)
			{
				System.Windows.Forms.MessageBox.Show("Supabase Cloud Error: " + ex.Message, "Database Error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
			}
		}

		public static async Task<List<string>> GetUniqueVinsAsync()
		{
			try
			{
				await InitializeAsync();
				var result = await supabase.From<HistoryLog>().Get();

				return result.Models
					.Select(x => x.VIN)
					.Where(v => !string.IsNullOrEmpty(v) && v != "-")
					.Distinct()
					.ToList();
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine("Error Vins: " + ex.Message);
				return new List<string>();
			}
		}

		public static async Task<List<HistoryLog>> GetHistoryByVinAsync(string vinCautat)
		{
			try
			{
				await InitializeAsync();
				var result = await supabase
					.From<HistoryLog>()
					.Where(x => x.VIN == vinCautat)
					.Get();

				return result.Models;
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine("Error Logs: " + ex.Message);
				return new List<HistoryLog>();
			}
		}
	}
}