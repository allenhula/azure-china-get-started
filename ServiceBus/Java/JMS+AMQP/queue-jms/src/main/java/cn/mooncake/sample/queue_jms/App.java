package cn.mooncake.sample.queue_jms;

import java.util.Hashtable;

import javax.jms.Connection;
import javax.jms.ConnectionFactory;
import javax.jms.DeliveryMode;
import javax.jms.Destination;
import javax.jms.ExceptionListener;
import javax.jms.JMSException;
import javax.jms.Message;
import javax.jms.MessageConsumer;
import javax.jms.MessageProducer;
import javax.jms.Session;
import javax.jms.TextMessage;
import javax.naming.Context;
import javax.naming.InitialContext;

/**
 * Hello world!
 *
 */
public class App {
	public static void main(String[] args) {
		System.out.println("Demo for qpid-jms-client!!!");
		try {
			Hashtable<String, String> env = new Hashtable<String, String>();
			// TODO: update to your local path
			env.put(Context.PROVIDER_URL,
					"D://Pilot//Java//servicebus//queue-jms//src//main//resources//jndi.properties");
			Context context = new InitialContext(env);
			
			ConnectionFactory factory = (ConnectionFactory) context.lookup("myFactoryLookup");
			Destination queue = (Destination) context.lookup("myQueueLookup");

			Connection connection = factory.createConnection();
			System.out.println(connection.getMetaData().toString());
			connection.setExceptionListener(new MyExceptionListener());

			Session sendSession = connection.createSession(false, Session.AUTO_ACKNOWLEDGE);
			MessageProducer messageProducer = sendSession.createProducer(queue);
			System.out.println(messageProducer.getDestination().toString());

			Session receiveSession = connection.createSession(false, Session.CLIENT_ACKNOWLEDGE);
			MessageConsumer messageConsumer = receiveSession.createConsumer(queue);
			messageConsumer.setMessageListener(new MyReceiver());

			connection.start();

			for (int i = 0; i < 5; i++) {
				TextMessage message = sendSession.createTextMessage("qpid-jms-client Text Message " + i);
				messageProducer.send(message, DeliveryMode.NON_PERSISTENT, Message.DEFAULT_PRIORITY,
						Message.DEFAULT_TIME_TO_LIVE);
				System.out.println("Message sent: " + message.getText());
			}

			System.out.println("Sleep for 1 minute...");
			Thread.sleep(60000);
			
			System.out.println("Close connection and exit!");
			connection.close();
		} catch (Exception exp) {
			System.out.println("Caught exception, exiting.");
			exp.printStackTrace(System.out);
			System.exit(1);
		}
	}

	private static class MyExceptionListener implements ExceptionListener {
		public void onException(JMSException exception) {
			System.out.println("Connection ExceptionListener fired, exiting.");
			exception.printStackTrace(System.out);
			System.exit(1);
		}
	}
}
