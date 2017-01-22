using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;
using System;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Microsoft.Azure.Devices.Shared;

namespace SimulatedDevice
{
    class Program
    {
        static DeviceClient MyDeviceClient;
        static string IotHubFullName = "";

        // For device authentication with registry sysmmetric key 
        static string DeviceId = "myfirstdevice";
        static string DeviceKey = "";

        // For device authentication with SAS key having device connect permission 
        //static string DeviceSasName = "";
        //static string DeviceSasKey = "";

        static void Main()
        {
            Console.WriteLine($"Simulated device {DeviceId} for D2C scenarios.");
            Console.WriteLine("Please choose actions you'd like take:");
            Console.WriteLine("1. Send D2C messages");
            Console.WriteLine("2. Receive C2D messages");
            Console.WriteLine("3. Upload files");
            Console.WriteLine("4. Get and update twin");
            Console.WriteLine("5. Expose direct method");
            var input = Console.ReadLine();

            // Connect to IoT Hub using sysmmetric key device authentication
            var authViaDeviceKey = new DeviceAuthenticationWithRegistrySymmetricKey(DeviceId, DeviceKey);
            MyDeviceClient = DeviceClient.Create(IotHubFullName, authViaDeviceKey, TransportType.Mqtt);

            // Connect to IoT Hub using SAS device authentication
            //var authViaSasKey = new DeviceAuthenticationWithSharedAccessPolicyKey(DeviceId, DeviceSasName, DeviceSasKey);
            //MyDeviceClient = DeviceClient.Create(IotHubFullName, authViaSasKey, TransportType.Mqtt);

            switch (input)
            {
                case "1":
                    SendDeviceToCloudMessagesAsync();
                    break;
                case "2":
                    ReceiveCloudToDeviceMessagesAsync();
                    break;
                case "3":
                    UploadFilesAsync();
                    break;
                case "4":
                    GetAndUpdateTwinAsync();
                    break;
                case "5":
                    ExposeDirectMethodAsync();
                    break;
                default:
                    Console.WriteLine("Wrong input! Please just input actions index provided above!");
                    break;
            }

            Console.ReadLine();
            MyDeviceClient.CloseAsync().Wait();
        }

        static async void SendDeviceToCloudMessagesAsync()
        {
            Console.WriteLine("Start to send D2C messages...");
            double avgWindSpeed = 10; // m/s
            Random rand = new Random();

            while (true)
            {
                double currentWindSpeed = avgWindSpeed + rand.NextDouble() * 4;

                var telemetryDataPoint = new
                {
                    deviceId = "myFirstDevice",
                    windSpeed = currentWindSpeed
                };
                var messageString = JsonConvert.SerializeObject(telemetryDataPoint);
                string level = currentWindSpeed >= 13.2 ? "critical" : "normal";

                var message = new Message(Encoding.UTF8.GetBytes(messageString));
                message.Properties.Add("level", level);
                
                await MyDeviceClient.SendEventAsync(message);
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"{DateTime.Now} > Sending message: {messageString}, level: {level}");
                Console.ResetColor();

                await Task.Delay(1000);
                Console.WriteLine("Sending... Press Enter to stop and exit!");
            }
        }

        static async void ReceiveCloudToDeviceMessagesAsync()
        {
            Console.WriteLine("Start to receive C2D messages...");
            while (true)
            {
                var receivedMessage = await MyDeviceClient.ReceiveAsync(TimeSpan.FromSeconds(1));
                if (receivedMessage == null) continue;

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"{DateTime.Now} < Received message: {Encoding.UTF8.GetString(receivedMessage.GetBytes())}");
                Console.ResetColor();

                await MyDeviceClient.CompleteAsync(receivedMessage);
                await Task.Delay(100);
                Console.WriteLine("Receiving... Press Enter to stop and exit!");
            }
        }

        static async void UploadFilesAsync()
        {
            var fileName = "IoT.png";
            var filePath = @".\img\IoT.png";
            Console.WriteLine($"Uploading file: {fileName}");

            using (var file2Upload = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                await MyDeviceClient.UploadToBlobAsync(fileName, file2Upload);
            }
            
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine($"{DateTime.Now} < File uploaded: {fileName}");
            Console.ResetColor();

            Console.WriteLine("Done! Press Enter to stop and exit!");
        }

        static async void GetAndUpdateTwinAsync()
        {
            Console.WriteLine($"Get twin of device {DeviceId}");
            var twin = await MyDeviceClient.GetTwinAsync();
            Console.WriteLine($"{twin.ToJson(Formatting.Indented)}");

            var propertyName = "reportedProp";
            var propertyValue = Guid.NewGuid().ToString();
            var reportedProps = new TwinCollection();
            reportedProps[propertyName] = propertyValue;
            Console.WriteLine($"\nUpdate reported property {propertyName} with new value {propertyValue}");
            await MyDeviceClient.UpdateReportedPropertiesAsync(reportedProps);

            Console.WriteLine($"\nSet desired property update callback to print the new value");
            await MyDeviceClient.SetDesiredPropertyUpdateCallback(
                (patch, context) =>
                {
                    return Task.Run(() =>
                    {
                        Console.BackgroundColor = ConsoleColor.DarkGreen;
                        Console.WriteLine($"callback - updated value: \n{patch["telemetryConfig"].ToString()}");
                        Console.ResetColor();
                    });
                }, null
                );

            Console.WriteLine("\nWaiting... Press Enter to stop and exit!");
        }

        static void ExposeDirectMethodAsync()
        {
            Console.WriteLine("Expose direct method!");
            var methodName = "print";
            MyDeviceClient.SetMethodHandler(methodName,
                (payload, context) => 
                {
                    Console.BackgroundColor = ConsoleColor.DarkCyan;
                    Console.WriteLine($"Direct method {methodName} is called.");
                    Console.ResetColor();
                    return Task.FromResult(new MethodResponse(200));
                }, 
                null);
            Console.WriteLine($"\nMethod {methodName} exposed, waiting for invoke... Press Enter to stop and exit!");
        }
    }
}
