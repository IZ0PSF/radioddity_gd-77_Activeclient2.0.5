using System;
using UsbLibrary;

internal class USBDeviceWrapper : SpecifiedOutputReport
{
	public USBDeviceWrapper(HIDDevice hiddevice_0):base(hiddevice_0)
	{
	}

	public override void PackData(byte[] byte_0)
	{
		byte[] buffer = base.Buffer;
		buffer[0] = 1;
		buffer[1] = 0;
		buffer[2] = Convert.ToByte(byte_0.Length);
		buffer[3] = Convert.ToByte(byte_0.Length >> 8);
		Array.Copy(byte_0, 0, buffer, 4, Math.Min(byte_0.Length, base.Buffer.Length - 4));
	}

	public override void PackData(byte[] byte_0, int int_0, int int_1)
	{
		byte[] buffer = base.Buffer;
		buffer[0] = 1;
		buffer[1] = 0;
		buffer[2] = Convert.ToByte(int_1);
		buffer[3] = Convert.ToByte(int_1 >> 8);
		Array.Copy(byte_0, int_0, buffer, 4, Math.Min(int_1, base.Buffer.Length - 4));
	}
}
