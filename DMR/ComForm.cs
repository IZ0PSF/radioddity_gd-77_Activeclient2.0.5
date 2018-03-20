using ActiveClient;
using System;
using System.ComponentModel;
using System.Drawing;
using System.IO.Ports;
using System.Windows.Forms;

namespace DMR
{
	public class ComForm : Form
	{
	//	private IContainer components;

		private Label lblPort;

		private ComboBox cmbPort;

		private Button btnCancel;

		private Button btnOK;

		private Button btnRefresh;

		protected override void Dispose(bool disposing)
		{
            /*
			if (disposing && this.components != null)
			{
				this.components.Dispose();
			}
             * */
			base.Dispose(disposing);
		}

		private void InitializeComponent()
		{
			this.lblPort = new Label();
			this.cmbPort = new ComboBox();
			this.btnCancel = new Button();
			this.btnOK = new Button();
			this.btnRefresh = new Button();
			base.SuspendLayout();
			this.lblPort.AutoSize = true;
			this.lblPort.Location = new Point(74, 63);
			this.lblPort.Name = "lblPort";
			this.lblPort.Size = new Size(29, 12);
			this.lblPort.TabIndex = 0;
			this.lblPort.Text = "Port";
			this.cmbPort.DropDownStyle = ComboBoxStyle.DropDownList;
			this.cmbPort.FormattingEnabled = true;
			this.cmbPort.Location = new Point(116, 60);
			this.cmbPort.Name = "cmbPort";
			this.cmbPort.Size = new Size(121, 20);
			this.cmbPort.TabIndex = 1;
			this.btnCancel.Location = new Point(180, 121);
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.Size = new Size(75, 23);
			this.btnCancel.TabIndex = 3;
			this.btnCancel.Text = "Cancel";
			this.btnCancel.UseVisualStyleBackColor = true;
			this.btnCancel.Click += this.btnCancel_Click;
//			this.btnOK.DialogResult = DialogResult.OK;
			this.btnOK.Location = new Point(56, 121);
			this.btnOK.Name = "btnOK";
			this.btnOK.Size = new Size(75, 23);
			this.btnOK.TabIndex = 2;
			this.btnOK.Text = "Save";
			this.btnOK.UseVisualStyleBackColor = true;
			this.btnOK.Click += this.btnOK_Click;
			this.btnRefresh.Location = new Point(56, 164);
			this.btnRefresh.Name = "btnRefresh";
			this.btnRefresh.Size = new Size(75, 22);
			this.btnRefresh.TabIndex = 4;
			this.btnRefresh.Text = "Refresh";
			this.btnRefresh.UseVisualStyleBackColor = true;
			this.btnRefresh.Click += this.btnRefresh_Click;
			base.AutoScaleDimensions = new SizeF(6f, 12f);
//			base.AutoScaleMode = AutoScaleMode.Font;
			this.ClientSize = new Size(310, 214);
			this.Controls.Add(this.btnRefresh);
			this.Controls.Add(this.btnCancel);
			this.Controls.Add(this.btnOK);
			this.Controls.Add(this.cmbPort);
			this.Controls.Add(this.lblPort);
//			this.FormBorderStyle = FormBorderStyle.FixedDialog;
			this.Name = "ComForm";
			this.StartPosition = FormStartPosition.CenterScreen;
			this.Text = "Port Setting";
			this.Load += new EventHandler(this.ComForm_Load);
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		public ComForm()
		{
	//		Class5.XCUF1frzK2Woy();
	//		base._002Ector();
			this.InitializeComponent();
		}

		private void refresh()
		{
			this.cmbPort.Items.Clear();
			string[] portNames = SerialPort.GetPortNames();
			string[] array = portNames;
			foreach (string item in array)
			{
				this.cmbPort.Items.Add(item);
			}
		}

		private void ComForm_Load(object sender, EventArgs e)
		{
			this.refresh();
			this.cmbPort.SelectedItem = MainForm.CurCom;
		}

		private void btnOK_Click(object sender, EventArgs e)
		{
			try
			{
				MainForm.CurCom = this.cmbPort.SelectedItem.ToString();
				IniFileUtils.smethod_6("Setup", "Com", MainForm.CurCom);
				base.Close();
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}
		}

		private void btnCancel_Click(object sender, EventArgs e)
		{
			base.Close();
		}

		private void btnRefresh_Click(object sender, EventArgs e)
		{
			this.refresh();
		}
	}
}
