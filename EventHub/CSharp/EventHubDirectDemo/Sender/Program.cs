using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using System;
using System.Globalization;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Web;

namespace Sender
{
    class Program
    {
        static string connectionString = "";

        static void Main(string[] args)
        {
            Console.WriteLine("Press Ctrl-C to stop the sender process");
            Console.WriteLine("Press Enter to start now");
            Console.ReadLine();
            SendMessages();
        }

        static void SendMessages()
        {
            var eventHubClient = EventHubClient.CreateFromConnectionString(connectionString);

            for (int i = 0; i < 1; i++)
            {
                try
                {
                    var message = Guid.NewGuid().ToString();
                    Console.WriteLine("{0} > Sending message: {1}", DateTime.Now, message);
                    var eventData = new EventData(Encoding.UTF8.GetBytes(message));
                    eventData.PartitionKey = "device" + i;
                    eventHubClient.Send(eventData);
                }
                catch (Exception exception)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("{0} > Exception: {1}", DateTime.Now, exception.Message);
                    Console.ResetColor();
                }

                Thread.Sleep(200);
            }
        }
    }
}
