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

            // ������� IP-����� ������� � ���� (�� ��������� 5000).
            var client = new GameClient("192.168.0.166", 5000); // �������� "192.168.1.100" �� IP-����� �������

            // ������ �������� �������
            var response = client.SendCommand("start");
            Console.WriteLine($"Server response: {response}");

            // �������� ������� ���������� �� �������
            client.ListenForUpdates();
        }
    }
}