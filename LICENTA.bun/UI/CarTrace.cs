using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using Websocket.Client.Logging;

namespace LICENTA.bun
{
	public partial class Form1 : Form
	{
		private ScannerOBD scanner = new ScannerOBD();
		private CsvLogger csvLogger = new CsvLogger();
		private bool seIncarca = true;

		private ComboBox cmbPorturiNou;
		private Button btnConectareNou;
		private Panel pnlContinutPrincipal;

		private PerformanceResult ultimulTest = new PerformanceResult();
		private bool isTestingPerformance = false;
		private bool isConnected = false;
		private Stopwatch ceasPerformanta = new Stopwatch();
		private List<string> istoricSesiuneCurenta = new List<string>();

		public Form1()
		{
			InitializeComponent();
		}

		private async void Form1_Load(object sender, EventArgs e)
		{
			InitializeModernUI();
			ConstruiesteMainPanel();
			AplicaTemaIntunecataPanouriVechi();

			if (pnlExtract != null) pnlExtract.Visible = false;
			if (pnlMonitoring != null) pnlMonitoring.Visible = false;
			if (pnlDiagnosis != null) pnlDiagnosis.Visible = false;
			if (pnlFreezeFrame != null) pnlFreezeFrame.Visible = false;
			if (pnlIstoric != null) pnlIstoric.Visible = false;
			if (pnlPerformance != null) pnlPerformance.Visible = false;
			if (pnlMain != null) pnlMain.Visible = false;

			pnlContinutPrincipal.Visible = true;
			pnlContinutPrincipal.BringToFront();

			cmbComenzi.Items.Clear();
			cmbGraphSensor.Items.Clear();

			cmbComenzi.Items.AddRange(CommandRepository.GetExtractCommands());
			cmbGraphSensor.Items.AddRange(CommandRepository.GetGraphCommands());

			if (cmbComenzi.Items.Count > 0) cmbComenzi.SelectedIndex = 0;
			if (cmbGraphSensor.Items.Count > 0) cmbGraphSensor.SelectedIndex = 0;

			string[] porturiDisponibile = SerialPort.GetPortNames();
			if (cmbPorturiNou != null)
			{
				cmbPorturiNou.Items.AddRange(porturiDisponibile);
				if (cmbPorturiNou.Items.Count > 0)
					cmbPorturiNou.SelectedIndex = 0;
				else
				{
					cmbPorturiNou.Items.Add("No port found");
					cmbPorturiNou.SelectedIndex = 0;
				}
			}

			UIConfigurator.ConfigureChart(chartSenzor);

			seIncarca = false;
			await IncarcaListaVinuriAsync();
		}
		private async void BtnConectareNou_Click(object sender, EventArgs e)
		{
			string portSelectat = cmbPorturiNou.SelectedItem.ToString();

			if (portSelectat == "No port found")
			{
				MessageBox.Show("Please connect the ELM327 to your laptop's Bluetooth!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return;
			}

			btnConectareNou.Text = "CONNECTING...";
			btnConectareNou.Refresh();
			bool conectat = await scanner.ConectareAsync(portSelectat);

			if (conectat)
			{
				isConnected = true;
				if (lstIstoricGlobal != null) lstIstoricGlobal.Items.Clear();

				AdaugaInIstoric("SYSTEM", "Connected to port " + portSelectat);
				btnConectareNou.BackColor = Color.OliveDrab;
				VehicleInfo info = await VehicleInfoService.GetVehicleDataAsync(scanner);

				lblEcuNameValue1.Text = info.EcuName;
				lblVINValue.Text = info.VIN;
				lblCalIDValue.Text = info.CalID;
				lblCVNValue.Text = info.CVN;

				pnlContinutPrincipal.Controls.Clear();
				ConstruiesteMainPanel();

				btnConectareNou.Text = "CONNECTED (" + portSelectat + ")";
				btnConectareNou.BackColor = Color.OliveDrab;
				btnConectareNou.Refresh();
			   ///   btnConectareNou.BackColor = Color.OliveDrab;
				MessageBox.Show("Connection established successfully with ECU!", "CarTrace");
			}
			else
			{
				isConnected = false;
				btnConectareNou.Text = "CONNECTION ERROR";
				btnConectareNou.BackColor = Color.DarkRed;
				AdaugaInIstoric("SYSTEM", "Connection error to " + portSelectat);
				MessageBox.Show("Could not connect to the vehicle. Check Bluetooth and vehicle ignition!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		private void btnNavExtract_Click(object sender, EventArgs e)
		{
			pnlContinutPrincipal.Visible = false;
			pnlExtract.Visible = true;
			pnlExtract.BringToFront();
			AdaugaInIstoric("NAVIGATION", "Entered Extract panel.");
		}

		private void btnBackExtract_Click(object sender, EventArgs e)
		{
			pnlExtract.Visible = false;
			pnlContinutPrincipal.Visible = true;
			pnlContinutPrincipal.BringToFront();
		}

		private void btnNavMonitoring_Click(object sender, EventArgs e)
		{
			pnlContinutPrincipal.Visible = false;
			pnlMonitoring.Visible = true;
			pnlMonitoring.BringToFront();
			AdaugaInIstoric("NAVIGATION", "Entered Monitoring panel.");
		}

		private void btnBackMonitoring_Click(object sender, EventArgs e)
		{
			pnlMonitoring.Visible = false;
			pnlContinutPrincipal.Visible = true;
			pnlContinutPrincipal.BringToFront();
		}

		private void btnNavDiagnosis_Click(object sender, EventArgs e)
		{
			pnlContinutPrincipal.Visible = false;
			pnlDiagnosis.Visible = true;
			pnlDiagnosis.BringToFront();
			AdaugaInIstoric("NAVIGATION", "Entered Diagnosis panel.");
		}

		private void btnBackDiagnosis_Click(object sender, EventArgs e)
		{
			pnlDiagnosis.Visible = false;
			pnlContinutPrincipal.Visible = true;
			pnlContinutPrincipal.BringToFront();
		}

		private void btnNavFreezeFrame_Click(object sender, EventArgs e)
		{
			pnlContinutPrincipal.Visible = false;
			pnlFreezeFrame.Visible = true;
			pnlFreezeFrame.BringToFront();
			AdaugaInIstoric("NAVIGATION", "Entered Freeze Frame panel.");
		}

		private void btnBackFreezeFrame_Click(object sender, EventArgs e)
		{
			pnlFreezeFrame.Visible = false;
			pnlContinutPrincipal.Visible = true;
			pnlContinutPrincipal.BringToFront();
		}

		private async void btnNavIstoric_Click(object sender, EventArgs e)
		{
			pnlContinutPrincipal.Visible = false;
			pnlIstoric.Visible = true;
			pnlIstoric.BringToFront();

			if (cmbVehicule.Items.Count <= 1)
			{
				await IncarcaListaVinuriAsync();
			}
		}

		private void btnBackIstoric_Click(object sender, EventArgs e)
		{
			pnlIstoric.Visible = false;
			pnlContinutPrincipal.Visible = true;
			pnlContinutPrincipal.BringToFront();
		}

		private void cmbGraphSensor_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (seIncarca) return;

			if (cmbGraphSensor.SelectedItem != null)
			{
				string senzorSelectat = cmbGraphSensor.SelectedItem.ToString();
				string numeSenzor = senzorSelectat.Length > 7 ? senzorSelectat.Substring(7) : senzorSelectat;

				AdaugaInIstoric("MONITORING", "Sensor selected for graphing: " + numeSenzor);
			}
		}
		private void btnBackExtract_Click_1(object sender, EventArgs e)
		{
			pnlExtract.Visible = false;
			pnlContinutPrincipal.Visible = true;
			pnlContinutPrincipal.BringToFront();
		}

		private async void btnSend1_Click_Click(object sender, EventArgs e)
		{
			if (cmbComenzi.SelectedItem == null) return;

			string textSelectat = cmbComenzi.SelectedItem.ToString();
			string codComanda = textSelectat.Substring(0, 4);

			txtConsole.AppendText("> SENT: " + codComanda + "\r\n");

			CommandResult rezultat = await CommandProcessor.ExecuteAsync(codComanda, scanner);

			txtConsole.AppendText("[RAW HEX] " + rezultat.RawHex + "\r\n");

			txtConsole.AppendText(rezultat.ConsoleMessage + "\r\n");
			txtConsole.AppendText("--------------------------------\r\n");

			txtConsole.ScrollToCaret();

			string numeSenzor = (!string.IsNullOrEmpty(textSelectat) && textSelectat.Length > 7)
								? textSelectat.Substring(7)
								: textSelectat;

			AdaugaInIstoric("EXTRACT", $"{numeSenzor} : {rezultat.ConsoleMessage}");
		}

		private void btnStartGraph_Click_1(object sender, EventArgs e)
		{
			if (timerGraph.Enabled)
			{
				timerGraph.Stop();
				btnStartGraph.Text = "START GRAPH";
				btnStartGraph.BackColor = System.Drawing.Color.FromArgb(45, 45, 50);

				AdaugaInIstoric("MONITORING", "Stopped graph data acquisition.");

				string caleSalvata = csvLogger.StopLog();
				if (!string.IsNullOrEmpty(caleSalvata))
				{
					MessageBox.Show("The monitoring session has been successfully saved in:\n" + caleSalvata, "CSV Export", MessageBoxButtons.OK, MessageBoxIcon.Information);
				}
			}
			else
			{
				chartSenzor.Series[0].Points.Clear();
				chartSenzor.ChartAreas[0].AxisY.Maximum = Double.NaN;
				if (chkLogData.Checked)
				{
					try
					{
						string vinCurent = lblVINValue != null ? lblVINValue.Text : "Unknown VIN";
						csvLogger.StartNewLog(vinCurent);
					}
					catch (Exception ex)
					{
						MessageBox.Show("Error creating the CSV file: " + ex.Message, "I/O Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
						return;
					}
				}

				timerGraph.Start();
				btnStartGraph.Text = "STOP GRAPH";
				btnStartGraph.BackColor = Color.DarkRed;

				string senzorSelectat = cmbGraphSensor.SelectedItem != null ? cmbGraphSensor.SelectedItem.ToString() : "Unknown";
				string numeSenzor = senzorSelectat.Length > 7 ? senzorSelectat.Substring(7) : senzorSelectat;
				AdaugaInIstoric("MONITORING", "Started graph for: " + numeSenzor);
			}
		}

		private async void timerGraph_Tick_1(object sender, EventArgs e)
		{
			timerGraph.Stop();
			if (cmbGraphSensor.SelectedItem == null) return;

			string textSelectat = cmbGraphSensor.SelectedItem.ToString();
			string codComanda = textSelectat.Substring(0, 4);

			string raspunsHex = await scanner.TrimiteComandaAsync(codComanda);
			string valoareReala = scanner.DecodeazaRaspuns(codComanda, raspunsHex);

			double? punctGrafic = GraphDataParser.Parse(valoareReala);

			if (punctGrafic.HasValue)
			{
				chartSenzor.Series[0].Points.AddY(punctGrafic.Value);

				if (chartSenzor.Series[0].Points.Count > 50)
				{
					chartSenzor.Series[0].Points.RemoveAt(0);
				}

				if (chkLogData.Checked)
				{
					bool scriereReusita = csvLogger.LogData(textSelectat, punctGrafic.Value);

					if (!scriereReusita)
					{
						btnStartGraph.Text = "START GRAPH";
						btnStartGraph.BackColor = System.Drawing.Color.FromArgb(45, 45, 50);

						AdaugaInIstoric("MONITORING", "Graph stopped automatically due to CSV write error.");
						csvLogger.StopLog();

						MessageBox.Show("Critical Error writing to CSV file!\n\nThe file is likely open in another program or you lack write permissions. The monitoring session has been stopped to prevent data loss.", "I/O Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

						return;
					}
				}
				if (isConnected)
				{
					_ = SupabaseService.InsertLogAsync("LIVE DATA", $"{textSelectat} : {punctGrafic.Value}", lblVINValue.Text);
				}
			}

			timerGraph.Start();
		}

		private void btnClear_Click(object sender, EventArgs e)
		{
			txtConsole.Clear();
		}

		private void btnSaveConsole_Click(object sender, EventArgs e)
		{
			if (string.IsNullOrWhiteSpace(txtConsole.Text))
			{
				MessageBox.Show("The console is empty. Nothing to save.", "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return;
			}

			try
			{
				string caleFisier = FileManager.ExportConsole(txtConsole.Lines);
				MessageBox.Show("The file was successfully saved to Desktop:\n" + caleFisier, "Export Completed", MessageBoxButtons.OK, MessageBoxIcon.Information);
			}
			catch (Exception ex)
			{
				MessageBox.Show("Error writing the file to disk:\n" + ex.Message, "I/O Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		private async void btnReadErrors_Click(object sender, EventArgs e)
		{
			if (!isConnected)
			{
				MessageBox.Show("Error: Establish connection with the ECU before reading errors.", "Port Closed", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			lstErrors.Items.Clear();
			lstErrors.Items.Add("Querying ECU for error codes...");

			DiagnosticsResult rezultat = await DiagnosticsService.ReadErrorsAsync(scanner);

			lstErrors.Items.Clear();

			if (rezultat.ErrorCodes.Count == 0)
			{
				lstErrors.Items.Add("No error codes found (No stored errors).");
				lstErrors.BackColor = Color.OliveDrab;
				AdaugaInIstoric("DIAGNOSIS", "Read errors: No error found.");
			}
			else
			{
				lstErrors.BackColor = Color.DarkRed;
				foreach (string cod in rezultat.ErrorCodes)
				{
					lstErrors.Items.Add("Error detected: " + cod);
				}
				AdaugaInIstoric("DIAGNOSIS", $"Read errors: {rezultat.ErrorCodes.Count} error codes detected.");
				MessageBox.Show($"{rezultat.ErrorCodes.Count} error codes stored in ECU detected!", "Diagnosis Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
			}
		}

		private async void btnClearErrors_Click(object sender, EventArgs e)
		{
			if (!isConnected)
			{
				MessageBox.Show("Error: Establish connection with the ECU before clearing errors.", "Port Closed", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			DialogResult dialog = MessageBox.Show("Warning: Clearing errors will turn off the Check Engine light and reset emissions data.\n\nOBD Rule: Engine must be OFF, and ignition ON.\n\nAre you sure you want to continue?", "Confirm DTC Clear", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

			if (dialog == DialogResult.Yes)
			{
				lstErrors.Items.Clear();
				lstErrors.BackColor = Color.FromArgb(30, 30, 36);
				lstErrors.Items.Add("Sending clear command (Service 04)...");

				DiagnosticsResult rezultat = await DiagnosticsService.ClearErrorsAsync(scanner);

				if (rezultat.Success)
				{
					lstErrors.Items.Clear();
					lstErrors.Items.Add("Clear command executed successfully!");
					lstErrors.BackColor = Color.OliveDrab;
					AdaugaInIstoric("DIAGNOSIS", "Clear errors executed successfully.");
					MessageBox.Show("Error memory has been cleared.\nPlease read errors again to confirm.", "Clear Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);
				}
				else
				{
					lstErrors.Items.Add("Clear error. Response received: " + rezultat.RawResponse);
					lstErrors.BackColor = Color.DarkRed;
					AdaugaInIstoric("DIAGNOSIS", "Clear errors failed. Response: " + rezultat.RawResponse);
					MessageBox.Show("ECU rejected the clear command or a communication problem occurred.", "ECU Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
			}
		}
		private async void btnDeconectare_Click(object sender, EventArgs e)
		{
			if (isConnected)
			{
				await CommandProcessor.ExecuteAsync("ATPC", scanner);
				await CommandProcessor.ExecuteAsync("ATZ", scanner);
			}

			isConnected = false;
			scanner.Deconectare();

			btnConectareNou.Text = "CONNECT";
			btnConectareNou.BackColor = Color.FromArgb(45, 45, 50);

			lblEcuNameValue1.Text = "-";
			lblVINValue.Text = "-";
			lblCalIDValue.Text = "-";
			lblCVNValue.Text = "-";

			if (lstErrors != null)
			{
				lstErrors.Items.Clear();
				lstErrors.BackColor = Color.FromArgb(30, 30, 36);
			}
			
			pnlContinutPrincipal.Controls.Clear();
			ConstruiesteMainPanel();

			AdaugaInIstoric("SYSTEM", "Disconnected from port.");
			MessageBox.Show("Port has been successfully closed and released.", "Disconnect", MessageBoxButtons.OK, MessageBoxIcon.Information);
		}

		private async void btnReadFreezeFrame_Click(object sender, EventArgs e)
		{
			lstFreezeFrame.Items.Clear();
			lstFreezeFrame.Items.Add("Scanning maps and sensors... Please wait.");

			try
			{
				var rezultate = await FreezeFrameService.ScanAsync(scanner);
				lstFreezeFrame.Items.Clear();

				if (rezultate == null || rezultate.Count == 0)
				{
					lstFreezeFrame.Items.Add("No Freeze Frame (Error) data found in the ECU.");
					AdaugaInIstoric("FREEZE FRAME", "Scan performed: No freeze frame found.");
					return;
				}

				foreach (string rand in rezultate)
				{
					lstFreezeFrame.Items.Add(rand);
				}

				AdaugaInIstoric("FREEZE FRAME", $"Scan completed. {rezultate.Count} values extracted.");
			}
			catch (Exception ex)
			{
				lstFreezeFrame.Items.Clear();
				lstFreezeFrame.Items.Add("Scan Error: " + ex.Message);
				AdaugaInIstoric("FREEZE FRAME", "Scan error: " + ex.Message);
			}
		}

		private async void btnVoltajBaterie_Click(object sender, EventArgs e)
		{
			CommandResult rezultat = await CommandProcessor.ExecuteAsync("ATRV", scanner);

			if (!string.IsNullOrEmpty(rezultat.RawHex) && !rezultat.RawHex.Contains("ERROR") && !rezultat.RawHex.Contains("?"))
			{
				AdaugaInIstoric("SYSTEM", $"ATRV : Battery Voltage : {rezultat.RawHex}");

				MessageBox.Show("Battery voltage is: " + rezultat.RawHex, "Battery Status", MessageBoxButtons.OK, MessageBoxIcon.Information);
			}
			else
			{
				AdaugaInIstoric("SYSTEM", "ATRV : Error reading battery voltage.");
				MessageBox.Show("Could not read voltage. Are you connected to the vehicle?", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
			}
		}

		private void AdaugaInIstoric(string categorie, string detalii)
		{
			string ora = DateTime.Now.ToString("HH:mm:ss");
			string linie = $"[{ora}] [{categorie}] {detalii}";

			istoricSesiuneCurenta.Add(linie);

			string currentVIN = lblVINValue.Text.Trim();
			if (string.IsNullOrEmpty(currentVIN)) currentVIN = "-";

			string selectieCombo = cmbVehicule.SelectedItem?.ToString().Trim() ?? "";

			bool esteSesiuneLocala = selectieCombo == "--- Local Session ---";
			bool esteMasinaCurenta = selectieCombo == currentVIN;

			if (esteSesiuneLocala || esteMasinaCurenta)
			{
				lstIstoricGlobal.Items.Add(linie);
				lstIstoricGlobal.TopIndex = lstIstoricGlobal.Items.Count - 1;
			}
			if (isConnected && currentVIN.Length >= 15)
			{
				_ = SupabaseService.InsertLogAsync(categorie, detalii, currentVIN);
			}
		}

		private void btnSalveazaIstoric_Click(object sender, EventArgs e)
		{
			if (lstIstoricGlobal.Items.Count == 0)
			{
				MessageBox.Show("The history is empty. Nothing to save.", "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return;
			}

			try
			{
				var liniiIstoric = new System.Collections.Generic.List<string>();
				foreach (var item in lstIstoricGlobal.Items)
				{
					liniiIstoric.Add(item.ToString());
				}

				string currentVIN = (lblVINValue != null) ? lblVINValue.Text : "-";
				string caleFisier = FileManager.ExportHistory(liniiIstoric, currentVIN);
				MessageBox.Show("History has been successfully saved to Desktop:\n" + caleFisier, "Save Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);
			}
			catch (Exception ex)
			{
				MessageBox.Show("Error saving the file:\n" + ex.Message, "I/O Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		private async void btnTrimiteManual_Click(object sender, EventArgs e)
		{
			string comanda = txtComandaManuala.Text.Trim().ToUpper();

			if (comanda.Length < 2)
			{
				MessageBox.Show("Please enter a valid command of at least 2 characters (e.g., 010C, 020C, 0902).", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return;
			}

			txtConsole.AppendText("> SENT MANUAL: " + comanda + "\r\n");

			CommandResult rezultat = await CommandProcessor.ExecuteAsync(comanda, scanner);

			txtConsole.AppendText("[RAW HEX] " + rezultat.RawHex + "\r\n");

			if (!string.IsNullOrEmpty(rezultat.ConsoleMessage))
			{
				txtConsole.AppendText(rezultat.ConsoleMessage + "\r\n");
			}

			if (rezultat.HasHistory)
			{
				AdaugaInIstoric(rezultat.HistoryCategory, rezultat.HistoryDetail);
			}

			txtConsole.AppendText("--------------------------------\r\n");
			txtConsole.ScrollToCaret();
		}

		private void btnNavPerformance_Click(object sender, EventArgs e)
		{
			pnlContinutPrincipal.Visible = false;
			pnlPerformance.Visible = true;
			pnlPerformance.BringToFront();
			AdaugaInIstoric("NAVIGATION", "Entered Performance Test panel.");

		}

		private void btnBackPerformance_Click(object sender, EventArgs e)
		{
			isTestingPerformance = false;

			btnStartTestPerformanta.Text = "START TEST";
			btnStartTestPerformanta.BackColor = Color.FromArgb(45, 45, 50);

			pnlPerformance.Visible = false;
			pnlContinutPrincipal.Visible = true;
			pnlContinutPrincipal.BringToFront();
		}

		private async void btnStartTestPerformanta_Click(object sender, EventArgs e)
		{
			if (isTestingPerformance)
			{
				isTestingPerformance = false;
				return;
			}

			if (!isConnected)
			{
				MessageBox.Show("Please connect to the ECU first!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			isTestingPerformance = true;
			btnStartTestPerformanta.Text = "STOP TEST";
			btnStartTestPerformanta.BackColor = Color.DarkRed;

			lblRezultat50.Text = "0 - 50 km/h: -- s";
			lblRezultat100.Text = "0 - 100 km/h: -- s";
			lblRezultat130.Text = "0 - 130 km/h: -- s";
			if (lblVitezaCurenta != null)
			{
				lblVitezaCurenta.ForeColor = Color.White;
			}

			ultimulTest = new PerformanceResult();
			ceasPerformanta.Reset();

			double lastSpeed = 0;
			double lastTime = 0;
			bool testInceput = false;

			AdaugaInIstoric("PERFORMANCE", "Waiting for vehicle to move.");

			while (isTestingPerformance)
			{
				try
				{
					string raspunsHex = await scanner.TrimiteComandaAsync("010D");
					string decodat = scanner.DecodeazaRaspuns("010D", raspunsHex);

					double? parsedSpeed = GraphDataParser.Parse(decodat);

					if (parsedSpeed.HasValue)
					{
						double currentSpeed = parsedSpeed.Value;
						lblVitezaCurenta.Text = $"{currentSpeed} km/h";

						if (!testInceput && currentSpeed > 0)
						{
							testInceput = true;
							ceasPerformanta.Start();
							AdaugaInIstoric("PERFORMANCE", "Vehicle moving. Timer started.");
						}

						if (testInceput)
						{
							double currentTime = ceasPerformanta.Elapsed.TotalSeconds;

							if (!ultimulTest.Completed50 && currentSpeed >= 50)
							{
								ultimulTest.Time50 = PerformanceService.InterpoleazaTimp(lastSpeed, currentSpeed, lastTime, currentTime, 50);
								ultimulTest.Completed50 = true;
								lblRezultat50.Text = $"0 - 50 km/h: {ultimulTest.Time50:F2} s";
							}

							if (!ultimulTest.Completed100 && currentSpeed >= 100)
							{
								ultimulTest.Time100 = PerformanceService.InterpoleazaTimp(lastSpeed, currentSpeed, lastTime, currentTime, 100);
								ultimulTest.Completed100 = true;
								lblRezultat100.Text = $"0 - 100 km/h: {ultimulTest.Time100:F2} s";
							}

							if (!ultimulTest.Completed130 && currentSpeed >= 130)
							{
								ultimulTest.Time130 = PerformanceService.InterpoleazaTimp(lastSpeed, currentSpeed, lastTime, currentTime, 130);
								ultimulTest.Completed130 = true;
								lblRezultat130.Text = $"0 - 130 km/h: {ultimulTest.Time130:F2} s";

								isTestingPerformance = false;
								break;
							}

							lastSpeed = currentSpeed;
							lastTime = currentTime;
						}
					}
					else
					{
						AdaugaInIstoric("PERFORMANCE", "Missed data frame from ECU. Skipping to next read.");
					}

					await Task.Delay(150);
				}
				catch (Exception ex)
				{
					isTestingPerformance = false;
					ceasPerformanta.Stop();

					btnStartTestPerformanta.Text = "START TEST";
					btnStartTestPerformanta.BackColor = Color.FromArgb(45, 45, 50);

					AdaugaInIstoric("PERFORMANCE", "Hardware error/Disconnect during test: " + ex.Message);
					MessageBox.Show("Connection lost during the performance test! The test has been aborted.\n\nDetails: " + ex.Message, "Hardware Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

					break;
				}
			}
			ceasPerformanta.Stop();
			btnStartTestPerformanta.Text = "START TEST";
			btnStartTestPerformanta.BackColor = Color.FromArgb(45, 45, 50);

			if (ultimulTest != null && ultimulTest.Completed130)
			{
				AdaugaInIstoric("PERFORMANCE", "Test completed successfully.");

				if (lblVitezaCurenta != null)
				{
					lblVitezaCurenta.ForeColor = Color.LimeGreen;
				}
				System.Media.SystemSounds.Beep.Play();
			}
			else
			{
				AdaugaInIstoric("PERFORMANCE", "Test stopped by user or interrupted.");
			}
		}

		private void btnSavePerformance_Click(object sender, EventArgs e)
		{
			if (!ultimulTest.Completed50 && !ultimulTest.Completed100)
			{
				MessageBox.Show("No test data available. Perform an acceleration test first!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return;
			}

			try
			{
				string model = lblVINValue != null ? lblVINValue.Text : "Unknown VIN";

				string cale = FileManager.ExportPerformance(ultimulTest, model);
				AdaugaInIstoric("PERFORMANCE", "Test results exported to Desktop.");
				MessageBox.Show($"Results successfully saved to Desktop:\n{cale}", "Save Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);
			}
			catch (Exception ex)
			{
				MessageBox.Show("Error saving data: " + ex.Message, "I/O Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}
		private async void cmbVehicule_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (cmbVehicule.SelectedItem == null) return;
			string selectie = cmbVehicule.SelectedItem.ToString();

			lstIstoricGlobal.Items.Clear();

			if (selectie == "--- Local Session ---")
			{
				foreach (var linie in istoricSesiuneCurenta)
				{
					lstIstoricGlobal.Items.Add(linie);
				}
				return;
			}

			lstIstoricGlobal.Items.Add($"Downloading history from the Cloud for: {selectie}...");
			var logs = await SupabaseService.GetHistoryByVinAsync(selectie);
			lstIstoricGlobal.Items.Clear();

			foreach (var log in logs)
			{
				string oraLocala = log.CreatedAt.ToString("dd/MM/yyyy HH:mm:ss");
				lstIstoricGlobal.Items.Add($"[{oraLocala}] [{log.Category}] {log.Details}");
			}
		}

		private async Task IncarcaListaVinuriAsync()
		{
			try
			{
				cmbVehicule.Items.Clear();
				cmbVehicule.Items.Add("--- Local Session ---");

				var vins = await SupabaseService.GetUniqueVinsAsync();

				foreach (var vin in vins)
				{
					cmbVehicule.Items.Add(vin);
				}

				if (cmbVehicule.Items.Count > 0)
				{
					cmbVehicule.SelectedIndex = 0;
				}
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine("Error fetching VINs: " + ex.Message);
			}
		}

		protected override void OnFormClosing(FormClosingEventArgs e)
		{
			isTestingPerformance = false;

			if (timerGraph != null && timerGraph.Enabled)
			{
				timerGraph.Stop();
			}
			if (csvLogger != null)
			{
				csvLogger.StopLog();
			}
			if (scanner != null && isConnected)
			{
				scanner.Deconectare();
			}
			base.OnFormClosing(e);
		}
	}
}
