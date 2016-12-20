package com.hula.demo;

import java.io.IOException;
import java.util.Map;

import org.apache.logging.log4j.LogManager;
import org.apache.logging.log4j.Logger;

import com.microsoft.azure.iot.service.sdk.DeliveryAcknowledgement;
import com.microsoft.azure.iot.service.sdk.IotHubServiceClientProtocol;
import com.microsoft.azure.iot.service.sdk.Message;
import com.microsoft.azure.iot.service.sdk.ServiceClient;

import backtype.storm.task.OutputCollector;
import backtype.storm.task.TopologyContext;
import backtype.storm.topology.OutputFieldsDeclarer;
import backtype.storm.topology.base.BaseRichBolt;
import backtype.storm.tuple.Tuple;

public class Cloud2DeviceBolt extends BaseRichBolt {
	/**
	 * 
	 */
	private static final long serialVersionUID = 1L;
	
	private static final Logger logger = LogManager.getLogger(Cloud2DeviceBolt.class);
	protected OutputCollector collector;	
	protected ServiceClient serviceClient;	
	protected String connectionString;
	protected IotHubServiceClientProtocol protocol = IotHubServiceClientProtocol.AMQPS;
	protected int count = 0;
	
	public Cloud2DeviceBolt(String connectionString){
		this.connectionString = connectionString;
	}

	@SuppressWarnings("rawtypes")
	@Override
	public void prepare(Map stormConf, TopologyContext context, OutputCollector collector) {
		this.collector = collector;
		try {
			logger.info("creating serviceClient: " + connectionString);
			serviceClient = ServiceClient.createFromConnectionString(connectionString, protocol);
			serviceClient.open();
		} catch (Exception e) {
			logger.error("creating serviceClient failed: " + connectionString, e);
			throw new RuntimeException(e);
		}
	}

	@Override
	public void execute(Tuple input) {
		String deviceId = (String) input.getValue(0);
		String data = (String) input.getValue(1);
		
		logger.info("device id: " + deviceId);
		logger.info("data: " + data);

		try {
			if(deviceId == null || deviceId == ""){
				logger.error("deviceId is null or empty");
				return;
			}
			
			Message messageToSend = new Message(data);
			messageToSend.setDeliveryAcknowledgement(DeliveryAcknowledgement.Full);
			serviceClient.send(deviceId, messageToSend);
			count++;
			logger.info("Message sent to device: " + count);
			collector.ack(input);
		} catch (Exception e) {
			logger.error("Send message failed with ex: ", e);
			collector.fail(input);
			try {
				serviceClient.close();
				logger.error("serviceClient closed");
			} catch (IOException e2) {
				logger.error("serviceClient close failed with ex", e2);
			}
			return;
		} 
		/*finally {
			try {
				serviceClient.close();
				logger.error("serviceClient closed");
			} catch (IOException e) {
				logger.error("serviceClient close failed with ex", e);
			}
		}*/
	}

	@Override
	public void declareOutputFields(OutputFieldsDeclarer declarer) {
		// TODO Auto-generated method stub

	}

}
