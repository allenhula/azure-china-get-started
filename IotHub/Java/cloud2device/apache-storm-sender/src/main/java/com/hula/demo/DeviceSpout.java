package com.hula.demo;

import java.util.Map;
import java.util.Random;
import java.util.UUID;

import org.apache.storm.shade.org.json.simple.JSONObject;

import backtype.storm.spout.SpoutOutputCollector;
import backtype.storm.task.TopologyContext;
import backtype.storm.topology.OutputFieldsDeclarer;
import backtype.storm.topology.base.BaseRichSpout;
import backtype.storm.tuple.Fields;
import backtype.storm.tuple.Values;
import backtype.storm.utils.Utils;

public class DeviceSpout extends BaseRichSpout {
	/**
	 * 
	 */
	private static final long serialVersionUID = 1L;
	//Collector used to emit output
	  SpoutOutputCollector _collector;
	  //Used to generate a random number
	  Random _rand;
	
	@SuppressWarnings("rawtypes")
	@Override
	public void open(Map conf, TopologyContext context, SpoutOutputCollector collector) {
		//Set the instance collector to the one passed in
	    _collector = collector;
	    //For randomness
	    _rand = new Random();
	}

	@Override
	public void nextTuple() {
		//Sleep for a bit
	    Utils.sleep(100);
	    
		String randomId = UUID.randomUUID().toString();
	    int randomNo = _rand.nextInt(10000);
	    
	    JSONObject message = new JSONObject();
	    message.put("deviceId", randomId);
	    message.put("deviceValue", randomNo);
	    
	    _collector.emit(new Values("hulaFirstDevice", message.toString()));
	}

	@Override
	public void declareOutputFields(OutputFieldsDeclarer declarer) {
		declarer.declare(new Fields("deviceId","message"));
	}

}
