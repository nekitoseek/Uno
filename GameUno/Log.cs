using MySql.Data.MySqlClient;

namespace GameUno
{
    public static class Log
    {
        public static int NumberLastMessage { get; private set; }

        public static string[] SelectLastMessages(int count = 0)
        {
            var list = new List<string>();
            using (var server = new Server())
            {
                if (server.Connected)
                {
                    using (var command = new MySqlCommand(
                        "SELECT `Message` FROM `Log` ORDER BY `Id` DESC" + (count > 0 ? $" LIMIT {count}" : ""),
                        server.Connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                list.Add(reader.GetString(0));
                            }
                        }
                    }
                    list.Reverse();
                }
            }
            return list.ToArray();
        }

        public static void Add(string message = "")
        {
            using (var server = new Server())
            {
                if (server.Connected)
                {
                    using (var command = new MySqlCommand("INSERT INTO `Log` (`Message`) VALUES (@Message)",
                        server.Connection))
                    {
                        command.Parameters.AddWithValue("@Message", message ?? string.Empty);
                        command.ExecuteNonQuery();
                    }
                    using (var command = new MySqlCommand("SELECT `Id` FROM `Log` ORDER BY `Id` DESC LIMIT 1", server.Connection))
                    {
                        var result = command.ExecuteScalar();
                        NumberLastMessage = result != null ? (int)result : 0;
                    }
                }
            }
        }

        public static void Clear()
        {
            using (var server = new Server())
            {
                if (server.Connected)
                {
                    using (var command = new MySqlCommand("DELETE FROM `Log`", server.Connection))
                    {
                        command.ExecuteNonQuery();
                    }
                    NumberLastMessage = 0;
                }
            }
        }
    }
}
