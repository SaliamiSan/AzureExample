using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Messaging
{
    class Program
    {
        static void Main(string[] args)
        {
            var connectionString = "Endpoint=sb://saliamisan.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=RbCrTk/twBwfD+mR0Sz9hucHzcXuvN8oM3r8YadlP6Q=";
            var client = QueueClient.CreateFromConnectionString(connectionString, "tasks");
            var message = new BrokeredMessage("This is a test message!");
            client.Send(message);

            client.OnMessage(m =>
            {
                Console.WriteLine(String.Format("Message body: {0}", m.GetBody<String>()));
                Console.WriteLine(String.Format("Message id: {0}", m.MessageId));
            });

            var namespaceManager =
    NamespaceManager.CreateFromConnectionString(connectionString);
            TopicDescription td = new TopicDescription("TestTopic");
            td.MaxSizeInMegabytes = 5120;
            td.DefaultMessageTimeToLive = new TimeSpan(0, 1, 0);

            if (!namespaceManager.TopicExists("TestTopic"))
            {
                namespaceManager.CreateTopic(td);
            }
            if (!namespaceManager.SubscriptionExists("TestTopic", "AllMessages"))
            {
                namespaceManager.CreateSubscription("TestTopic", "AllMessages");
            }

            // Create a "LowMessages" filtered subscription.
            SqlFilter lowMessagesFilter =
               new SqlFilter("MessageNumber <= 3");
            SqlFilter highMessagesFilter =
   new SqlFilter("MessageNumber > 3");

            namespaceManager.CreateSubscription("TestTopic",
               "HighMessages",
               highMessagesFilter);
            //namespaceManager.CreateSubscription("TestTopic",
            //   "LowMessages",
            //   lowMessagesFilter);
            TopicClient topicClient =
    TopicClient.CreateFromConnectionString(connectionString, "TestTopic");
            for (int i = 0; i < 5; i++)
            {
                // Create message, passing a string message for the body.
                BrokeredMessage messageInTopic = new BrokeredMessage("Test message " + i);

                // Set additional custom app-specific property.
                messageInTopic.Properties["MessageNumber"] = i;

                // Send message to the topic.
                topicClient.Send(messageInTopic);
            }

            SubscriptionClient subscriptionClient =
    SubscriptionClient.CreateFromConnectionString
            (connectionString, "TestTopic", "HighMessages");

            // Configure the callback options.
            OnMessageOptions options = new OnMessageOptions();
            options.AutoComplete = false;
            options.AutoRenewTimeout = TimeSpan.FromMinutes(1);

            subscriptionClient.OnMessage((m) =>
            {
                try
                {
                    // Process message from subscription.
                    Console.WriteLine("\n**High Messages**");
                    Console.WriteLine("Body: " + m.GetBody<string>());
                    Console.WriteLine("MessageID: " + m.MessageId);
                    Console.WriteLine("Message Number: " +
                        m.Properties["MessageNumber"]);

                    // Remove message from subscription.
                    m.Complete();
                }
                catch (Exception)
                {
                    // Indicates a problem, unlock message in subscription.
                    m.Abandon();
                }
            }, options);



            Console.ReadLine();
        }
    }
}
