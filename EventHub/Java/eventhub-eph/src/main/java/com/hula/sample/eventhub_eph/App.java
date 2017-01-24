package com.hula.sample.eventhub_eph;

import java.nio.charset.Charset;
import java.util.concurrent.ExecutionException;
import java.util.function.Consumer;

import com.microsoft.azure.eventhubs.EventData;
import com.microsoft.azure.eventhubs.EventHubClient;
import com.microsoft.azure.eventprocessorhost.EventProcessorHost;
import com.microsoft.azure.eventprocessorhost.EventProcessorOptions;
import com.microsoft.azure.eventprocessorhost.ExceptionReceivedEventArgs;

public class App {
	public static void main(String[] args) {
		// TODO: update to your value 
		String namespaceName = "";
		String eventHubName = "";
		String sasKeyName = "";
		String sasKey = "";
		String storageConnectionString = "";
		String storageContainerName = "";
		String hostName = "demohost";
		
		// For mooncake
		URI endpointUri = URI.create("amqps://" + namespaceName + ".servicebus.chinacloudapi.cn/");
		ConnectionStringBuilder connStrBuilder = new ConnectionStringBuilder(endpointUri, eventHubName, sasKeyName, sasKey);
		String eventHubConnectionString = connStrBuilder.toString();

		System.out.println("Creating Event Processor Host named " + hostName);
		EventProcessorHost epHost = new EventProcessorHost(hostName, eventHubName,
				EventHubClient.DEFAULT_CONSUMER_GROUP_NAME, eventHubConnectionString, storageConnectionString,
				storageContainerName);

		System.out.println("Registering Event Processor Host named " + epHost.getHostName());
		EventProcessorOptions options = new EventProcessorOptions();
		options.setExceptionNotification(new ErrorNotificationHandler());
		
		try {
			epHost.registerEventProcessor(DemoEventProcessor.class, options).get();
			System.out.println("Demo Event Processor has been registered, start to receive and process events now...");
		} catch (Exception e) {
			System.out.println("Register failed!");
			e.printStackTrace();
		}
		
		System.out.println("Press enter to stop");
        try
        {
            System.in.read();            
            // Processing of events continues until unregisterEventProcessor is called. Unregistering shuts down the
            // receivers on all currently owned leases, shuts down the instances of the event processor class, and
            // releases the leases for other instances of EventProcessorHost to claim.
            System.out.println("Calling unregister");
            epHost.unregisterEventProcessor();
            
            // There are two options for shutting down EventProcessorHost's internal thread pool: automatic and manual.
            // Both have their advantages and drawbacks. See the JavaDocs for setAutoExecutorShutdown and forceExecutorShutdown
            // for more details. This example uses forceExecutorShutdown because it is the safe option, at the expense of
            // another line of code.
            System.out.println("Calling forceExecutorShutdown");
            EventProcessorHost.forceExecutorShutdown(120);
        }
        catch(Exception e)
        {
            System.out.println("Unregister or Shutdown failed!");
            e.printStackTrace();
        }
        System.out.println("End of sample");
	}
	
	public static class ErrorNotificationHandler implements Consumer<ExceptionReceivedEventArgs>
    {
		public void accept(ExceptionReceivedEventArgs t)
		{
			System.out.println("SAMPLE: Host " + t.getHostname() + " received general error notification during " + t.getAction() + ": " + t.getException().toString());
		}
    }
}
