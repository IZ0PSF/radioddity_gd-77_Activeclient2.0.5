using DMR;
using ReadWriteCsv;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using UsbLibrary;

namespace ActiveClient
{
	public class MainForm : Form
	{
		public enum CurStepE
		{
			None,
			Program,
			ProgramTwo,
			ACK,
			EncryptVersion,
			McuId,
			Active,
			TmpActive,
			Read,
			Write,
			ChangeBaseAddr,
			EndRead,
			EndWrite
		}

		public enum CurSubStepE
		{
			None,
			Active,
			TempActive,
			Read = 0x10,
			Write
		}

		public class IdInfo
		{
			[DisplayName("ID")]
			public uint Id
			{
				get;
				set;
			}

			[DisplayName("CallSign")]
			public string CallSign
			{
				get;
				set;
			}

			public byte[] GetBytes()
			{
				List<byte> list = new List<byte>();
				list.AddRange(BitConverter.GetBytes(MainForm.smethod_1(this.Id)));
				list.AddRange(Encoding.ASCII.GetBytes(this.CallSign.PadRight(8, '\0').Substring(0, 8)));
				return list.ToArray();
			}

			public IdInfo()
			{
				//Class5.XCUF1frzK2Woy();
				//base._002Ector();
			}
		}

		private const byte CMD_READ = 82;

		private const byte CMD_WRITE = 87;

		private const byte CMD_ACTIVE = 65;

		private const byte CMD_COMMAND = 67;

		private const byte CMD_CHANAGE_BASEADDR = 66;

		private const byte CMD_ENCRYPT_VER = 2;

		private const byte CMD_MCU_ID = 16;

		private const byte CMD_TMP_ACTIVE = 1;

		private const byte DATA_ACTIVE = 1;

		private const byte CMD_ACK = 65;

		private const byte CMD_NACK = 78;

		private const byte CMD_ACTIVE_LENGTH = 32;

		private const byte LEN_HID_HEAD = 4;

		private const byte LEN_DATA_HEAD = 4;

		public const int MAX_ID_COUNT = 10920;

		public const int ID_BASE_ADDR = 196608;

		public const int ID_BASE_END_ADDR = 327680;

		public const int ID_VERSION_ADDR = 196612;

		public const int SPACE_VERSION = 4;

		public const int ID_COUNT_ADDR = 196616;

		public const int ID_START_ADDR = 196620;

		public const int INDEX_ID = 0;

		public const int SPACE_ID = 4;

		public const int INDEX_CALL_SIGN = 4;

		public const int SPACE_CALL_SIGN = 8;

		public const int SPACE_ID_INFO = 12;

		public const string SZ_DATABASE_NOT_EXIST = "The database file does not exist";

		public const string SZ_NOT_ACK = "No response received";

		public const string SZ_DEVICE_NOT_FOUND = "Device not found";

		public const string SZ_SAVE_SUCCESS = "Save successfully";

		public const string SZ_SAVE_FAIL = "Save failed";

		public const string SZ_OPEN_SUCCESS = "Open successfully";

		public const string SZ_DATA_FORMAT_WRONG = "The data format is wrong";

		public const string SZ_ID_OVER = "ID over {0}";

		private const int EeromLen = 1048576;

		private const int MAX_COMM_LEN = 32;

        private static readonly byte[] FRAME_ENCRYPT_VER = new byte[4] { 67, 82, 65, 2 };
        private static readonly byte[] FRAME_MCU_ID = new byte[4] { 67, 82, 65, 16 };
        private static readonly byte[] FRAME_TMP_ACTIVE = new byte[5] { 67, 87, 65, 1, 1 };
        private static readonly byte[] FRAME_ACTIVE = new byte[4] { 67, 87, 65, 32 };
        private static readonly byte[] FRAME_PROGRAM = new byte[7] { 2, 80, 82, 79, 71, 82, 65 };
        private static readonly byte[] FRAME_PROGRAM2 = new byte[2] { 77, 2 };
        private static readonly byte[] FRAME_ENDR = Encoding.ASCII.GetBytes("ENDR");
        private static readonly byte[] FRAME_ENDW = Encoding.ASCII.GetBytes("ENDW");

		public List<IdInfo> lstIdInfo;

		private SQLiteConnection conn;

		private SQLiteCommand cmd;

		private SQLiteHelper sh;

		public byte[] Eerom;

		public List<byte> Buffer;

		private IContainer components;

		private UsbHidPort usbHidPort;

		private System.Windows.Forms.Timer tmrDataRx;

		private ListBox lstInfo;

		private ProgressBar prgAddr;

		private Button btnReadData;

		private Button btnSave;

		private Button btnWrite;

		private Button btnOpen;

		private OpenFileDialog ofdMain;

		private TextBox txtOffsetAddr;

		private Label lblOffsetAddr;

		private Label lblPos;

		private Button btnPort;

		private SerialPort sptMain;

		private DataGridView dgvId;

		private TextBox txtVersion;

		private Label lblVersion;

		private Button btnImportCsv;

		private OpenFileDialog ofdCsv;

		public CurStepE CurStep
		{
			get;
			set;
		}

		public CurSubStepE CurSubStep
		{
			get;
			set;
		}

		public static byte[] CurMcuId
		{
			get;
			set;
		}

		public int CurAddr
		{
			get;
			set;
		}

		public int PrevAddr
		{
			get;
			set;
		}

		public int EndAddr
		{
			get;
			set;
		}

		public int CurGroupIndex
		{
			get;
			set;
		}

		public int[] StartAddrGroup
		{
			get;
			set;
		}

		public int[] EndAddrGroup
		{
			get;
			set;
		}

		public int CurPos
		{
			get;
			set;
		}

		public int MaxPrgPos
		{
			get;
			set;
		}

		public static int CurCbr
		{
			get;
			set;
		}

		public static string CurCom
		{
			get;
			set;
		}

		public static uint smethod_0(uint bcd)
		{
			int num = 0;
			uint num2 = 0u;
			uint num3 = 0u;
			for (num = 0; num < 8; num++)
			{
				num2 = (bcd & 0xF);
				bcd >>= 4;
				num3 += num2 * (uint)Math.Pow(10.0, (double)num);
			}
			return num3;
		}

		public static uint smethod_1(uint dec)
		{
			int num = 0;
			uint num2 = 0u;
			uint num3 = 0u;
			for (num = 0; num < 8; num++)
			{
				num2 = dec % 10u;
				dec /= 10u;
				num3 += num2 * (uint)Math.Pow(16.0, (double)num);
			}
			return num3;
		}

		private void usbHidPort_OnDataSend(object sender, EventArgs e)
		{
			this.tmrDataRx.Enabled = true;
		}

        private void showNackMessage()
        {
            string errorMsg1 = "Could not connect to the GD-77\n";
            string errorMsg2 = "Please restart the GD-77 whilst holding down..\n";
            string errorMsg3 = "The Blue side button\n";
            string errorMsg4 = "The Green menu button\n";
            string errorMsg5 = "and the hash (#) button";
            MessageBox.Show(errorMsg1 + errorMsg2 + errorMsg3 + errorMsg4 + errorMsg5 , "Error");
            this.method_4("Error." + errorMsg1);
            this.method_4( errorMsg2);
            this.method_4(errorMsg3);
            this.method_4(errorMsg4);
            this.method_4(errorMsg5);
        }

		private void usbHidPort_OnDataRecieved(object sender, DataRecievedEventArgs e)
		{
			this.tmrDataRx.Enabled = false;
			byte[] data = e.data;
			byte[] array = this.method_2(data);
			if (this.CurStep == CurStepE.Program)
			{
				if (array.Length > 0 && array[0] == 65)
				{
					this.method_8();
				}
			}
			else if (this.CurStep == CurStepE.ProgramTwo)
			{
				if (array.Length == 16)
				{
					this.method_9();
				}
			}
			else if (this.CurStep == CurStepE.ACK)
			{
				if (array.Length > 0 && array[0] == 65)
				{
					if (this.CurSubStep == CurSubStepE.Active)
					{
						this.method_10();
					}
					else if (this.CurSubStep == CurSubStepE.TempActive)
					{
						this.method_12();
					}
					else if (this.CurSubStep == CurSubStepE.Read)
					{
						this.CurAddr = this.StartAddrGroup[0];
						this.method_14(this.CurAddr);
					}
					else if (this.CurSubStep == CurSubStepE.Write)
					{
						this.CurAddr = this.StartAddrGroup[0];
						this.method_15(this.CurAddr);
					}
				}
			}
			else if (this.CurStep == CurStepE.ChangeBaseAddr)
			{
				if (array.Length > 0 && array[0] == 65)
				{
					if (this.CurSubStep == CurSubStepE.Read)
					{
						this.method_14(this.CurAddr);
					}
					else if (this.CurSubStep == CurSubStepE.Write)
					{
						this.method_15(this.CurAddr);
					}
				}
			}
			else if (this.CurStep == CurStepE.EncryptVersion)
			{
				if (array.Length > 0)
				{
					if (array[0] == 78)
					{
                        this.method_4("The mode is wrong");
					}
					else
					{
						this.method_1(array);
						this.method_11();
					}
				}
			}
			else if (this.CurStep == CurStepE.McuId)
			{
				if (array.Length > 0)
				{
					if (array[0] == 78)
					{
                        showNackMessage();
					}
					else
					{
						byte[] array2 = this.method_1(array);
						Dictionary<string, object> dictionary = new Dictionary<string, object>();
						dictionary["@McuId"] = array2;
						DataTable dataTable = this.sh.Select("select * from ActiveTable where McuId = @McuId", dictionary);
						if (dataTable.Rows.Count > 0)
						{
							if (dataTable.Rows[0][1] != DBNull.Value)
							{
								byte[] byte_ = (byte[])dataTable.Rows[0][1];
								this.method_13(byte_);
								MainForm.CurMcuId = array2;
							}
							else
							{
								this.method_4("Id already exists");
								this.method_19();
							}
						}
						else
						{
							Dictionary<string, object> dictionary2 = new Dictionary<string, object>();
							dictionary2["McuId"] = array2;
							dictionary2["ActiveCode"] = null;
							this.sh.Insert("ActiveTable", dictionary2);
                            this.method_4("Get Id successful");
							this.method_19();
						}
					}
				}
			}
			else if (this.CurStep == CurStepE.Active)
			{
				if (array.Length > 0)
				{
					if (array[0] == 65)
					{
						this.method_20();
					}
					else if (array[0] == 78)
					{
                        this.method_4("Activation code error");
					}
				}
			}
			else if (this.CurStep == CurStepE.TmpActive)
			{
				if (array.Length > 0 && array[0] == 65)
				{
					this.method_20();
				}
			}
			else if (this.CurStep == CurStepE.Read)
			{
				if (array[0] == 78)
				{
                    showNackMessage();
				}
				else
				{
					int int_ = array[1] * 256 + array[2];
					byte[] byte_2 = this.method_1(array);
					this.LesHmDnig(byte_2, int_);
					this.method_14(this.CurAddr);
				}
			}
			else if (this.CurStep == CurStepE.EndRead)
			{
				if (array.Length > 0 && array[0] == 65)
				{
					if (this.CurSubStep == CurSubStepE.Read)
					{
						base.Invoke(new MethodInvoker(this.method_7));
                        this.method_4("Read data successfully");
					}
					this.CurSubStep = CurSubStepE.None;
					this.CurStep = CurStepE.None;
				}
			}
			else if (this.CurStep == CurStepE.Write)
			{
				if (array.Length > 0 && array[0] == 65)
				{
					this.method_5();
					this.method_15(this.CurAddr);
				}
			}
			else if (this.CurStep == CurStepE.EndWrite && array.Length > 0 && array[0] == 65)
			{
				if (this.CurSubStep == CurSubStepE.Active)
				{
					Dictionary<string, object> dictionary3 = new Dictionary<string, object>();
					dictionary3["McuId"] = MainForm.CurMcuId;
					this.sh.Execute("delete from ActiveTable where McuId = @McuId", dictionary3);
                    this.method_4("Activation successful");
				}
				else if (this.CurSubStep == CurSubStepE.TempActive)
				{
                    this.method_4("Temporary activation success");
				}
				else if (this.CurSubStep == CurSubStepE.Write)
				{
                    this.method_4("Write data successfully");
				}
				this.CurSubStep = CurSubStepE.None;
				this.CurStep = CurStepE.None;
			}
		}

		private void usbHidPort_OnDeviceArrived(object sender, EventArgs e)
		{
		}

		private void usbHidPort_OnDeviceRemoved(object sender, EventArgs e)
		{
		}

		private void usbHidPort_OnSpecifiedDeviceArrived(object sender, EventArgs e)
		{
		}

		private void usbHidPort_OnSpecifiedDeviceRemoved(object sender, EventArgs e)
		{
		}

		private int method_0(int int_0, int int_1)
		{
			int num = 0;
//			int num2 = 0;
			num = int_0 % 32;
			if (int_0 + 32 > int_1)
			{
				return int_1 - int_0;
			}
			return 32 - num;
		}

		private int neYtqVfun()
		{
			int num = 0;
			int num2 = 0;
			int num3 = 0;
			int num4 = 0;
			int num5 = 0;
			int num6 = 0;
			int num7 = 0;
			for (num = 0; num < this.StartAddrGroup.Length; num++)
			{
				num6 = this.StartAddrGroup[num];
				num7 = this.EndAddrGroup[num];
				for (num5 = num6; num5 < num7; num5 += num3)
				{
					num2 = num5 % 32;
					num3 = ((num5 + 32 <= num7) ? (32 - num2) : (num7 - num5));
					num4++;
				}
			}
			return num4;
		}

		private byte[] method_1(byte[] byte_0)
		{
			int num = byte_0[3];
			byte[] array = new byte[num];
			Array.Copy(byte_0, 4, array, 0, num);
			return array;
		}

		private byte[] method_2(byte[] byte_0)
		{
			int num = (byte_0[3] << 8) + byte_0[2];
			byte[] array = new byte[num];
			Array.Copy(byte_0, 4, array, 0, num);
			return array;
		}

		public MainForm()
		{
		//	Class5.XCUF1frzK2Woy();
			this.lstIdInfo = new List<IdInfo>();
			this.Eerom = new byte[1048576];
			this.Buffer = new List<byte>();
			//base._002Ector();
			this.InitializeComponent();
            this.Text = "GD-77 DMR ID database utility";
		}

		private void MainForm_Load(object sender, EventArgs e)
		{
			this.usbHidPort.CheckDevicePresent();
			SpecifiedDevice specifiedDevice = this.usbHidPort.SpecifiedDevice;
			MainForm.CurCom = Class1.smethod_4("Setup", "Com", "Com1");
			MainForm.CurCbr = Class1.smethod_2("Setup", "Baudrate", 9600);
			this.method_22();
			this.method_3();
		}

		private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
		{
			this.usbHidPort.OnDataRecieved -= this.usbHidPort_OnDataRecieved;
			Thread.Sleep(100);
			this.cmd.Dispose();
			this.conn.Close();
		}

		protected override void OnHandleCreated(EventArgs e)
		{
			base.OnHandleCreated(e);
			this.usbHidPort.RegisterHandle(base.Handle);
		}

		protected override void WndProc(ref Message m)
		{
			this.usbHidPort.ParseMessages(ref m);
			base.WndProc(ref m);
		}

		private void method_3()
		{
			if (File.Exists("Mcu.db"))
			{
				this.conn = new SQLiteConnection("data source = Mcu.db");
				this.cmd = new SQLiteCommand();
				this.cmd.Connection = this.conn;
				this.conn.Open();
				this.sh = new SQLiteHelper(this.cmd);
			}
			else
			{
				MessageBox.Show("The database file does not exist");
				Application.Exit();
			}
		}

		private void tmrDataRx_Tick(object sender, EventArgs e)
		{
			this.tmrDataRx.Enabled = false;
			this.CurStep = CurStepE.None;
			this.CurSubStep = CurSubStepE.None;
			MessageBox.Show("No response received");
		}

		private void method_4(string string_0)
		{
			if (this.lstInfo.InvokeRequired)
			{
				this.lstInfo.Invoke(new Action<string>(this.method_4), string_0);
			}
			else
			{
				this.lstInfo.Items.Add(string_0);
				this.lstInfo.SelectedIndex = this.lstInfo.Items.Count - 1;
			}
		}

		private void LesHmDnig(byte[] byte_0, int int_0)
		{
			try
			{
				if (base.InvokeRequired)
				{
					base.Invoke(new Action<byte[], int>(this.LesHmDnig), byte_0, int_0);
				}
				else
				{
					int destinationIndex = this.PrevAddr + int_0;
					Array.Copy(byte_0, 0, this.Eerom, destinationIndex, byte_0.Length);
					this.method_5();
				}
			}
			catch
			{
			}
		}

		private void method_5()
		{
			if (base.InvokeRequired)
			{
				base.Invoke(new Action(this.method_5), null);
			}
			else
			{
				this.prgAddr.Value = ++this.CurPos * 100 / this.MaxPrgPos;
				this.lblPos.Text = this.prgAddr.Value.ToString();
			}
		}

		private byte[] method_6()
		{
			List<byte> list = new List<byte>();
			list.AddRange(Encoding.ASCII.GetBytes("ID-V"));
			list.AddRange(Encoding.ASCII.GetBytes(this.txtVersion.Text.PadRight(4, '\0').Substring(0, 4)));
			list.AddRange(BitConverter.GetBytes(this.lstIdInfo.Count));
			foreach (IdInfo item in this.lstIdInfo.OrderBy(MainForm.smethod_2))
			{
				list.AddRange(item.GetBytes());
			}
			return list.ToArray();
		}

		private void method_7()
		{
			this.lstIdInfo.Clear();
			uint num = BitConverter.ToUInt32(this.Eerom, 196616);
			this.txtVersion.Text = Encoding.ASCII.GetString(this.Eerom, 196612, 4);
			for (int i = 0; i < num; i++)
			{
				byte[] array = new byte[12];
				System.Buffer.BlockCopy(this.Eerom, 196620 + i * 12, array, 0, array.Length);
				this.lstIdInfo.Add(new IdInfo
				{
					Id = MainForm.smethod_0(BitConverter.ToUInt32(array, 0)),
					CallSign = Encoding.ASCII.GetString(array, 4, 8)
				});
			}
			this.method_23();
		}

		private void sptMain_DataReceived(object sender, SerialDataReceivedEventArgs e)
		{
			if (this.sptMain.BytesToRead <= 0)
			{
				Console.WriteLine(this.sptMain.BytesToRead);
			}
			else
			{
				this.tmrDataRx.Enabled = false;
				byte[] array = new byte[this.sptMain.BytesToRead];
				this.sptMain.Read(array, 0, array.Length);
				this.Buffer.AddRange(array);
				byte[] array2 = this.Buffer.ToArray();
				if (this.CurStep == CurStepE.Program)
				{
					if (array2.Length > 0 && array2[0] == 65)
					{
						this.method_8();
					}
				}
				else if (this.CurStep == CurStepE.ProgramTwo)
				{
					if (array2.Length == 16)
					{
						this.method_9();
					}
				}
				else if (this.CurStep == CurStepE.ACK)
				{
					if (array2.Length > 0 && array2[0] == 65)
					{
						if (this.CurSubStep == CurSubStepE.Active)
						{
							this.method_10();
						}
						else if (this.CurSubStep == CurSubStepE.TempActive)
						{
							this.method_12();
						}
						else if (this.CurSubStep == CurSubStepE.Read)
						{
							this.CurAddr = this.StartAddrGroup[0];
							this.method_14(this.CurAddr);
						}
						else if (this.CurSubStep == CurSubStepE.Write)
						{
							this.CurAddr = this.StartAddrGroup[0];
							this.method_15(this.CurAddr);
						}
					}
				}
				else if (this.CurStep == CurStepE.ChangeBaseAddr)
				{
					if (array2.Length > 0 && array2[0] == 65)
					{
						if (this.CurSubStep == CurSubStepE.Read)
						{
							this.method_14(this.CurAddr);
						}
						else if (this.CurSubStep == CurSubStepE.Write)
						{
							this.method_15(this.CurAddr);
						}
					}
				}
				else if (this.CurStep == CurStepE.EncryptVersion)
				{
					if (array2.Length > 0)
					{
						if (array2[0] == 78)
						{
                            this.method_4("The mode is wrong");
						}
						else
						{
							this.method_1(array2);
							this.method_11();
						}
					}
				}
				else if (this.CurStep == CurStepE.McuId)
				{
					if (array2.Length > 0)
					{
						if (array2[0] == 78)
						{
                            showNackMessage();
						}
						else
						{
							byte[] array3 = this.method_1(array2);
							Dictionary<string, object> dictionary = new Dictionary<string, object>();
							dictionary["@McuId"] = array3;
							DataTable dataTable = this.sh.Select("select * from ActiveTable where McuId = @McuId", dictionary);
							if (dataTable.Rows.Count > 0)
							{
								if (dataTable.Rows[0][1] != DBNull.Value)
								{
									byte[] byte_ = (byte[])dataTable.Rows[0][1];
									this.method_13(byte_);
									MainForm.CurMcuId = array3;
								}
								else
								{
									this.method_4("Id 已经存在");
									this.method_19();
								}
							}
							else
							{
								Dictionary<string, object> dictionary2 = new Dictionary<string, object>();
								dictionary2["McuId"] = array3;
								dictionary2["ActiveCode"] = null;
								this.sh.Insert("ActiveTable", dictionary2);
                                this.method_4("Get Id successful");
								this.method_19();
							}
						}
					}
				}
				else if (this.CurStep == CurStepE.Active)
				{
					if (array2.Length > 0)
					{
						if (array2[0] == 65)
						{
							this.method_20();
						}
						else if (array2[0] == 78)
						{
                            this.method_4("Activation code error");
						}
					}
				}
				else if (this.CurStep == CurStepE.TmpActive)
				{
					if (array2.Length > 0 && array2[0] == 65)
					{
						this.method_20();
					}
				}
				else if (this.CurStep == CurStepE.Read)
				{
					if (array2[0] == 78)
					{
                        showNackMessage();
					}
					else if (array2.Length > 4 && array2.Length >= array2[3] + 4)
					{
						int int_ = array2[1] * 256 + array2[2];
						byte[] byte_2 = this.method_1(array2);
						this.LesHmDnig(byte_2, int_);
						this.method_14(this.CurAddr);
					}
				}
				else if (this.CurStep == CurStepE.EndRead)
				{
					if (array2.Length > 0 && array2[0] == 65)
					{
						if (this.CurSubStep == CurSubStepE.Read)
						{
                            this.method_4("Read data successfully");
						}
						this.CurSubStep = CurSubStepE.None;
						this.CurStep = CurStepE.None;
					}
				}
				else if (this.CurStep == CurStepE.Write)
				{
					if (array2.Length > 0 && array2[0] == 65)
					{
						this.method_5();
						this.method_15(this.CurAddr);
					}
				}
				else if (this.CurStep == CurStepE.EndWrite && array2.Length > 0 && array2[0] == 65)
				{
					if (this.CurSubStep == CurSubStepE.Active)
					{
						Dictionary<string, object> dictionary3 = new Dictionary<string, object>();
						dictionary3["McuId"] = MainForm.CurMcuId;
						this.sh.Execute("delete from ActiveTable where McuId = @McuId", dictionary3);
                        this.method_4("Activation successful");
					}
					else if (this.CurSubStep == CurSubStepE.TempActive)
					{
                        this.method_4("Temporary activation success");
					}
					else if (this.CurSubStep == CurSubStepE.Write)
					{
                        this.method_4("Write data successfully");
					}
					this.CurSubStep = CurSubStepE.None;
					this.CurStep = CurStepE.None;
				}
			}
		}

		private void fEcjRtaQO()
		{
			this.method_21(MainForm.FRAME_PROGRAM);
			this.CurStep = CurStepE.Program;
		}

		private void method_8()
		{
			this.method_21(MainForm.FRAME_PROGRAM2);
			this.CurStep = CurStepE.ProgramTwo;
		}

		private void method_9()
		{
			this.method_21(new byte[1]
			{
				65
			});
			this.CurStep = CurStepE.ACK;
		}

		private void method_10()
		{
			this.method_21(MainForm.FRAME_ENCRYPT_VER);
			this.CurStep = CurStepE.EncryptVersion;
		}

		private void method_11()
		{
			this.method_21(MainForm.FRAME_MCU_ID);
			this.CurStep = CurStepE.McuId;
		}

		private void method_12()
		{
			this.method_21(MainForm.FRAME_TMP_ACTIVE);
			this.CurStep = CurStepE.TmpActive;
		}

		private void method_13(byte[] byte_0)
		{
			byte[] array = new byte[4 + byte_0.Length];
			Array.Copy(MainForm.FRAME_ACTIVE, 0, array, 0, 4);
			Array.Copy(byte_0, 0, array, 4, array.Length - 4);
			this.method_21(array);
			this.CurStep = CurStepE.Active;
		}

		private void method_14(int int_0)
		{
			if (int_0 >= this.EndAddr)
			{
				if (this.CurGroupIndex < this.EndAddrGroup.Length - 1)
				{
					if (this.CurGroupIndex == 0)
					{
						int num = BitConverter.ToInt32(this.Eerom, 196616);
						this.StartAddrGroup[1] = 196620;
						this.EndAddrGroup[1] = 196620 + num * 12;
						this.MaxPrgPos = this.neYtqVfun();
					}
					this.CurGroupIndex++;
					this.CurAddr = this.StartAddrGroup[this.CurGroupIndex];
					this.EndAddr = this.EndAddrGroup[this.CurGroupIndex];
					this.method_14(this.CurAddr);
				}
				else
				{
					this.method_19();
				}
			}
			else if (this.PrevAddr >> 16 != int_0 >> 16)
			{
				this.method_18(int_0);
			}
			else
			{
				int num2 = this.method_0(int_0, this.EndAddr);
				this.method_16(int_0, num2);
				this.CurAddr += num2;
			}
		}

		private void method_15(int int_0)
		{
			if (int_0 >= this.EndAddr)
			{
				if (this.CurGroupIndex < this.EndAddrGroup.Length - 1)
				{
					this.CurGroupIndex++;
					this.CurAddr = this.StartAddrGroup[this.CurGroupIndex];
					this.EndAddr = this.EndAddrGroup[this.CurGroupIndex];
					this.method_15(this.CurAddr);
				}
				else
				{
					this.method_20();
				}
			}
			else if (this.PrevAddr >> 16 != int_0 >> 16)
			{
				this.method_18(int_0);
			}
			else
			{
				int num = this.method_0(int_0, this.EndAddr);
				this.method_17(int_0, num);
				this.CurAddr += num;
			}
		}

		private void method_16(int int_0, int int_1)
		{
			byte[] array = new byte[4]
			{
				82,
				0,
				0,
				0
			};
			array[1] = (byte)(int_0 >> 8);
			array[2] = (byte)int_0;
			array[3] = (byte)int_1;
			this.method_21(array);
			this.CurStep = CurStepE.Read;
		}

		private void method_17(int int_0, int int_1)
		{
			byte[] array = new byte[4 + int_1];
			array[0] = 87;
			array[1] = (byte)(int_0 >> 8);
			array[2] = (byte)int_0;
			array[3] = (byte)int_1;
			Array.Copy(this.Eerom, int_0, array, 4, int_1);
			this.method_21(array);
			this.CurStep = CurStepE.Write;
		}

		private void method_18(int int_0)
		{
			byte[] array = new byte[8]
			{
				67,
				87,
				66,
				4,
				0,
				0,
				0,
				0
			};
			int num = int_0 >> 16 << 16;
			array[4] = (byte)(num >> 24);
			array[5] = (byte)(num >> 16);
			array[6] = (byte)(num >> 8);
			array[7] = (byte)num;
			this.PrevAddr = num;
			this.method_21(array);
			this.CurStep = CurStepE.ChangeBaseAddr;
		}

		private void method_19()
		{
			this.method_21(MainForm.FRAME_ENDR);
			this.CurStep = CurStepE.EndRead;
		}

		private void method_20()
		{
			this.method_21(MainForm.FRAME_ENDW);
			this.CurStep = CurStepE.EndWrite;
		}

		private void method_21(byte[] byte_0)
		{
			SpecifiedDevice specifiedDevice = this.usbHidPort.SpecifiedDevice;
			if (specifiedDevice != null)
			{
				Class2 @class = new Class2(specifiedDevice);
				@class.PackData(byte_0);
				specifiedDevice.SendData(@class);
			}
			else
			{
				MessageBox.Show("Device not found");
			}
		}

		private void btnReadData_Click(object sender, EventArgs e)
		{
			this.StartAddrGroup = new int[2]
			{
				196608,
				0
			};
			this.EndAddrGroup = new int[2]
			{
				196620,
				0
			};
			this.CurGroupIndex = 0;
			this.PrevAddr = 0;
			this.CurAddr = this.StartAddrGroup[this.CurGroupIndex];
			this.EndAddr = this.EndAddrGroup[this.CurGroupIndex];
			this.CurPos = 0;
			this.MaxPrgPos = this.neYtqVfun();
			this.fEcjRtaQO();
			this.CurSubStep = CurSubStepE.Read;
		}

		private void btnWrite_Click(object sender, EventArgs e)
		{
			byte[] array = this.method_6();
			System.Buffer.BlockCopy(array, 0, this.Eerom, 196608, array.Length);
			this.StartAddrGroup = new int[1]
			{
				196608
			};
			this.EndAddrGroup = new int[1]
			{
				196608 + array.Length
			};
			this.CurGroupIndex = 0;
			this.PrevAddr = 0;
			this.CurAddr = this.StartAddrGroup[this.CurGroupIndex];
			this.EndAddr = this.EndAddrGroup[this.CurGroupIndex];
			this.CurPos = 0;
			this.MaxPrgPos = this.neYtqVfun();
			this.fEcjRtaQO();
			this.CurSubStep = CurSubStepE.Write;
		}

		private void btnSave_Click(object sender, EventArgs e)
		{
			try
			{
				File.WriteAllBytes(DateTime.Now.ToString("MMdd_HHmmss") + ".bin", this.Eerom);
				MessageBox.Show("Save successfully");
			}
			catch
			{
				MessageBox.Show("Save failed");
			}
		}

		private void btnOpen_Click(object sender, EventArgs e)
		{
			try
			{
				this.ofdMain.InitialDirectory = Application.StartupPath;
				DialogResult dialogResult = this.ofdMain.ShowDialog();
				if (dialogResult == DialogResult.OK && !string.IsNullOrEmpty(this.ofdMain.FileName))
				{
					byte[] array = File.ReadAllBytes(this.ofdMain.FileName);
					int destinationIndex = 0;
					int.TryParse(this.txtOffsetAddr.Text, NumberStyles.HexNumber, (IFormatProvider)null, out destinationIndex);
					Array.Copy(array, 0, this.Eerom, destinationIndex, array.Length);
					MessageBox.Show("Open successfully");
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}
		}

		private void btnPort_Click(object sender, EventArgs e)
		{
			ComForm comForm = new ComForm();
			comForm.ShowDialog();
		}

		private void btnImportCsv_Click(object sender, EventArgs e)
		{
			try
			{
				this.ofdCsv.InitialDirectory = Application.StartupPath;
				DialogResult dialogResult = this.ofdCsv.ShowDialog();
				if (dialogResult == DialogResult.OK && !string.IsNullOrEmpty(this.ofdCsv.FileName))
				{
                    bool overMaxNumber = false;
					using (CsvFileReader csvFileReader = new CsvFileReader(this.ofdCsv.FileName, Encoding.Default))
					{
						CsvRow csvRow = new CsvRow();
                        
						csvFileReader.ReadRow(csvRow);
						if (csvRow.Count == 8)
						{
							this.lstIdInfo.Clear();
							while (csvFileReader.ReadRow(csvRow))
							{
								this.lstIdInfo.Add(new IdInfo
								{
									Id = Convert.ToUInt32(((List<string>)csvRow)[0]),
									CallSign = ((List<string>)csvRow)[1]
								});
								if (this.lstIdInfo.Count < 10920)
								{
									continue;
								}
                                overMaxNumber = true;
                                MessageBox.Show(string.Format("Open successfully\n\nNote.\nCSV contains more than the maximum number IDs ({0})\nOnly the first {0} have been imported", 10920), "Warning");
								break;
							}
						}
						else
						{
							MessageBox.Show("The data format is wrong.\nPlease check the format of the CSV file","Error");
						}
					}
					this.method_23();
                    if (!overMaxNumber)
                    {
                        MessageBox.Show("Open successfully");
                    }
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}
		}

		private void method_22()
		{
			this.dgvId.ReadOnly = true;
		}

		private void method_23()
		{
			this.dgvId.DataSource = null;
			this.dgvId.DataSource = this.lstIdInfo.OrderBy(MainForm.smethod_3).ToList();
		}

		private void dgvId_RowPostPaint(object sender, DataGridViewRowPostPaintEventArgs e)
		{
			try
			{
				DataGridView dataGridView = sender as DataGridView;
				if (e.RowIndex >= dataGridView.FirstDisplayedScrollingRowIndex)
				{
					using (SolidBrush brush = new SolidBrush(dataGridView.RowHeadersDefaultCellStyle.ForeColor))
					{
						string s = (e.RowIndex + 1).ToString();
						e.Graphics.DrawString(s, e.InheritedRowStyle.Font, brush, (float)(e.RowBounds.Location.X + 15), (float)(e.RowBounds.Location.Y + 5));
					}
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && this.components != null)
			{
				this.components.Dispose();
			}
			base.Dispose(disposing);
		}

		private void InitializeComponent()
		{
			this.components = new Container();
			this.usbHidPort = new UsbHidPort(this.components);
			this.tmrDataRx = new System.Windows.Forms.Timer(this.components);
			this.lstInfo = new ListBox();
			this.prgAddr = new ProgressBar();
			this.btnReadData = new Button();
			this.btnSave = new Button();
			this.btnWrite = new Button();
			this.btnOpen = new Button();
			this.ofdMain = new OpenFileDialog();
			this.txtOffsetAddr = new TextBox();
			this.lblOffsetAddr = new Label();
			this.lblPos = new Label();
			this.btnPort = new Button();
			this.sptMain = new SerialPort(this.components);
			this.dgvId = new DataGridView();
			this.txtVersion = new TextBox();
			this.lblVersion = new Label();
			this.btnImportCsv = new Button();
			this.ofdCsv = new OpenFileDialog();
			((ISupportInitialize)this.dgvId).BeginInit();
			base.SuspendLayout();
			this.usbHidPort.ProductId = 115;
			this.usbHidPort.VendorId = 5538;
			this.usbHidPort.OnSpecifiedDeviceRemoved += this.usbHidPort_OnSpecifiedDeviceRemoved;
			this.usbHidPort.OnDeviceArrived += this.usbHidPort_OnDeviceArrived;
			this.usbHidPort.OnDeviceRemoved += this.usbHidPort_OnDeviceRemoved;
			this.usbHidPort.OnDataRecieved += this.usbHidPort_OnDataRecieved;
			this.usbHidPort.OnSpecifiedDeviceArrived += this.usbHidPort_OnSpecifiedDeviceArrived;
			this.usbHidPort.OnDataSend += this.usbHidPort_OnDataSend;
			this.tmrDataRx.Interval = 3000;
			this.tmrDataRx.Tick += this.tmrDataRx_Tick;
			this.lstInfo.FormattingEnabled = true;
			this.lstInfo.ItemHeight = 12;
			this.lstInfo.Location = new Point(25, 15);
			this.lstInfo.Name = "lstInfo";
			this.lstInfo.Size = new Size(407, 232);
			this.lstInfo.TabIndex = 2;
			this.prgAddr.Location = new Point(108, 259);
			this.prgAddr.Name = "prgAddr";
			this.prgAddr.Size = new Size(316, 23);
			this.prgAddr.TabIndex = 3;
			this.btnReadData.Location = new Point(42, 351);
			this.btnReadData.Name = "btnReadData";
			this.btnReadData.Size = new Size(75, 23);
			this.btnReadData.TabIndex = 4;
			this.btnReadData.Text = "Read";
			this.btnReadData.UseVisualStyleBackColor = true;
			this.btnReadData.Click += this.btnReadData_Click;
			this.btnSave.Location = new Point(334, 391);
			this.btnSave.Name = "btnSave";
			this.btnSave.Size = new Size(75, 23);
			this.btnSave.TabIndex = 7;
			this.btnSave.Text = "Save";
			this.btnSave.UseVisualStyleBackColor = true;
			this.btnSave.Visible = false;
			this.btnSave.Click += this.btnSave_Click;
			this.btnWrite.Location = new Point(140, 351);
			this.btnWrite.Name = "btnWrite";
			this.btnWrite.Size = new Size(75, 23);
			this.btnWrite.TabIndex = 8;
			this.btnWrite.Text = "Write";
			this.btnWrite.UseVisualStyleBackColor = true;
			this.btnWrite.Click += this.btnWrite_Click;
			this.btnOpen.Location = new Point(244, 391);
			this.btnOpen.Name = "btnOpen";
			this.btnOpen.Size = new Size(75, 23);
			this.btnOpen.TabIndex = 9;
			this.btnOpen.Text = "Open";
			this.btnOpen.UseVisualStyleBackColor = true;
			this.btnOpen.Visible = false;
			this.btnOpen.Click += this.btnOpen_Click;
            this.ofdMain.Filter = "data file (*.bin)|*.bin";
			this.txtOffsetAddr.Location = new Point(108, 393);
			this.txtOffsetAddr.Name = "txtOffsetAddr";
			this.txtOffsetAddr.Size = new Size(107, 21);
			this.txtOffsetAddr.TabIndex = 11;
			this.txtOffsetAddr.Visible = false;
			this.lblOffsetAddr.AutoSize = true;
			this.lblOffsetAddr.Location = new Point(9, 396);
			this.lblOffsetAddr.Name = "lblOffsetAddr";
			this.lblOffsetAddr.Size = new Size(89, 12);
			this.lblOffsetAddr.TabIndex = 12;
			this.lblOffsetAddr.Text = "Offset Address";
			this.lblOffsetAddr.Visible = false;
			this.lblPos.AutoSize = true;
			this.lblPos.Location = new Point(53, 264);
			this.lblPos.Name = "lblPos";
			this.lblPos.Size = new Size(11, 12);
			this.lblPos.TabIndex = 10;
			this.lblPos.Text = "0";
			this.btnPort.Location = new Point(244, 432);
			this.btnPort.Name = "btnPort";
			this.btnPort.Size = new Size(75, 23);
			this.btnPort.TabIndex = 13;
			this.btnPort.Text = "Port";
			this.btnPort.UseVisualStyleBackColor = true;
			this.btnPort.Visible = false;
			this.btnPort.Click += this.btnPort_Click;
			this.sptMain.DataReceived += this.sptMain_DataReceived;
			this.dgvId.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.dgvId.Location = new Point(468, 15);
			this.dgvId.Name = "dgvId";
			this.dgvId.RowHeadersWidth = 60;
			this.dgvId.RowTemplate.Height = 23;
			this.dgvId.Size = new Size(497, 481);
			this.dgvId.TabIndex = 14;
			this.dgvId.RowPostPaint += this.dgvId_RowPostPaint;
			this.txtVersion.Location = new Point(108, 306);
			this.txtVersion.MaxLength = 4;
			this.txtVersion.Name = "txtVersion";
			this.txtVersion.Size = new Size(100, 21);
			this.txtVersion.TabIndex = 15;
			this.txtVersion.Text = "001";
			this.lblVersion.AutoSize = true;
			this.lblVersion.Location = new Point(51, 309);
			this.lblVersion.Name = "lblVersion";
			this.lblVersion.Size = new Size(47, 12);
			this.lblVersion.TabIndex = 16;
			this.lblVersion.Text = "Version";
			this.btnImportCsv.Location = new Point(244, 351);
			this.btnImportCsv.Name = "btnImportCsv";
			this.btnImportCsv.Size = new Size(75, 23);
			this.btnImportCsv.TabIndex = 17;
			this.btnImportCsv.Text = "Import csv";
			this.btnImportCsv.UseVisualStyleBackColor = true;
			this.btnImportCsv.Click += this.btnImportCsv_Click;
            this.ofdCsv.Filter = "data file (*.csv)|*.csv";
			base.AutoScaleDimensions = new SizeF(6f, 12f);
			base.AutoScaleMode = AutoScaleMode.Font;
			base.ClientSize = new Size(994, 519);
			base.Controls.Add(this.btnImportCsv);
			base.Controls.Add(this.lblVersion);
			base.Controls.Add(this.txtVersion);
			base.Controls.Add(this.dgvId);
			base.Controls.Add(this.btnPort);
			base.Controls.Add(this.lblOffsetAddr);
			base.Controls.Add(this.txtOffsetAddr);
			base.Controls.Add(this.lblPos);
			base.Controls.Add(this.btnOpen);
			base.Controls.Add(this.btnWrite);
			base.Controls.Add(this.btnSave);
			base.Controls.Add(this.btnReadData);
			base.Controls.Add(this.prgAddr);
			base.Controls.Add(this.lstInfo);
			base.Name = "MainForm";
			this.Text = "ID Library";
			base.Load += this.MainForm_Load;
			base.FormClosing += this.MainForm_FormClosing;
			((ISupportInitialize)this.dgvId).EndInit();
			base.ResumeLayout(false);
			base.PerformLayout();
		}

		[CompilerGenerated]
		private static uint smethod_2(IdInfo idInfo_0)
		{
			return idInfo_0.Id;
		}

		[CompilerGenerated]
		private static uint smethod_3(IdInfo idInfo_0)
		{
			return idInfo_0.Id;
		}

		static MainForm()
		{
			//Class5.XCUF1frzK2Woy();
            /*
			MainForm.FRAME_ENCRYPT_VER = new byte[4]
			{
				67,
				82,
				65,
				2
			};
			MainForm.FRAME_MCU_ID = new byte[4]
			{
				67,
				82,
				65,
				16
			};
			MainForm.FRAME_TMP_ACTIVE = new byte[5]
			{
				67,
				87,
				65,
				1,
				1
			};
			MainForm.FRAME_ACTIVE = new byte[4]
			{
				67,
				87,
				65,
				32
			};
			MainForm.FRAME_PROGRAM = new byte[7]
			{
				2,
				80,
				82,
				79,
				71,
				82,
				65
			};
			MainForm.FRAME_PROGRAM2 = new byte[2]
			{
				77,
				2
			};
			MainForm.FRAME_ENDR = Encoding.ASCII.GetBytes("ENDR");
			MainForm.FRAME_ENDW = Encoding.ASCII.GetBytes("ENDW");
             * */
		}
	}
}
