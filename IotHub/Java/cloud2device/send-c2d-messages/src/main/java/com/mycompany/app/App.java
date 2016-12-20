package com.mycompany.app;

import com.microsoft.azure.iot.service.sdk.*;

import java.io.IOException;
import java.net.URISyntaxException;

public class App 
{
	// iothub connection string
	private static final String connectionString = "HostName={iothubname}.azure-devices.cn;SharedAccessKeyName=iothubowner;SharedAccessKey={keyforiothubowner}";	
	private static final String deviceId = "mryFirstDevice";
	private static final IotHubServiceClientProtocol protocol = IotHubServiceClientProtocol.AMQPS;
    
	public static void main(String[] args) throws IOException, URISyntaxException, Exception {
		ServiceClient serviceClient = ServiceClient.createFromConnectionString(
			connectionString, protocol);
		
		if (serviceClient != null) {
			serviceClient.open();
			FeedbackReceiver feedbackReceiver = serviceClient.getFeedbackReceiver(deviceId);
			if (feedbackReceiver != null) feedbackReceiver.open();
			
			for (int i=0; i<5; i++) {
				Message messageToSend = new Message("Cloud to device message " + i);
				messageToSend.setDeliveryAcknowledgement(DeliveryAcknowledgement.Full);

				serviceClient.send(deviceId, messageToSend);
				System.out.println("Message sent to device " + i);
				
				FeedbackBatch feedbackBatch = feedbackReceiver.receive(10000);
				if (feedbackBatch != null) {
					System.out.println("Message feedback received, feedback time: "
						+ feedbackBatch.getEnqueuedTimeUtc().toString());
				}
			}		
			
			if (feedbackReceiver != null) feedbackReceiver.close();
			serviceClient.close();
		}
	}
}
