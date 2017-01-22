using Microsoft.Azure.Devices;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackendApp
{
    class Program
    {
        static ServiceClient MyServiceClient;
        // Connection string with Service permission SAS
        static string IoTHubConnectionString = "";

        static EventHubClient MyEventHubClient;
        static string EventHubCompatibleConnectionString = "Endpoint=<eventhub-compatible-endpint>;SharedAccessKeyName=<ServicePermissionSasName>;SharedAccessKey=<ServicePermissionSasKey>;EntityPath=<eventhub-compatible-name>";

        static string TargetDeviceId = "myfirstdevice";

        static void Main(string[] args)
        {
            Console.WriteLine($"Backend App for C2D scenarios, targeting to simulated device {TargetDeviceId}.");
            Console.WriteLine("Please choose actions you'd like take:");
            Console.WriteLine("1. Send C2D messages");
            Console.WriteLine("2. Receive D2C messages");
            Console.WriteLine("3. Receive file upload notification");
            Console.WriteLine("4. Invoke direct method");
            var input = Console.ReadLine();

            switch (input)
            {
                case "1":
                    MyServiceClient = ServiceClient.CreateFromConnectionString(IoTHubConnectionString);
                    ReceiveC2dFeedbackAsync();
                    SendCloudToDeviceMessagesAsync();
                    Console.WriteLine("Start to send messages... Press Enter to stop and exit!");
                    Console.ReadLine();
                    MyServiceClient.CloseAsync().Wait();
                    break;
                case "2":
                    MyEventHubClient = EventHubClient.CreateFromConnectionString(EventHubCompatibleConnectionString);
                    ReceiveDeviceToCloudMessagesAsync();
                    Console.WriteLine("Start to receive messages... Press Enter to stop and exit!");
                    Console.ReadLine();
                    MyEventHubClient.Close();
                    break;
                case "3":
                    MyServiceClient = ServiceClient.CreateFromConnectionString(IoTHubConnectionString);
                    ReceiveFileUploadNotificationAsync();
                    Console.WriteLine("Start to receive file upload notification... Press Enter to stop and exit!");
                    Console.ReadLine();
                    MyServiceClient.CloseAsync().Wait();
                    break;
                case "4":
                    MyServiceClient = ServiceClient.CreateFromConnectionString(IoTHubConnectionString);
                    InvokeDirectMethodAsync();
                    Console.WriteLine("Start to invoke direct method... Press Enter to stop and exit!");
                    Console.ReadLine();
                    MyServiceClient.CloseAsync().Wait();
                    break;
                default:
                    Console.WriteLine("Wrong input! Please just input actions index provided above!");
                    break;
            }                    
        }

        static async void SendCloudToDeviceMessagesAsync()
        {
            var messageString = "cloud hello device";
            var message = new Message(Encoding.UTF8.GetBytes(messageString));
            message.Ack = DeliveryAcknowledgement.Full;
            await MyServiceClient.SendAsync(TargetDeviceId, message);
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"{DateTime.Now} > Sending message: {messageString}");
            Console.ResetColor();
        }

        static async void ReceiveC2dFeedbackAsync()
        {
            var feedbackReceiver = MyServiceClient.GetFeedbackReceiver();

            Console.WriteLine("Enable receiving c2d feedback from service");
            while (true)
            {
                var feedbackBatch = await feedbackReceiver.ReceiveAsync();
                if (feedbackBatch == null) continue;

                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.WriteLine($"Received c2d feedback: {string.Join(", ", feedbackBatch.Records.Select(f => f.StatusCode))}");
                Console.ResetColor();

                await feedbackReceiver.CompleteAsync(feedbackBatch);
                await Task.Delay(10000);
            }
        }

        static async void ReceiveDeviceToCloudMessagesAsync()
        {
            var runtimeInfo = MyEventHubClient.GetRuntimeInformation();
            var defaultConsumerGroup = MyEventHubClient.GetDefaultConsumerGroup();
            var receivers = new List<EventHubReceiver>(runtimeInfo.PartitionCount);

            defaultConsumerGroup.RetryPolicy = new RetryExponential(
                minBackoff: TimeSpan.FromSeconds(1),
                maxBackoff: TimeSpan.FromSeconds(30),
                maxRetryCount: 3);

            foreach (var partitionId in runtimeInfo.PartitionIds)
            {
                // create receiver to receive messages from beginning
                var receiver = await defaultConsumerGroup.CreateReceiverAsync(partitionId, EventHubConsumerGroup.StartOfStream);
                receivers.Add(receiver);
            }

            while (true)
            {
                foreach (var receiver in receivers)
                {
                    Console.WriteLine("Listening to partition: {0}", receiver.PartitionId);
                    var message = receiver.Receive(TimeSpan.FromSeconds(1));
                    if (message != null)
                    {
                        var body = Encoding.UTF8.GetString(message.GetBytes());
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"Received message: sequence {message.SequenceNumber} | offset {message.Offset} | EnqueueTime {message.EnqueuedTimeUtc} \nbody: {body}");
                        Console.ResetColor();
                    }
                }

                await Task.Delay(TimeSpan.FromSeconds(1));
            }
        }

        static async void ReceiveFileUploadNotificationAsync()
        {
            var notificationReceiver = MyServiceClient.GetFileNotificationReceiver();
            
            while (true)
            {
                var fileUploadNotification = await notificationReceiver.ReceiveAsync();
                if (fileUploadNotification == null) continue;

                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine($"Received file upload noticiation: {fileUploadNotification.BlobName}");
                Console.ResetColor();

                await notificationReceiver.CompleteAsync(fileUploadNotification);

                await Task.Delay(TimeSpan.FromSeconds(1));
            }
        }

        static async void InvokeDirectMethodAsync()
        {
            var directMethod = new CloudToDeviceMethod("print");
            directMethod.ResponseTimeout = TimeSpan.FromSeconds(30);

            var directMethodResult = await MyServiceClient.InvokeDeviceMethodAsync(TargetDeviceId, directMethod);
            Console.WriteLine($"Direct method invocation result: {directMethodResult.Status}");
        }
    }
}
