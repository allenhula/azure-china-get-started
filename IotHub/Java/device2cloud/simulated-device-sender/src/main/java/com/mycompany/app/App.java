package com.mycompany.app;

import com.microsoft.azure.iothub.DeviceClient;
import com.microsoft.azure.iothub.IotHubClientProtocol;
import com.microsoft.azure.iothub.Message;
import com.microsoft.azure.iothub.IotHubStatusCode;
import com.microsoft.azure.iothub.IotHubEventCallback;
import com.microsoft.azure.iothub.IotHubMessageResult;
import com.google.gson.Gson;
import java.io.IOException;
import java.net.URISyntaxException;
import java.util.Random;
import java.util.concurrent.Executors;
import java.util.concurrent.TimeUnit;
import java.util.concurrent.ExecutorService;

public class App 
{
	// connection string with device id and device key which are generated in project create-device-identity
	private static String connString = "HostName={iothubname}.azure-devices.cn;DeviceId={deviceid};SharedAccessKey={devicekey}";
	private static IotHubClientProtocol protocol = IotHubClientProtocol.AMQPS;
	private static String deviceId = "myFirstDevice";
	private static DeviceClient client;

    public static void main( String[] args ) throws IOException, URISyntaxException, InterruptedException {
		client = new DeviceClient(connString, protocol);
		// below set certificate path is required for AMQPS protocol
		// download the wosign root certificate from http://www.wosign.com/Root/index.htm#, save to your local. e.g. D:Certs\WS_CA1_NEW.cer
    	client.setOption("SetCertificatePath", "D:\\Certs\\WS_CA1_NEW.cer");
		client.open();

		// send 5 messages
		for (int i = 0; i <= 5; i++)
		{
			String msgStr = "message " + i;
			Message msg = new Message(msgStr);
			msg.setExpiryTime(5000);
			
			EventCallback eventCallback = new EventCallback();
			Object lockobj = new Object();
			client.sendEventAsync(msg, eventCallback, null);
			System.out.println("sent message " + i);
			
			synchronized (lockobj) {
				lockobj.wait();
			}
		}

		System.out.println("Press ENTER to exit.");
		System.in.read();
		client.close(); 
	}
	
    protected static class EventCallback implements IotHubEventCallback
    {
    	public void execute(IotHubStatusCode status, Object context)
        {
            System.out.println("IoT Hub responded to message with status " + status.name());

            if (context != null) {
                synchronized (context) {
                    context.notify();
                }
            }
        }
    }
}
