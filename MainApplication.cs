using ActiveClient;
using System;
using System.Windows.Forms;

internal static class MainApplication
{
	[STAThread]
	private static void Main()
	{
		Application.EnableVisualStyles();
		Application.SetCompatibleTextRenderingDefault(false);
		//Class5.XCUF1frzK2Woy();
		Application.Run(new MainForm());
	}

	private static void smethod_0(Exception exception_0)
	{
		MessageBox.Show(exception_0.Message + "\r\n" + exception_0.StackTrace, "");
	}
}
