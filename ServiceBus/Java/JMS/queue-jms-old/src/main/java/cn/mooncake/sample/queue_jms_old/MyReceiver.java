package cn.mooncake.sample.queue_jms_old;

import javax.jms.Message;
import javax.jms.MessageListener;
import javax.jms.TextMessage;

public class MyReceiver implements MessageListener {

	public void onMessage(Message message) {
		try {
            System.out.println("Received message with JMSMessageID = " + message.getJMSMessageID());
            TextMessage txtmessage = (TextMessage) message;
            System.out.println("                      Text = " + txtmessage.getText());
            message.acknowledge();
        } catch (Exception e) {
            e.printStackTrace();
        }	
	}

}
