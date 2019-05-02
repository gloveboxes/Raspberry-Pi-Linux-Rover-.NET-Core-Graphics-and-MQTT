using System.Device.I2c;
using System.Device.I2c.Drivers;
using System.Runtime.InteropServices;

namespace Emmellsoft.IoT.Rpi
{
	internal static class I2cDeviceFactory
	{
		public static I2cDevice Create(I2cConnectionSettings settings)
		{
			if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
			{
				return new Windows10I2cDevice(settings);
			}

			return new UnixI2cDevice(settings);
		}
	}
}
