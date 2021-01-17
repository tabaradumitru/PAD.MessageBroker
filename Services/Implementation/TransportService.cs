using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Services.Implementation
{
    public class TransportService : IService
    {
        private readonly UdpClient _udpClient = new UdpClient();

        public TransportService(IPEndPoint broker)
        {
            _udpClient.Connect(broker);
        }

        public async Task<string> AsyncRead()
        {
            var receivedDatagram = await _udpClient.ReceiveAsync();
            
            return Encoding.ASCII.GetString(receivedDatagram.Buffer, 0, receivedDatagram.Buffer.Length);
        }

        public async Task AsyncWrite(string message)
        {
            var bytes = Encoding.ASCII.GetBytes(message);
            
            await _udpClient.SendAsync(bytes, bytes.Length);
        }

        public async Task AsyncReload() { }
    }
}