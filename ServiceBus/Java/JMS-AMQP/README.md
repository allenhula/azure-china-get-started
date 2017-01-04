# JMS API with Azure China Service Bus and AMQP 1.0 Sample

Sample queue-jms is based on SDK qpid-jms-client: 
- Introduction: https://qpid.apache.org/components/jms/
- Jar:https://mvnrepository.com/artifact/org.apache.qpid/qpid-jms-client

Sample queue-jms-old is based on SDK qpid-amqp-1-0-client-jms:
- https://mvnrepository.com/artifact/org.apache.qpid/qpid-amqp-1-0-client
- https://mvnrepository.com/artifact/org.apache.qpid/qpid-amqp-1-0-client-jms
- https://mvnrepository.com/artifact/org.apache.qpid/qpid-amqp-1-0-common
- https://mvnrepository.com/artifact/org.apache.geronimo.specs/geronimo-jms_1.1_spec

As JMS library author said (http://qpid.2158936.n2.nabble.com/What-Qpid-AMQP-1-0-client-to-use-td7635443.html), qpid-amqp-1-0-client-jms will not be maintained, and qpid-jms-client is the one they are working on. So suggested to use latest version of qpid-jms-client.