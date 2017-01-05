package com.hula.sample.eventhub_direct;

import java.io.IOException;
import java.nio.charset.Charset;
import java.time.Duration;
import java.util.ArrayList;
import java.util.List;
import java.util.concurrent.ExecutionException;

import com.microsoft.azure.eventhubs.EventData;
import com.microsoft.azure.eventhubs.EventHubClient;
import com.microsoft.azure.eventhubs.PartitionReceiver;
import com.microsoft.azure.servicebus.ServiceBusException;

public class Receiver {

	public static void main(String[] args)
			throws InterruptedException, ExecutionException, ServiceBusException, IOException {
		// TODO: update to your value
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

		List<PartitionReceiver> receivers = new ArrayList<PartitionReceiver>();
		for (int i = 0; i < 4; i++) {
			PartitionReceiver receiver = client.createReceiverSync(EventHubClient.DEFAULT_CONSUMER_GROUP_NAME, Integer.toString(i), PartitionReceiver.START_OF_STREAM);
			receivers.add(receiver);
		}
		
		for(PartitionReceiver receiver : receivers) {
			System.out.println(">>>>>Receive message from Partition: " + receiver.getPartitionId());
			receiver.setReceiveTimeout(Duration.ofSeconds(2));
			Iterable<EventData> receivedEvents = receiver.receive(100).get();
			for(EventData receivedEvent: receivedEvents)
			{
				System.out.println(String.format("Message Payload: %s", new String(receivedEvent.getBody(), Charset.defaultCharset())));
				System.out.println(String.format("PartitionKey: %s, Offset: %s, SeqNo: %s, EnqueueTime: %s", 
						receivedEvent.getSystemProperties().getPartitionKey(),
						receivedEvent.getSystemProperties().getOffset(), 
						receivedEvent.getSystemProperties().getSequenceNumber(), 
						receivedEvent.getSystemProperties().getEnqueuedTime()));
			}
			receiver.close().get();
			System.out.println(">>>>>Receive ending...Close!");
		}
		client.close().get();
	}
}
