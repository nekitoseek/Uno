using System.Net.Sockets;
using System.Text;

namespace GameUno
{
    public class GameClient
    {
        private readonly string serverAddress;
        private readonly int port;

        public GameClient(string serverAddress, int port = 5000)
        {
            this.serverAddress = serverAddress;
            this.port = port;
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

        public void ListenForUpdates()
        {
            try
            {
                using var client = new TcpClient(serverAddress, port);
                using var stream = client.GetStream();
                var buffer = new byte[1024];

                while (true)
                {
                    var bytesRead = stream.Read(buffer, 0, buffer.Length);
                    var message = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim();
                    Console.WriteLine($"Update from server: {message}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error receiving updates: {ex.Message}");
            }
        }
    }
}
