using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Data;

namespace Services.Implementation
{
    public class BrokerService: IService
    {
        private readonly ConcurrentQueue<string> _messageQueue = new ConcurrentQueue<string>();
        private readonly HashSet<Receiver> _receivers = new HashSet<Receiver>();
        private readonly UdpClient _udpClient;
        private readonly ConcurrentQueue<string> _unreceivedList = new ConcurrentQueue<string>();

        public BrokerService()
        {
            _udpClient = new UdpClient(44444);
        }

        public async Task<string> AsyncRead()
        {
            var receivedDatagram = await _udpClient.ReceiveAsync();
            
            var decodedString = Encoding.ASCII.GetString(receivedDatagram.Buffer, 0, receivedDatagram.Buffer.Length);

            DeserializeXMLMessage(decodedString, out var messageData);
            
            if (messageData.Message.Equals("-"))
            {
                AddReceiver(receivedDatagram, messageData.Sender);
                return "";
            }

            _messageQueue.Enqueue(decodedString);

            return decodedString;
        }
        
        public async Task AsyncWrite(string message)
        {
            var mess = message;

            if (message.Length > 0)
            {
                DeserializeXMLMessage(message, out var messageData);

                var bytes = Encoding.ASCII.GetBytes(messageData.Sender.Name + ": " + messageData.Message);
                var receiver = _receivers.ToList().Find(r => r.User.Id == messageData.ReceiverId);

                if (receiver != null)
                {
                    await _udpClient.SendAsync(bytes, bytes.Length, receiver.IpEndPoint);
                    _messageQueue.TryDequeue(out message);
                }
                else
                {
                    _unreceivedList.Enqueue(mess);
                    _messageQueue.TryDequeue(out message);
                }
            }
        }
        
        public async Task AsyncReload()
        {
            foreach (var unsentMessage in _unreceivedList)
            {
                DeserializeXMLMessage(unsentMessage, out var messageData);
                
                var bytes = Encoding.ASCII.GetBytes(messageData.Sender.Name + ": " + messageData.Message);

                var receiver = _receivers.ToList().Find(r => r.User.Id == messageData.ReceiverId);

                if (receiver != null)
                {
                    await _udpClient.SendAsync(bytes, bytes.Length, receiver.IpEndPoint);
                    var mess1 = unsentMessage;
                    _unreceivedList.TryDequeue(out mess1);
                    Console.WriteLine("Resending was done successfully!");
                }
                else
                {
                    Console.WriteLine("Failed to send the message because the receiver could not be found!");
                }
            }
        }

        private void AddReceiver(UdpReceiveResult rec, User user)
        {
            _receivers.Add(new Receiver
            {
                User = new User
                {
                    Name = user.Name,
                    Id = user.Id
                },
                IpEndPoint = rec.RemoteEndPoint
            });
        }
        
        private static void DeserializeXMLMessage(string message, out MessageData messageData)
        {
            var formatter = new XmlSerializer(typeof(MessageData));
            
            using (var stream = new MemoryStream())
            {
                var writer = new StreamWriter(stream);
                writer.Write(message);
                writer.Flush();
                stream.Position = 0;
        
                messageData = (MessageData) formatter.Deserialize(stream);
            }
        }
    }
}