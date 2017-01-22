using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Common.Exceptions;
using Microsoft.Azure.Devices.Shared;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeviceIdentityOps
{
    class Program
    {
        static RegistryManager MyRegistryManager;
        // Connection string with RegistryWrite and Service permission SAS
        static string ConnectionString = "";
        static string DeviceId = "myfirstdevice";

        static void Main(string[] args)
        {
            MyRegistryManager = RegistryManager.CreateFromConnectionString(ConnectionString);
            RegisterDeviceAsync().Wait();
            AddTagsAndQuery().Wait();

            Console.WriteLine("\nPress Enter to exit!");
            Console.ReadLine();
            MyRegistryManager.CloseAsync().Wait();
        }

        static async Task RegisterDeviceAsync()
        {
            var device = new Device(DeviceId);

            try
            {
                device = await MyRegistryManager.AddDeviceAsync(device);
            }
            catch (DeviceAlreadyExistsException)
            {
                device = await MyRegistryManager.GetDeviceAsync(DeviceId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Got unexpected exception: {ex.Message}");
            }

            Console.WriteLine($"Device register successfully!\n > Device Id: {DeviceId}\n > Device Key: {device.Authentication.SymmetricKey.PrimaryKey}");
        }

        static async Task AddTagsAndQuery()
        {
            var twin = await MyRegistryManager.GetTwinAsync(DeviceId);
            Console.WriteLine($"Current Twin of device {DeviceId}: \n{twin.ToJson(Formatting.Indented)}");

            dynamic patch;

            if (twin.Tags.Count > 0)
            {
                patch = new
                {
                    properties = new
                    {
                        desired = new
                        {
                            telemetryConfig = new
                            {
                                sendFrequency = Guid.NewGuid().ToString()
                            }
                        }
                    }
                };
            }
            else
            {
                patch = new
                {
                    tags = new
                    {
                        location = new
                        {
                            region = "CN",
                            city = "SH"
                        }
                    },
                    properties = new
                    {
                        desired = new
                        {
                            telemetryConfig = new
                            {
                                sendFrequency = "5m"
                            }
                        }
                    }
                };
            }

            Console.WriteLine("\nUpdating twin...");
            await MyRegistryManager.UpdateTwinAsync(DeviceId, JsonConvert.SerializeObject(patch), twin.ETag);
            twin = await MyRegistryManager.GetTwinAsync(DeviceId);
            Console.WriteLine($"Updated twin: \n{twin.ToJson(Formatting.Indented)}");

            Console.WriteLine("\nQuery with tags...");
            var query = MyRegistryManager.CreateQuery("SELECT * FROM devices WHERE tags.location.city = 'SH'", 10);
            while (query.HasMoreResults)
            {
                var twinInSh = await query.GetNextAsTwinAsync();
                Console.WriteLine($"Devices in SH: {string.Join(", ", twinInSh.Select(t => t.DeviceId))}");
            }
        }
    }
}
