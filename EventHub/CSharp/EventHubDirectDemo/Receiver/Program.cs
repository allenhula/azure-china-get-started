using Microsoft.ServiceBus.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Receiver
{
    class Program
    {
        static string connectionString = "";

        static void Main(string[] args)
        {
            var eventHubClient = EventHubClient.CreateFromConnectionString(connectionString);
            var consumerGroup = eventHubClient.GetDefaultConsumerGroup();
            var runtimeInfo = eventHubClient.GetRuntimeInformation();
            var partitionCount = runtimeInfo.PartitionCount;

            var receiverList = new List<EventHubReceiver>(partitionCount);

            foreach (var partitionId in runtimeInfo.PartitionIds)
            {
                receiverList.Add(consumerGroup.CreateReceiver(partitionId));
            }

            while (true)
            {
                foreach (var receiver in receiverList)
                {
                    Console.WriteLine("Listening to partition: {0}", receiver.PartitionId);
                    var eventData = receiver.Receive(TimeSpan.FromSeconds(1));
                    if (eventData != null)
                    {
                        var body = Encoding.UTF8.GetString(eventData.GetBytes());
                        Console.WriteLine("Received event: key {0} | sequence {1} | offset {2} | EnqueueTime {3} \nbody: {4}", 
                            eventData.PartitionKey, 
                            eventData.SequenceNumber,
                            eventData.Offset,
                            eventData.EnqueuedTimeUtc,
                            body);
                    }                    
                }
                Thread.Sleep(200);

                Console.WriteLine("Continue? [Y | N]");
                var input = Console.ReadLine();
                if (!input.Equals("Y", StringComparison.InvariantCultureIgnoreCase))
                {
                    break;
                }
            }
        }
    }
}
