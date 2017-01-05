using Microsoft.ServiceBus.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventProcessorHostDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            // TODO update to your values
            // Namespace level connection string. Be specific, no EntityPath included
            var eventHubNamespaceConnectionString = "";
            var eventHubName = "";
            var storageConnectionString = "";

            var hostName = Guid.NewGuid().ToString();
            EventProcessorHost eventProcessorHost = new EventProcessorHost(hostName, eventHubName, EventHubConsumerGroup.DefaultGroupName, eventHubNamespaceConnectionString, storageConnectionString);
            Console.WriteLine("Registering EventProcessor...");
            var options = new EventProcessorOptions();
            options.ExceptionReceived += (sender, e) => { Console.WriteLine("Received exception when processing. Exception: {0}", e.Exception); };
            eventProcessorHost.RegisterEventProcessorAsync<DemoEventProcessor>(options).Wait();

            Console.WriteLine("Start receiving... Press enter key to stop worker.");
            Console.ReadLine();
            eventProcessorHost.UnregisterEventProcessorAsync().Wait();

            Console.WriteLine("End of Sample!");
        }
    }
}
