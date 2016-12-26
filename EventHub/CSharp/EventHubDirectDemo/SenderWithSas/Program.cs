using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using System;
using System.Globalization;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Web;


namespace SenderWithSas
{
    class Program
    {
        static string namespaceName = "";
        static string eventHubName = "";
        static string publisherName = "";
        static string keyName = "";
        static string keyValue = "";

        static void Main(string[] args)
        {
            Console.WriteLine("Press Ctrl-C to stop the sender process");
            Console.WriteLine("Press Enter to start now");
            Console.ReadLine();

            var endpoint = string.Format("sb://{0}.servicebus.chinacloudapi.cn/", namespaceName);
            var resourceUri = string.Format("{0}{1}", endpoint, eventHubName);
            var resourceUri4Publisher = string.Format("{0}{1}/publishers/{2}", endpoint, eventHubName, publisherName);

            // common sas
            //var sas = createToken(resourceUri, keyName, keyValue);
            // publisher specific sas. created in native way
            var sas4Publisher = createToken(resourceUri4Publisher, keyName, keyValue);
            // publisher speicifc sas. created from library
            //var generatedSas4Publisher = GetSasPerPublisher(endpoint, keyName, keyValue, publisherName);

            SendMessagesWithSas(endpoint, eventHubName, publisherName, sas4Publisher);

            var httpEndpoint = string.Format("https://{0}.servicebus.chinacloudapi.cn/", namespaceName);
            var httpResourceUri = string.Format("{0}{1}", httpEndpoint, eventHubName);
            var httpResourceUri4Publisher = string.Format("{0}{1}/publishers/{2}", httpEndpoint, eventHubName, publisherName);

            // common sas
            //var httpSas = createToken(httpResourceUri, keyName, keyValue);
            // publisher specific sas. created in native way
            var httpSas4Publisher = createToken(httpResourceUri4Publisher, keyName, keyValue);
            // publisher speicifc sas. created from library
            //var httpGeneratedSas4Publisher = GetSasPerPublisher(httpEndpoint, keyName, keyValue, publisherName);

            SendMessagesWithSasViaHttp(httpEndpoint, eventHubName, publisherName, httpSas4Publisher);
        }

        static void SendMessagesWithSas(string endpoint, string eventHubName, string publisherName, string sas)
        {
            var msgFactory = MessagingFactory.Create(new Uri(endpoint), new MessagingFactorySettings()
            {
                TokenProvider = TokenProvider.CreateSharedAccessSignatureTokenProvider(sas),
                TransportType = TransportType.Amqp
            });

            // below are two working approach to send messages with publisher sas
            var sender = msgFactory.CreateEventHubClient(eventHubName).CreateSender(publisherName);
            //var publisherClient = msgFactory.CreateEventHubClient(string.Format("{0}/publishers/{1}", eventHubName, publisherName));

            // if using below client to send message with publisher sas, it will fail with error "invalid signature"
            // that's because the generated token is specified for particular publisher, not common client
            //var client = msgFactory.CreateEventHubClient(eventHubName);

            for (int i = 0; i < 8; i++)
            {
                try
                {
                    var message = Guid.NewGuid().ToString();
                    Console.WriteLine("{0} > Sending message with SAS : {1}", DateTime.Now, message);
                    var eventData = new EventData(Encoding.UTF8.GetBytes(message));
                    // bug here: 
                    eventData.PartitionKey = "device" + i;
                    sender.Send(eventData);
                    //publisherClient.Send(eventData);
                    //client.Send(eventData);
                }
                catch (Exception exception)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("{0} > Exception: {1}", DateTime.Now, exception.Message);
                    Console.ResetColor();
                }

                Thread.Sleep(200);
            }
        }

        static void SendMessagesWithSasViaHttp(string endpoint, string eventHubName, string publisherName, string sas)
        {
            var message = Guid.NewGuid().ToString();
            Console.WriteLine("{0} > Sending message with SAS via HTTPS : {1}", DateTime.Now, message);
            var eventData = new EventData(Encoding.UTF8.GetBytes(message));

            var httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(endpoint);
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", sas);

            try
            {
                var requestUriString = string.Format("{0}/publishers/{1}/messages", eventHubName, publisherName);
                var postResult = httpClient.PostAsJsonAsync(requestUriString, eventData).Result;
                Console.WriteLine("{0} > Response status: {1}", DateTime.Now, postResult.StatusCode.ToString());
                //Console.WriteLine("{0} > Response content: {1}", DateTime.Now, postResult.Content.ReadAsStringAsync().Result);
            }
            catch (Exception exception)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("{0} > Exception: {1}", DateTime.Now, exception.Message);
                Console.ResetColor();
            }
        }

        static string GetSasPerPublisher(string endpoint, string keyName, string key, string publisherName)
        {
            return SharedAccessSignatureTokenProvider.GetPublisherSharedAccessSignature(
                new Uri(endpoint),
                eventHubName,
                publisherName,
                keyName,
                keyValue,
                TimeSpan.FromTicks(DateTime.UtcNow.AddDays(7).Ticks)
                );
        }

        static string createToken(string resourceUri, string keyName, string key)
        {
            TimeSpan sinceEpoch = DateTime.UtcNow - new DateTime(1970, 1, 1);
            var week = 60 * 60 * 24 * 7;
            var expiry = Convert.ToString((int)sinceEpoch.TotalSeconds + week);
            string stringToSign = HttpUtility.UrlEncode(resourceUri) + "\n" + expiry;
            HMACSHA256 hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key));
            var signature = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(stringToSign)));
            var sasToken = String.Format(CultureInfo.InvariantCulture, "SharedAccessSignature sr={0}&sig={1}&se={2}&skn={3}", HttpUtility.UrlEncode(resourceUri), HttpUtility.UrlEncode(signature), expiry, keyName);
            return sasToken;
        }
    }
}
