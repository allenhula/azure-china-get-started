package cn.mooncake.sample.queue_http;

import java.io.BufferedReader;
import java.io.IOException;
import java.io.InputStreamReader;

import com.microsoft.windowsazure.Configuration;
import com.microsoft.windowsazure.exception.ServiceException;
import com.microsoft.windowsazure.services.servicebus.ServiceBusConfiguration;
import com.microsoft.windowsazure.services.servicebus.ServiceBusContract;
import com.microsoft.windowsazure.services.servicebus.ServiceBusService;
import com.microsoft.windowsazure.services.servicebus.models.BrokeredMessage;
import com.microsoft.windowsazure.services.servicebus.models.QueueInfo;
import com.microsoft.windowsazure.services.servicebus.models.ReceiveMessageOptions;
import com.microsoft.windowsazure.services.servicebus.models.ReceiveMode;
import com.microsoft.windowsazure.services.servicebus.models.ReceiveQueueMessageResult;

/**
 * Hello world!
 *
 */
public class App {
	public static void main(String[] args) throws IOException {
		System.out.println("Demo for Service Bus Queue via HTTP!");
		// TODO: update to your value
		String namespace = "";
		String queueName = "";
		String sasKeyName = "";
		String sasKey = "";

		Configuration config = ServiceBusConfiguration.configureWithSASAuthentication(namespace, sasKeyName, sasKey,
				".servicebus.chinacloudapi.cn");
		ServiceBusContract sbService = ServiceBusService.create(config);

		// Create Queue or Update Queue if existed
		QueueInfo queueInfo = new QueueInfo(queueName);
		queueInfo.setEnableBatchedOperations(true);
		try {
			sbService.createQueue(queueInfo);
			System.out.println("Create queue " + queueName + " successfully!");
		} catch (ServiceException createEx) {
			// TODO Auto-generated catch block
			if (createEx.getHttpStatusCode() == 409) {
				System.out.println("Queue " + queueName + " existed already! Trying to update it...");
				try {
					sbService.updateQueue(queueInfo);
					System.out.println("Update queue " + queueName + " successfully!");
				} catch (ServiceException updateEx) {
					System.out.println("Update queue " + queueName + " failed!");
					updateEx.printStackTrace();
					System.exit(-1);
				}
			} else {
				System.out.println("Create queue " + queueName + " failed!");
				createEx.printStackTrace();
			}
		}

		// Send Message to queue
		try {
			for (int i = 0; i < 5; i++) {
				String msgBody = "Test message from HTTP " + i;
				// Create message, passing a string message for the body.
				BrokeredMessage message = new BrokeredMessage(msgBody);
				// Set an additional app-specific property.
				message.setProperty("CustomProperty", i);
				// Send message to the queue
				sbService.sendQueueMessage(queueName, message);
				
				System.out.println("Send message: " + msgBody);
			}
		} catch (ServiceException e) {
			System.out.print("ServiceException encountered: ");
			e.printStackTrace();
			System.exit(-1);
		}

		System.out.println("Press Enter to start to receive messages now");
		BufferedReader commandLine = new java.io.BufferedReader(new InputStreamReader(System.in));
		commandLine.readLine();

		// Receive message from queue
		try {
			ReceiveMessageOptions opts = ReceiveMessageOptions.DEFAULT;
			// Default ReceiveMode is PEEK_LOCK
			opts.setReceiveMode(ReceiveMode.RECEIVE_AND_DELETE);
			// opts.setReceiveMode(ReceiveMode.PEEK_LOCK);

			while (true) {
				ReceiveQueueMessageResult resultQM = sbService.receiveQueueMessage(queueName, opts);
				BrokeredMessage message = resultQM.getValue();
				if (message != null && message.getMessageId() != null) {
					System.out.println("MessageID: " + message.getMessageId());
					// Display the queue message.
					System.out.print("From queue: ");
					byte[] b = new byte[200];
					String s = null;
					int numRead = message.getBody().read(b);
					while (-1 != numRead) {
						s = new String(b);
						s = s.trim();
						System.out.print(s);
						numRead = message.getBody().read(b);
					}
					System.out.println();
					System.out.println("Custom Property: " + message.getProperty("CustomProperty"));
					// Remove message from queue if PEEK_LOCK mode.
					// System.out.println("Deleting this message.");
					// sbService.deleteMessage(message);
				} else {
					System.out.println("No more message, exit...");
					break;
				}
			}
		} catch (ServiceException e) {
			System.out.print("ServiceException encountered: ");
			e.printStackTrace();
			System.exit(-1);
		} catch (Exception e) {
			System.out.print("Generic exception encountered: ");
			e.printStackTrace();
			System.exit(-1);
		}
	}
}
