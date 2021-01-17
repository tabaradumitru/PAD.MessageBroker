using System;
using System.IO;
using System.Xml.Serialization;

namespace Data
{
    public class MessageData
    {
        public User Sender { get; set; }
        public int ReceiverId { get; set; }
        public string Message { get; set; }
        public DateTime Time { get; set; } 
        
        public static string SerializeMessage(MessageData messageData)
        {
            var serializer = new XmlSerializer(typeof (MessageData));

            using var stream = new MemoryStream();
            
            serializer.Serialize(stream, messageData);
            stream.Position = 0;
            var sr = new StreamReader(stream);
            var serializedData = sr.ReadToEnd();

            return serializedData;
        }
    }
}