using System;
using System.Drawing;
using System.Windows.Forms;

namespace LICENTA.bun
{
	public partial class Form1 : Form
	{
		private bool trageFereastra = false;
		private Point punctStart = new Point(0, 0);

		private void InitializeModernUI()
		{
			this.FormBorderStyle = FormBorderStyle.None;
			this.Size = new Size(1280, 672);
			this.StartPosition = FormStartPosition.CenterScreen;
			this.BackColor = Color.FromArgb(24, 24, 28);

			Panel pnlHeader = new Panel
			{
				Height = 60,
				Dock = DockStyle.Top,
				BackColor = Color.FromArgb(18, 18, 22)
			};

			pnlHeader.MouseDown += (s, e) => { trageFereastra = true; punctStart = new Point(e.X, e.Y); };
			pnlHeader.MouseMove += (s, e) =>
			{
				if (trageFereastra)
				{
					Point p = PointToScreen(e.Location);
					this.Location = new Point(p.X - punctStart.X, p.Y - punctStart.Y);
				}
			};
			pnlHeader.MouseUp += (s, e) => { trageFereastra = false; };

			Label lblAppTitle = new Label
			{
				Text = "CarTrace",
				Font = new Font("Segoe UI", 16, FontStyle.Bold),
				ForeColor = Color.Orange,
				AutoSize = true,
				Location = new Point(20, 15)
			};
			pnlHeader.Controls.Add(lblAppTitle);

			Button btnMaximize = new Button
			{
				Text = "◻", 
				Size = new Size(40, 40),
				Location = new Point(pnlHeader.Width - 80, 0), 
				Anchor = AnchorStyles.Top | AnchorStyles.Right,
				FlatStyle = FlatStyle.Flat,
				ForeColor = Color.White,
				BackColor = Color.Transparent,
				Font = new Font("Segoe UI", 14, FontStyle.Bold)
			};
			btnMaximize.FlatAppearance.BorderSize = 0;
			btnMaximize.Click += (s, e) =>
			{
				if (this.WindowState == FormWindowState.Normal)
				{
					this.WindowState = FormWindowState.Maximized;
				}
				else
				{
					this.WindowState = FormWindowState.Normal;
				}
			};
			btnMaximize.MouseEnter += (s, e) => btnMaximize.BackColor = Color.FromArgb(50, 50, 55); 
			btnMaximize.MouseLeave += (s, e) => btnMaximize.BackColor = Color.Transparent;
			pnlHeader.Controls.Add(btnMaximize);

			Button btnClose = new Button
			{
				Text = "X",
				Size = new Size(40, 40),
				Location = new Point(pnlHeader.Width - 40, 0),
				Anchor = AnchorStyles.Top | AnchorStyles.Right,
				FlatStyle = FlatStyle.Flat,
				ForeColor = Color.White,
				BackColor = Color.Transparent,
				Font = new Font("Segoe UI", 12, FontStyle.Bold)
			};
			btnClose.FlatAppearance.BorderSize = 0;
			btnClose.Click += (s, e) => this.Close();
			btnClose.MouseEnter += (s, e) => btnClose.BackColor = Color.Red;
			btnClose.MouseLeave += (s, e) => btnClose.BackColor = Color.Transparent;
			pnlHeader.Controls.Add(btnClose);

			this.Controls.Add(pnlHeader);

			Panel pnlSidebar = new Panel
			{
				Width = 300,
				Dock = DockStyle.Left,
				BackColor = Color.FromArgb(30, 30, 36)
			};

			Label lblSidebarTitle = new Label
			{
				Text = "SYSTEM ACTIONS",
				Font = new Font("Segoe UI", 10, FontStyle.Bold),
				ForeColor = Color.Gray,
				AutoSize = true,
				Location = new Point(20, 20)
			};
			pnlSidebar.Controls.Add(lblSidebarTitle);

			Label lblPort = new Label
			{
				Text = "Select Port:",
				ForeColor = Color.White,
				Location = new Point(20, 60),
				AutoSize = true,
				Font = new Font("Segoe UI", 10)
			};
			pnlSidebar.Controls.Add(lblPort);

			cmbPorturiNou = new ComboBox
			{
				Location = new Point(20, 85),
				Size = new Size(260, 25),
				DropDownStyle = ComboBoxStyle.DropDownList
			};
			pnlSidebar.Controls.Add(cmbPorturiNou);

			btnConectareNou = CreazaButonSidebar("CONNECT", 130, Color.FromArgb(45, 45, 50));
			btnConectareNou.Click += BtnConectareNou_Click;
			pnlSidebar.Controls.Add(btnConectareNou);

			Button btnVoltaj = CreazaButonSidebar("BATTERY VOLTAGE", 190, Color.FromArgb(45, 45, 50));
			btnVoltaj.Click += btnVoltajBaterie_Click;
			pnlSidebar.Controls.Add(btnVoltaj);

			Button btnHistory = CreazaButonSidebar("ORDER HISTORY", 250, Color.FromArgb(45, 45, 50));
			btnHistory.Click += btnNavIstoric_Click;
			pnlSidebar.Controls.Add(btnHistory);

			Button btnDisconnect = CreazaButonSidebar("DISCONNECT", 310, Color.DarkRed);
			btnDisconnect.Click += btnDeconectare_Click;
			pnlSidebar.Controls.Add(btnDisconnect);

			this.Controls.Add(pnlSidebar);

			pnlContinutPrincipal = new Panel
			{
				Dock = DockStyle.Fill,
				BackColor = Color.FromArgb(24, 24, 28)
			};
			this.Controls.Add(pnlContinutPrincipal);
		}

		private Button CreazaButonSidebar(string text, int y, Color baseColor)
		{
			Button btn = new Button
			{
				Text = text,
				Location = new Point(20, y),
				Size = new Size(260, 45),
				FlatStyle = FlatStyle.Flat,
				BackColor = baseColor,
				ForeColor = Color.White,
				Font = new Font("Segoe UI", 10, FontStyle.Bold),
				Cursor = Cursors.Hand
			};
			btn.FlatAppearance.BorderSize = 0;
			btn.MouseEnter += (s, e) => btn.BackColor = Color.Orange;
			btn.MouseLeave += (s, e) => btn.BackColor = baseColor;
			return btn;
		}

		private void ConstruiesteMainPanel()
		{
			Label lblCore = new Label
			{
				Text = "CORE FUNCTIONS",
				Font = new Font("Segoe UI", 14, FontStyle.Bold),
				ForeColor = Color.White,
				AutoSize = true,
				Location = new Point(50, 40)
			};
			pnlContinutPrincipal.Controls.Add(lblCore);

			int startX = 50;
			int startY = 90;
			int width = 280;
			int height = 180;
			int gapX = 40;
			int gapY = 40;

			Button btnExtract = CreazaButonMain("EXTRACT\nLive Sensor Data", startX, startY, width, height);
			btnExtract.Click += btnNavExtract_Click;
			pnlContinutPrincipal.Controls.Add(btnExtract);

			Button btnMonitor = CreazaButonMain("MONITORING\nGraph Analysis", startX + width + gapX, startY, width, height);
			btnMonitor.Click += btnNavMonitoring_Click;
			pnlContinutPrincipal.Controls.Add(btnMonitor);

			Button btnDiag = CreazaButonMain("ERROR DIAGNOSIS\nRead & Clear DTC", startX, startY + height + gapY, width, height);
			btnDiag.Click += btnNavDiagnosis_Click;
			pnlContinutPrincipal.Controls.Add(btnDiag);

			Button btnFreeze = CreazaButonMain("FREEZE FRAME\nCrash Data Scan", startX + width + gapX, startY + height + gapY, width, height);
			btnFreeze.Click += btnNavFreezeFrame_Click;
			pnlContinutPrincipal.Controls.Add(btnFreeze);

			GroupBox grpCarInfo = new GroupBox
			{
				Text = "Vehicle Information",
				ForeColor = Color.Orange,
				Font = new Font("Segoe UI", 12, FontStyle.Bold),
				Size = new Size(300, 400),
				Location = new Point(startX + (width * 2) + (gapX * 2), startY)
			};

			AdaugaLabelInfo(grpCarInfo, "ECU Name:", lblEcuNameValue1 != null ? lblEcuNameValue1.Text : "-", 40);
			AdaugaLabelInfo(grpCarInfo, "VIN:", lblVINValue != null ? lblVINValue.Text : "-", 120);
			AdaugaLabelInfo(grpCarInfo, "Calibration ID:", lblCalIDValue != null ? lblCalIDValue.Text : "-", 200);
			AdaugaLabelInfo(grpCarInfo, "CVN:", lblCVNValue != null ? lblCVNValue.Text : "-", 280);

			pnlContinutPrincipal.Controls.Add(grpCarInfo);

			int performanceY = startY + (height * 2) + (gapY * 2);
			int performanceW = (width * 2) + gapX;

			Button btnPerformance = CreazaButonMain("PERFORMANCE\nAcceleration Test", startX, performanceY, performanceW,60);
			btnPerformance.Click += btnNavPerformance_Click;

			btnPerformance.FlatAppearance.BorderColor = Color.Orange;
			pnlContinutPrincipal.Controls.Add(btnPerformance);
		}
		private Button CreazaButonMain(string text, int x, int y, int w, int h)
		{
			Button btn = new Button
			{
				Text = text,
				Location = new Point(x, y),
				Size = new Size(w, h),
				FlatStyle = FlatStyle.Flat,
				BackColor = Color.FromArgb(30, 30, 36),
				ForeColor = Color.White,
				Font = new Font("Segoe UI", 14, FontStyle.Regular),
				Cursor = Cursors.Hand
			};
			btn.FlatAppearance.BorderColor = Color.FromArgb(80, 80, 80);
			btn.FlatAppearance.BorderSize = 1;

			btn.MouseEnter += (s, e) =>
			{
				btn.BackColor = Color.FromArgb(40, 40, 48);
				btn.FlatAppearance.BorderColor = Color.Orange;
			};
			btn.MouseLeave += (s, e) =>
			{
				btn.BackColor = Color.FromArgb(30, 30, 36);
				btn.FlatAppearance.BorderColor = Color.FromArgb(80, 80, 80);
			};

			return btn;
		}

		private void AdaugaLabelInfo(GroupBox parent, string titlu, string valoare, int yPos)
		{
			Label lblTitlu = new Label
			{
				Text = titlu,
				ForeColor = Color.Gray,
				Font = new Font("Segoe UI", 10),
				Location = new Point(20, yPos),
				AutoSize = true
			};
			parent.Controls.Add(lblTitlu);

			Label lblValoare = new Label
			{
				Text = valoare,
				ForeColor = Color.White,
				Font = new Font("Segoe UI", 10, FontStyle.Bold),
				Location = new Point(20, yPos + 25),
				AutoSize = true,
				MaximumSize = new Size(260, 0) 
			};
			parent.Controls.Add(lblValoare);
		}

		private void AplicaTemaIntunecataPanouriVechi()
		{
			Color fundalPrincipal = Color.FromArgb(24, 24, 28);
			Color textAlb = Color.White;
			Color butonFundal = Color.FromArgb(45, 45, 50);

			Panel[] panouriVechi = { pnlExtract, pnlMonitoring, pnlDiagnosis, pnlFreezeFrame, pnlIstoric, pnlPerformance };

			foreach (Panel pnl in panouriVechi)
			{
				if (pnl != null)
				{
					pnl.BackColor = fundalPrincipal;									
					pnl.Location = new Point(300, 60);
					pnl.Size = new Size(this.ClientSize.Width - 300, this.ClientSize.Height - 60);
					pnl.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;

					StilizeazaControale(pnl.Controls, textAlb, butonFundal);
				}
			}
			
			if (label25 != null)
			{
				label25.Text = "SESSION DATA SOURCE:";
				label25.ForeColor = Color.DarkGray;
				label25.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
			}

			cmbVehicule.FlatStyle = FlatStyle.Flat;
			cmbVehicule.DropDownStyle = ComboBoxStyle.DropDownList;
			cmbVehicule.BackColor = Color.FromArgb(30, 30, 36);
			cmbVehicule.ForeColor = Color.White;
		}

		private void StilizeazaControale(Control.ControlCollection controls, Color textColor, Color btnColor)
		{
			foreach (Control c in controls)
			{
				if (c is Label || c is GroupBox || c is CheckBox)
				{
					if (c.Name != "lblRezultat50" && c.Name != "lblRezultat100" && c.Name != "lblRezultat130")
					{
						c.ForeColor = textColor; 
					}
				}
				else if (c is Button)
				{
					Button btn = (Button)c;
					btn.FlatStyle = FlatStyle.Flat;
					btn.FlatAppearance.BorderSize = 0;
					btn.BackColor = btnColor;
					btn.ForeColor = textColor;
					btn.Font = new Font("Segoe UI", 10, FontStyle.Bold);

					if (btn.Text == "BACK" || btn.Text == "CLEAR")
					{
						btn.Width = 100;
					}
					else if (btn.Text == "START GRAPH")
					{
						btn.Width = 140;
					}
				}
				else if (c is TextBox || c is ListBox || c is ComboBox)
				{
					c.BackColor = Color.FromArgb(30, 30, 36);
					c.ForeColor = textColor;
					c.Font = new Font("Segoe UI", 10);
				}

				if (c.HasChildren)
				{
					StilizeazaControale(c.Controls, textColor, btnColor);
				}
			}
		}
	}
}