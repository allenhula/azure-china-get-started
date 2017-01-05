package com.hula.sample.eventhub_direct;

import java.io.IOException;
import java.time.Instant;
import java.util.concurrent.ExecutionException;

import com.microsoft.azure.eventhubs.EventData;
import com.microsoft.azure.eventhubs.EventHubClient;
import com.microsoft.azure.eventhubs.PartitionSender;
import com.microsoft.azure.servicebus.ServiceBusException;

public class Sender {
	public static void main(String[] args)
			throws InterruptedException, ExecutionException, ServiceBusException, IOException {
		// TODO: update to your values
		String namespaceName = "";
		String eventHubName = "";
		String sasKeyName = "";
		String sasKey = "";

		// NOTE: this is to workaround issue https://github.com/Azure/azure-event-hubs-java/issues/22
		// Once the issue is fixed, it is better to use ConnectionStringBuilder to create the connection string
		String endpoint = "sb://" + namespaceName + ".servicebus.chinacloudapi.cn/";
		// Endpoint=<eventhub_compatible_endpoint>;SharedAccessKeyName=<iothub_sas_policy_name>;SharedAccessKey=<iothub_sas_policy_key>;EntityPath=<eventhub_compatible_name>
		String connectionString = String.format("Endpoint=%s;SharedAccessKeyName=%s;SharedAccessKey=%s;EntityPath=%s",
				endpoint, sasKeyName, sasKey, eventHubName);

		EventHubClient client = EventHubClient.createFromConnectionString(connectionString).get();
		System.out.println(Instant.now() + ": Event Hub" + eventHubName + "connected!");
		System.out.println(Instant.now() + ": Start to send messages...");
		
		for (int i = 0; i < 4; i++) {
			String message = "Text Message " + i;
			EventData eventData = new EventData(message.getBytes("UTF-8"));
			System.out.println(Instant.now() + ": Send message: " + message);

			// Type-1 - Basic Send - not tied to any partition, round-robin fashion
			client.send(eventData).get();
			System.out.println(Instant.now() + ": Basic send done!");

			// Type-2 - Send using PartitionKey - all Events with Same
			// partitionKey will land on the same Partition
			String partitionKey = "device" + i;
			client.send(eventData, partitionKey).get();
			System.out.println(Instant.now() + ": Send with partition key [" + partitionKey + "] done!");
			
			// Type-3 - Send to a Specific Partition
			PartitionSender sender = client.createPartitionSender(Integer.toString(i)).get();
			sender.send(eventData).get();
			System.out.println(Instant.now() + ": Send to specific partition [" + Integer.toString(i) + "] done!");
		}

		System.out.println(Instant.now() + ": Send Complete... Exit!");
		client.close().get();
	}
}
