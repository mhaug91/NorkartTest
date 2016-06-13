using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Common.Exceptions;

namespace TestIOTHubApp
{
    class Program
    {

        static RegistryManager registryManager;
        static string connectionString = "HostName=Teest.azure-devices.net;SharedAccessKeyName=iothubowner;SharedAccessKey=ZvPc1OsXzK+J8HBaIov1CERAq+cWviAog3plbgQ1xBY=";
        static void Main(string[] args)
        {
            registryManager = RegistryManager.CreateFromConnectionString(connectionString);
            AddDeviceAsync().Wait();
            Console.ReadLine();
        }

        private static async Task AddDeviceAsync()
        {
            string deviceID = "TestDevice";
            Device device;
            try
            {
                device = await registryManager.AddDeviceAsync(new Device(deviceID));
            }
            catch(DeviceAlreadyExistsException)
            {
                device = await registryManager.GetDeviceAsync(deviceID);
            }
            Console.WriteLine("Generated device key: {0}", device.Authentication.SymmetricKey.PrimaryKey);
        }
    }
}
