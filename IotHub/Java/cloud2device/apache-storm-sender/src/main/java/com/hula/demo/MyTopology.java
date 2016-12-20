package com.hula.demo;

import backtype.storm.Config;
import backtype.storm.LocalCluster;
import backtype.storm.StormSubmitter;
import backtype.storm.topology.TopologyBuilder;
import backtype.storm.tuple.Fields;

import com.hula.demo.*;

public class MyTopology {

  //Entry point for the topology
  public static void main(String[] args) throws Exception {
	  
	  //Used to build the topology
    TopologyBuilder builder = new TopologyBuilder();
    builder.setSpout("devicespout", new DeviceSpout(), 1);
    // iothub connection string
    String connectionString = "HostName={iothubname}.azure-devices.cn;SharedAccessKeyName={};SharedAccessKey={}";
    builder.setBolt("cloudtodevicebolt", new Cloud2DeviceBolt(connectionString), 1).shuffleGrouping("devicespout");

    //new configuration
    Config conf = new Config();
    //Set to false to disable debug information
    // when running in production mode.
    conf.setDebug(true);
    conf.setMessageTimeoutSecs(60);

    //If there are arguments, we are running on a cluster
    if (args != null && args.length > 0) {
      //parallelism hint to set the number of workers
      conf.setNumWorkers(3);
      //submit the topology
      StormSubmitter.submitTopology(args[0], conf, builder.createTopology());
    }
    //Otherwise, we are running locally
    else {
      //Cap the maximum number of executors that can be spawned
      //for a component to 3
      conf.setMaxTaskParallelism(1);
      //LocalCluster is used to run locally
      LocalCluster cluster = new LocalCluster();
      //submit the topology
      cluster.submitTopology("cloud-to-device", conf, builder.createTopology());
      //cluster.submitTopology("word-count", conf, builder.createTopology());
      //the sleep time will decide how long the process running as the cluster will shutdown later after the sleep
      Thread.sleep(100000);
      //shut down the cluster
      cluster.killTopology("cloud-to-device");
      cluster.shutdown();
    }
  }
}