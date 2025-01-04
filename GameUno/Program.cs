namespace GameUno
{
    internal static class Program
    {

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());

            // Укажите IP-адрес сервера и порт (по умолчанию 5000).
            var client = new GameClient("192.168.0.166", 5000); // Замените "192.168.1.100" на IP-адрес сервера

            // Пример отправки команды
            var response = client.SendCommand("start");
            Console.WriteLine($"Server response: {response}");

            // Начинаем слушать обновления от сервера
            client.ListenForUpdates();
        }
    }
}