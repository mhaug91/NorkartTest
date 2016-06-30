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
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Windows.Storage;
using System.Runtime.Serialization;
using System.Net.Http;
using System.Net;
using GeoCoordinatePortable;
using Windows.Devices.Geolocation;



// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace App10
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {



        IDHTTemperatureAndHumiditySensor sensor = DeviceFactory.Build.DHTTemperatureAndHumiditySensor(Pin.DigitalPin4, DHTModel.Dht11);

        double sensorTemp = 0.0;
        double sensorHum = 0.0;
        

        HttpClient _httpClient = new HttpClient();

        static DeviceClient deviceClient;
        static string iotHubUri = "norkartiothub.azure-devices.net";
        static string deviceKey = "vQ6yFQMCW3mSx50SXmPX/PI9QHQOGCKqZip15QFo94E=";

        

        private async void SendDeviceToCloudMessagesAsync()
        {
            var accessStatus = await Geolocator.RequestAccessAsync();
            double lat = 0;
            double lon = 0;
            switch (accessStatus)
            {
                case GeolocationAccessStatus.Allowed:


                    // If DesiredAccuracy or DesiredAccuracyInMeters are not set (or value is 0), DesiredAccuracy.Default is used.
                    Geolocator geolocator = new Geolocator { DesiredAccuracyInMeters = 0 };                    

                    // Carry out the operation.
                    Geoposition pos = await geolocator.GetGeopositionAsync();
                    System.Diagnostics.Debug.WriteLine(pos);
                    System.Diagnostics.Debug.WriteLine(pos.Coordinate.Latitude);
                    System.Diagnostics.Debug.WriteLine(pos.Coordinate.Longitude);
                    System.Diagnostics.Debug.WriteLine(pos.ToString());
                    lat = pos.Coordinate.Latitude;
                    lon = pos.Coordinate.Longitude;

                    break;

                case GeolocationAccessStatus.Denied:
                   
                    break;

                case GeolocationAccessStatus.Unspecified:
                   
                    break;
            }

            while (true)
            {

                try
                {

                    // Check the value of the Sensor.
                    // Temperature in Celsius is returned as a double type.  Convert it to string so we can print it.
                    sensor.Measure();
                    sensorTemp = sensor.TemperatureInCelsius;
                    // Same for Humidity.  
                    sensorHum = sensor.Humidity;

                    // Print all of the values to the debug window.  
                    System.Diagnostics.Debug.WriteLine("Temp is " + sensorTemp + " C.  And the Humidity is " + sensorHum + "%. ");


                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("ERROR: " + ex);
                    // NOTE: There are frequent exceptions of the following:
                    // WinRT information: Unexpected number of bytes was transferred. Expected: '. Actual: '.
                    // This appears to be caused by the rapid frequency of writes to the GPIO
                    // These are being swallowed here/

                    // If you want to see the exceptions uncomment the following:
                    // System.Diagnostics.Debug.WriteLine(ex.ToString());

                }



                JsonValues telemetryDataPoint = new JsonValues
                {
                    Id = DateTime.Now.ToString("yyyy-dd-M:hh:mm:ss"),
                    name = "Pi1",
                    humidity = sensorHum,
                    temperature = sensorTemp,
                    date = DateTime.Now.ToString("dd.MM.yyyy"),
                    longitude = lon,
                    latitude = lat


                };

                System.Diagnostics.Debug.WriteLine(telemetryDataPoint);
                var messageString = JsonConvert.SerializeObject(telemetryDataPoint);
                
                var message = new Message(Encoding.ASCII.GetBytes(messageString));
                

                //await deviceClient.SendEventAsync(message);
                //UploadToAzureStorage(messageString);
                await SendAsync(telemetryDataPoint);

                Task.Delay(10000).Wait();
            }
        }

        private async Task<int> UploadToAzureStorage(String messageString)
        {
            try
            {
                //  create Azure Storage
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse("DefaultEndpointsProtocol=https;AccountName=norkartstorageaccount;AccountKey=ywvq9KDDK/F1mqiz1NJ/vEj9lT7wuVrEqWtO1f3hi+i4vw9gOCvJG1rOjVKpjx/Ki1q6rVjG7uUalT+hWdXxCA==");


                //  create a blob client.
                CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

                //  create a container 
                CloudBlobContainer container = blobClient.GetContainerReference("norkartstorageaccount");


                CloudAppendBlob appBlob = container.GetAppendBlobReference("Temperature&Humidity/" + DateTime.Now.ToString("yyyy-dd-M") + ".json");

                //CloudAppendBlob appBlob = container.GetAppendBlobReference("TestFile.txt");

                System.Diagnostics.Debug.WriteLine("APPBLOB EXISTS: " + appBlob.ExistsAsync().Result.ToString());
                if (appBlob.ExistsAsync().Result.ToString() != "True")
                {
                    await appBlob.CreateOrReplaceAsync();
                }
                //  create a local file
                //StorageFile file = await ApplicationData.Current.LocalFolder.CreateFileAsync(DateTime.Now.ToString("yyyy-dd-M--HH-mm-ss"), CreationCollisionOption.GenerateUniqueName);

                //StorageFile file = await ApplicationData.Current.LocalFolder.CreateFileAsync("TestFile.txt", CreationCollisionOption.GenerateUniqueName);


                await appBlob.AppendTextAsync(messageString + Environment.NewLine);

                return 1;
            }
            catch
            {
                //  return error
                System.Diagnostics.Debug.WriteLine("STORAGE ERROR");
                return 0;
            }
        }


        public async Task<HttpStatusCode> SendAsync(JsonValues data)
        {
            HttpResponseMessage response = await _httpClient.PostAsJsonAsync("http://norkartsommerwebapp.azurewebsites.net/api/Values/PostTempAndHum", data);
            System.Diagnostics.Debug.WriteLine(response.StatusCode);
            return response.StatusCode;
        }






        public class JsonValues
        {
            [JsonProperty(PropertyName = "id")]
            public string Id { get; set; }
            public string name { get; set; }
            public double humidity { get; set; }
            public double temperature { get; set; }
            public string date { get;  set;}
            public double longitude { get; set; }
            public double latitude { get; set; }
            public override string ToString()
            {
                return JsonConvert.SerializeObject(this);
            }
        }

        public MainPage()
        {
            this.InitializeComponent();
            deviceClient = DeviceClient.Create(iotHubUri, new DeviceAuthenticationWithRegistrySymmetricKey("Pi1", deviceKey));

            SendDeviceToCloudMessagesAsync();
           
            

        }
    }


}

