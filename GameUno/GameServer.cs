using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace GameUno
{
    public class GameServer
    {
        private const int Port = 5000;
        private TcpListener? listener;
        private bool isRunning;
        private readonly List<TcpClient> clients = new();

        public void Start()
        {
            listener = new TcpListener(IPAddress.Any, Port);
            listener.Start();
            isRunning = true;

            Console.WriteLine($"Game server started on port {Port}...");

            while (isRunning)
            {
                var client = listener.AcceptTcpClient();
                lock (clients)
                {
                    clients.Add(client);
                }
                var thread = new Thread(() => HandleClient(client));
                thread.Start();
            }
        }

        public void Stop()
        {
            isRunning = false;
            listener?.Stop();
            lock (clients)
            {
                foreach (var client in clients)
                {
                    client.Close();
                }
                clients.Clear();
            }
            Console.WriteLine("Game server stopped.");
        }

        private void HandleClient(TcpClient client)
        {
            using var stream = client.GetStream();
            var buffer = new byte[1024];
            int bytesRead;

            try
            {
                while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) != 0)
                {
                    var request = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim();
                    Console.WriteLine($"Received request: {request}");

                    var response = ProcessRequest(request);
                    BroadcastToClients(response);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Client connection error: {ex.Message}");
            }
            finally
            {
                lock (clients)
                {
                    clients.Remove(client);
                }
                client.Close();
            }
        }

        private string ProcessRequest(string request)
        {
            try
            {
                var parts = request.Split(' ');
                var command = parts[0].ToLower();

                switch (command)
                {
                    case "start":
                        Game.RunGame();
                        return "Game started.";

                    case "stop":
                        Game.StopGame();
                        return "Game stopped.";

                    case "status":
                        return $"Run: {Game.Run}, StepOrder: {Game.StepOrder}, Direction: {Game.Direction}, Round: {Game.Round}";

                    case "nextstep":
                        Game.NextStep();
                        return $"Next step executed. StepOrder: {Game.StepOrder}";

                    case "prevstep":
                        Game.PrevStep();
                        return $"Previous step executed. StepOrder: {Game.StepOrder}";

                    case "reversedirection":
                        Game.ReverseDirection();
                        return $"Direction reversed. Direction: {Game.Direction}";

                    case "clear":
                        Game.Clear();
                        return "Game cleared.";

                    default:
                        return "Unknown command.";
                }
            }
            catch (Exception ex)
            {
                return $"Error processing request: {ex.Message}";
            }
        }

        private void BroadcastToClients(string message)
        {
            lock (clients)
            {
                foreach (var client in clients)
                {
                    try
                    {
                        var stream = client.GetStream();
                        var responseBytes = Encoding.UTF8.GetBytes(message + "\n");
                        stream.Write(responseBytes, 0, responseBytes.Length);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error broadcasting to client: {ex.Message}");
                    }
                }
            }
        }
    }
}
