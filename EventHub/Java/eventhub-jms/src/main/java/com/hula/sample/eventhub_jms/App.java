package com.hula.sample.eventhub_jms;

import java.nio.charset.Charset;
import java.util.Hashtable;

import javax.jms.BytesMessage;
import javax.jms.Connection;
import javax.jms.ConnectionFactory;
import javax.jms.DeliveryMode;
import javax.jms.Destination;
import javax.jms.ExceptionListener;
import javax.jms.JMSException;
import javax.jms.Message;
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
		System.out.println("Demo for JMS messaging to Eventhub!!!");
		try {
			Hashtable<String, String> env = new Hashtable<String, String>();
			// TODO: update to your own local file path.
			env.put(Context.PROVIDER_URL,
					"<Path of jndi.properties>");
			Context context = new InitialContext(env);

			ConnectionFactory factory = (ConnectionFactory) context.lookup("myFactoryLookup");
			Destination eventhub = (Destination) context.lookup("myEventhubLookup");

			Connection connection = factory.createConnection();
			System.out.println(connection.getMetaData().toString());
			connection.setExceptionListener(new MyExceptionListener());

			Session sendSession = connection.createSession(false, Session.AUTO_ACKNOWLEDGE);
			MessageProducer sender = sendSession.createProducer(eventhub);
			System.out.println(sender.getDestination().toString());

			for (int i = 0; i < 5; i++) {
				String msgString = "Test AMQP message from JMS to Event Hub " + i;
				BytesMessage message = sendSession.createBytesMessage();
				message.writeBytes(msgString.getBytes(Charset.defaultCharset()));
				sender.send(message);
				System.out.println("Message sent: " + msgString);
			}

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
