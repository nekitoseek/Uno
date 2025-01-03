using System.Net.Sockets;
using System.Text;

namespace GameUnoClient
{
    public class GameClient
    {
        private readonly string serverAddress;
        private readonly int port;

        public GameClient(string serverAddress, int port)
        {
            this.serverAddress = serverAddress;
            this.port = 5000;
        }

        public string SendCommand(string command)
        {
            try
            {
                using var client = new TcpClient(serverAddress, port);
                using var stream = client.GetStream();

                var commandBytes = Encoding.UTF8.GetBytes(command + "\n");
                stream.Write(commandBytes, 0, commandBytes.Length);

                var buffer = new byte[1024];
                var bytesRead = stream.Read(buffer, 0, buffer.Length);

                return Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim();
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }
    }
}
