using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using System.Text;
using Microsoft.Azure.Devices.Client;
using System.Threading.Tasks;
using Newtonsoft.Json;
using GrovePi.Sensors;
using GrovePi;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace App10
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {

     
        IDHTTemperatureAndHumiditySensor sensor = DeviceFactory.Build.DHTTemperatureAndHumiditySensor(Pin.DigitalPin4, DHTModel.Dht11);

        string sensorTemp = "";
        string sensorHum = "";
        string sensTemp = "";
        string sensHum = "";


        static DeviceClient deviceClient;
        static string iotHubUri = "Teest.azure-devices.net";
        static string deviceKey = "U2jMw+4VOZiqZWrMwRqRgr98LhNpZUkbeMnx+GsEkEE=";

        private async void SendDeviceToCloudMessagesAsync()
        {

            while (true)
            {
                try
                {
                    // Check the value of the Sensor.
                    // Temperature in Celsius is returned as a double type.  Convert it to string so we can print it.
                    sensor.Measure();
                    sensorTemp = sensor.TemperatureInCelsius.ToString();
                    // Same for Humidity.  
                    sensorHum = sensor.Humidity.ToString();

                    // Print all of the values to the debug window.  
                    System.Diagnostics.Debug.WriteLine("Temp is " + sensorTemp + " C.  And the Humidity is " + sensorHum + "%. ");


                }
                catch (Exception ex)
                {
                    // NOTE: There are frequent exceptions of the following:
                    // WinRT information: Unexpected number of bytes was transferred. Expected: '. Actual: '.
                    // This appears to be caused by the rapid frequency of writes to the GPIO
                    // These are being swallowed here/

                    // If you want to see the exceptions uncomment the following:
                    // System.Diagnostics.Debug.WriteLine(ex.ToString());
                    
                }

                var telemetryDataPoint = new
                {
                    deviceId = "testnorkart",
                    sensTemp = sensorTemp,
                    sensHum = sensorHum

                };
                var messageString = JsonConvert.SerializeObject(telemetryDataPoint);
                var message = new Message(Encoding.ASCII.GetBytes(messageString));

                await deviceClient.SendEventAsync(message);
            

                Task.Delay(10000).Wait();
            }
        }

        
        public MainPage()
        {
            this.InitializeComponent();
            deviceClient = DeviceClient.Create(iotHubUri, new DeviceAuthenticationWithRegistrySymmetricKey("testnorkart", deviceKey));

            SendDeviceToCloudMessagesAsync();
            
        }
    }

}

