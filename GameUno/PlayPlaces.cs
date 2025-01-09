using System.Xml.Linq;

namespace GameUno
{
    public static class PlayPlaces
    {
        private static readonly string filePath = "Players.xml";
        private static readonly int maxRange = Game.PlaceCount;
        public static int Order { get; private set; }

        static PlayPlaces()
        {
            if (!File.Exists(filePath))
            {
                Clear(); // Инициализируем файл, если его нет
            }
        }

        public static void DefinePlaceOrder(string playerName)
        {
            var document = XDocument.Load(filePath);
            var players = document.Root?.Elements("Player");

            foreach (var player in players!)
            {
                if (string.IsNullOrEmpty(player.Element("Name")?.Value))
                {
                    player.Element("Name")!.Value = playerName;
                    Order = int.Parse(player.Element("Order")!.Value);
                    document.Save(filePath);
                    return;
                }
            }
            Order = 0;
        }

        public static void Clear()
        {
            var document = new XDocument(new XElement("Players"));
            for (int i = 1; i <= maxRange; i++)
            {
                document.Root?.Add(new XElement("Player",
                    new XElement("Order", i),
                    new XElement("Name", string.Empty),
                    new XElement("CountCards", 0),
                    new XElement("PlayScore", 0)));
            }
            document.Save(filePath);
        }

        public static void ClearScores()
        {
            var document = XDocument.Load(filePath);
            foreach (var player in document.Root!.Elements("Player"))
            {
                player.Element("PlayScore")!.Value = "0";
            }
            document.Save(filePath);
        }

        public static void Update()
        {
            var document = XDocument.Load(filePath);
            foreach (var player in document.Root!.Elements("Player"))
            {
                var order = int.Parse(player.Element("Order")!.Value);
                var name = player.Element("Name")!.Value;
                var cards = int.Parse(player.Element("CountCards")!.Value);
                var score = int.Parse(player.Element("PlayScore")!.Value);

                SetPlayer(order, name, cards, score);
            }
        }

        public static event PlayerEventHandler? PlayerChanged;

        private static void SetPlayer(int order, string name, int cards, int score)
        {
            PlayerChanged?.Invoke(new object(),
                new PlayerEventArgs(order, name, cards, score));
        }

        public static void SetPlayerName(int stepOrder, string name)
        {
            var document = XDocument.Load(filePath);
            var player = document.Root?.Elements("Player").FirstOrDefault(p =>
                int.Parse(p.Element("Order")!.Value) == stepOrder);

            if (player != null)
            {
                player.Element("Name")!.Value = name;
                document.Save(filePath);
            }
        }

        public static string? GetPlayerName(int stepOrder)
        {
            var document = XDocument.Load(filePath);
            var player = document.Root?.Elements("Player").FirstOrDefault(p =>
                int.Parse(p.Element("Order")!.Value) == stepOrder);

            var name = player?.Element("Name")?.Value;
            return string.IsNullOrWhiteSpace(name) ? $"Бот {stepOrder}" : name;
        }

        public static void ClearPlayerPlace(int stepOrder)
        {
            var document = XDocument.Load(filePath);
            var player = document.Root?.Elements("Player").FirstOrDefault(p =>
                int.Parse(p.Element("Order")!.Value) == stepOrder);

            if (player != null)
            {
                player.Element("Name")!.Value = string.Empty;
                player.Element("CountCards")!.Value = "0";
                player.Element("PlayScore")!.Value = "0";
                document.Save(filePath);
            }
        }

        public static void AddOrSubCountCards(int stepOrder, int addOrSub)
        {
            var document = XDocument.Load(filePath);
            var player = document.Root?.Elements("Player").FirstOrDefault(p =>
                int.Parse(p.Element("Order")!.Value) == stepOrder);

            if (player != null)
            {
                var count = int.Parse(player.Element("CountCards")!.Value);
                player.Element("CountCards")!.Value = (count + addOrSub).ToString();
                document.Save(filePath);
            }
        }

        public static int GetCountCards(int stepOrder)
        {
            var document = XDocument.Load(filePath);
            var player = document.Root?.Elements("Player").FirstOrDefault(p =>
                int.Parse(p.Element("Order")!.Value) == stepOrder);

            return player != null ? int.Parse(player.Element("CountCards")!.Value) : 0;
        }

        public static void SetCountCards(int stepOrder, int count)
        {
            var document = XDocument.Load(filePath);
            var player = document.Root?.Elements("Player").FirstOrDefault(p =>
                int.Parse(p.Element("Order")!.Value) == stepOrder);

            if (player != null)
            {
                player.Element("CountCards")!.Value = count.ToString();
                document.Save(filePath);
            }
        }

        public static void SetPlayScore(int stepOrder, int score)
        {
            var document = XDocument.Load(filePath);
            var player = document.Root?.Elements("Player").FirstOrDefault(p =>
                int.Parse(p.Element("Order")!.Value) == stepOrder);

            if (player != null)
            {
                player.Element("PlayScore")!.Value = score.ToString();
                document.Save(filePath);
            }
        }

        public static int GetPlayScore(int stepOrder)
        {
            var document = XDocument.Load(filePath);
            var player = document.Root?.Elements("Player").FirstOrDefault(p =>
                int.Parse(p.Element("Order")!.Value) == stepOrder);

            return player != null ? int.Parse(player.Element("PlayScore")!.Value) : 0;
        }

        public static int AddPlayScore(int stepOrder, int score)
        {
            var document = XDocument.Load(filePath);
            var player = document.Root?.Elements("Player").FirstOrDefault(p =>
                int.Parse(p.Element("Order")!.Value) == stepOrder);

            if (player != null)
            {
                var currentScore = int.Parse(player.Element("PlayScore")!.Value);
                var newScore = currentScore + score;
                player.Element("PlayScore")!.Value = newScore.ToString();
                document.Save(filePath);
                return newScore;
            }
            return 0;
        }

        public static void ClearPlace(int stepOrder)
        {
            ClearPlayerPlace(stepOrder);
        }
    }

    public delegate void PlayerEventHandler(object sender, PlayerEventArgs args);

    public class PlayerEventArgs : EventArgs
    {
        public PlayerEventArgs(int stepOrder, string name, int countCards, int playScore)
        {
            StepOrder = stepOrder;
            Name = name;
            CountCards = countCards;
            PlayScore = playScore;
        }

        public int StepOrder { get; }
        public string Name { get; }
        public int CountCards { get; }
        public int PlayScore { get; }
    }
}
