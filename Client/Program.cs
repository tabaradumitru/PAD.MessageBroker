using System;
using System.Net;
using System.Threading.Tasks;
using Data;
using Services;
using Services.Implementation;

namespace Client
{
    class Program
    {
        static void Main(string[] args)
        {
            var ipEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 44444);
            
            IService client = new TransportService(ipEndPoint);

            var user = new User();

            AuthenticateUser(user);
            
            // subscribe for message receiving
            Task receiverTask = Task.Factory.StartNew(async () =>
            {
                var message = BuildSubscriberMessage(user);
                
                await client.AsyncWrite(message);

                string m;
                
                while ((m = await client.AsyncRead()) != "quit")
                {
                    Console.WriteLine("\nNew Message Received: \n\t" + m);
                }
            });
            
            // start the message sending process
            Task senderTask = Task.Factory.StartNew(() =>
            {
                BuildMessage(user, out var messageData);
                
                while (messageData.Message != "quit")
                {
                    var serializedMessage = MessageData.SerializeMessage(messageData);
                    
                    client.AsyncWrite(serializedMessage);
                    BuildMessage(user, out messageData);
                }
            });

            receiverTask.Wait();
            senderTask.Wait();
        }

        private static void AuthenticateUser(User user)
        {
            Console.Write("Your Name: ");
            user.Name = Console.ReadLine();
            
            Console.Write("Your ID: ");
            user.Id = int.Parse(Console.ReadLine());
        }
        
        private static void BuildMessage(User sender, out MessageData messageData)
        {
            Console.Write("Receiver ID: ");
            var receiverId = int.Parse(Console.ReadLine());

            Console.WriteLine("Message: ");
            var message = Console.ReadLine();
            
            messageData = new MessageData
            {
                Time = new DateTime(),
                Sender = sender,
                ReceiverId = receiverId,
                Message = message
            };
        }
        
        private static string BuildSubscriberMessage(User user) => MessageData.SerializeMessage(new MessageData
        {
            Sender = user,
            Message = "-"
        });
    }
}