
using System.Xml.Linq;

namespace GameUno
{
    public static class Helper
    {
        class Content
        {
            public int Id { get; set; }
            public string? Name { get; set; }
        }

        private static readonly string HandsXmlPath = "Hands.xml";
        private static readonly string PurchaseXmlPath = "Purchase.xml";
        private static readonly string DropXmlPath = "Drop.xml";

        public static List<Card> GetHandsCards(int stepOrder)
        {
            var list = new List<Card>();
            if (File.Exists(HandsXmlPath))
            {
                var doc = XDocument.Load(HandsXmlPath);
                var cards = doc.Root?.Elements("Card")
                    .Where(x => (int)x.Element("Order") == stepOrder);

                foreach (var card in cards!)
                {
                    var id = (int)card.Element("Id");
                    var name = (string)card.Element("Name");
                    CreateAndAddCardToList(list, id, name);
                }
            }
            list.Sort(ComparisonCardsFunc);
            return list;
        }

        private static int ComparisonCardsFunc(Card x, Card y)
        {
            if (x.Color == y.Color)
            {
                if (x.Cost == y.Cost) return 0;
                return x.Cost > y.Cost ? 1 : -1;
            }
            return x.Color > y.Color ? 1 : -1;
        }

        public static Card? CreateACard(int id, string? name)
        {
            var list = new List<Card>();
            CreateAndAddCardToList(list, id, name);
            return list.Count == 1 ? list[0] : null;
        }

        private static void CreateAndAddCardToList(List<Card> list, int id, string? name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("Card name cannot be null or empty.", nameof(name));
            }
            var card = new Card(id, name);
            var n = 0;
            foreach (var colorName in Enum.GetNames(typeof(CardColor)))
            {
                if (name.StartsWith(colorName))
                {
                    card.Color = (CardColor)n;
                    var funcName = name.Substring(colorName.Length).Trim('(', ')');
                    if (int.TryParse(funcName, out int number))
                    {
                        CardBuilder.BuildNumericCard(card, number);
                        list.Add(card);
                    }
                    else
                    {
                        switch (funcName)
                        {
                            case "ActiveRotate":
                                CardBuilder.BuildRotateCard(card);
                                list.Add(card);
                                break;
                            case "ActiveSkip":
                                CardBuilder.BuildSkipCard(card);
                                list.Add(card);
                                break;
                            case "ActiveTakeTwo":
                                CardBuilder.BuildTakeTwoCard(card);
                                list.Add(card);
                                break;
                            case "WildColor":
                                CardBuilder.BuildWildColorCard(card);
                                list.Add(card);
                                break;
                            case "WildColor, TakeFour":
                                CardBuilder.BuildWildColorTakeFourCard(card);
                                list.Add(card);
                                break;
                        }
                    }
                }
                n++;
            }
        }

        public static Card? GetPurchaseCardToHands(int stepOrder, int count = 1)
        {
            Card? card = null;
            if (File.Exists(PurchaseXmlPath))
            {
                var purchaseDoc = XDocument.Load(PurchaseXmlPath);
                var handDoc = File.Exists(HandsXmlPath) ? XDocument.Load(HandsXmlPath) : new XDocument(new XElement("Hands"));

                var cards = purchaseDoc.Root?.Elements("Card").Take(count).ToList();

                foreach (var cardElement in cards!)
                {
                    var id = (int)cardElement.Element("Id");
                    var name = (string)cardElement.Element("Name");

                    handDoc.Root?.Add(new XElement("Card",
                        new XElement("Order", stepOrder),
                        new XElement("Id", id),
                        new XElement("Name", name)));

                    cardElement.Remove();
                    card = CreateACard(id, name);
                }

                handDoc.Save(HandsXmlPath);
                purchaseDoc.Save(PurchaseXmlPath);
            }
            return card;
        }

        private static void RebuildPurchase()
        {
            if (File.Exists(DropXmlPath))
            {
                var dropDoc = XDocument.Load(DropXmlPath);
                var purchaseDoc = File.Exists(PurchaseXmlPath) ? XDocument.Load(PurchaseXmlPath) : new XDocument(new XElement("Purchase"));

                foreach (var card in dropDoc.Root?.Elements("Card")!)
                {
                    purchaseDoc.Root?.Add(card);
                }

                dropDoc.Root?.RemoveAll();
                dropDoc.Save(DropXmlPath);
                purchaseDoc.Save(PurchaseXmlPath);
            }
        }

        public static void GetPurchaseCardToDropFirstCard()
        {
            if (File.Exists(PurchaseXmlPath))
            {
                var purchaseDoc = XDocument.Load(PurchaseXmlPath);
                var dropDoc = File.Exists(DropXmlPath) ? XDocument.Load(DropXmlPath) : new XDocument(new XElement("Drop"));

                var firstCard = purchaseDoc.Root?.Elements("Card").FirstOrDefault();

                if (firstCard != null)
                {
                    dropDoc.Root?.Add(firstCard);
                    firstCard.Remove();
                }

                purchaseDoc.Save(PurchaseXmlPath);
                dropDoc.Save(DropXmlPath);
            }
        }

        public static void EmptyHands()
        {
            if (File.Exists(HandsXmlPath))
            {
                var doc = XDocument.Load(HandsXmlPath);
                doc.Root?.RemoveAll();
                doc.Save(HandsXmlPath);
            }
        }

        public static Dictionary<string, Image> GetResourceImages()
        {
            // Логика не изменяется, т.к. она не связана с базой данных
            var dict = new Dictionary<string, Image>();
            var heights = new int[] { 0, 360, 720, 1080, 1440, 1800, 2160, 2520 };
            var widths = new int[] { 0, 240, 480, 720, 960, 1200, 1440, 1680, 1920, 2160, 2400, 2640, 2880, 3120 };
            var colorNames = Enum.GetNames(typeof(CardColor)).Take(4).ToArray();
            var source = Properties.Resources.Cards;

            using (var graphics = Graphics.FromImage(source))
            {
                var width = 241;
                var height = 360;
                for (var i = 0; i < 8; i++)
                {
                    for (var j = 0; j < 14; j++)
                    {
                        var rect = new Rectangle(widths[j], heights[i], width, height);
                        var image = new Bitmap(width, height);
                        using (var g = Graphics.FromImage(image))
                        {
                            g.FillRectangle(Brushes.Green, rect);
                            g.DrawImage(source, 0, 0, rect, GraphicsUnit.Pixel);
                        }
                        image.MakeTransparent(Color.Green);
                        if (i > 3 && j == 0) continue;
                        string key = string.Empty;
                        var colorName = j == 13 ? $"Black" : $"{colorNames[i % 4]}";
                        switch (j)
                        {
                            case 10:
                                key = $"{colorName}(ActiveSkip)";
                                break;
                            case 11:
                                key = $"{colorName}(ActiveRotate)";
                                break;
                            case 12:
                                key = $"{colorName}(ActiveTakeTwo)";
                                break;
                            case 13:
                                key = i < 4 ? "Black(WildColor)" : "Black(WildColor, TakeFour)";
                                break;
                            default:
                                if (j <= 9) key = $"{colorName}({j})";
                                break;
                        }
                        if (!string.IsNullOrEmpty(key) && !dict.ContainsKey(key))
                        {
                            dict.Add(key, image);
                            if (key == "Black(WildColor)")
                            {
                                foreach (var cn in colorNames)
                                    dict.Add($"{cn}(WildColor)", image);
                            }
                            if (key == "Black(WildColor, TakeFour)")
                            {
                                foreach (var cn in colorNames)
                                    dict.Add($"{cn}(WildColor, TakeFour)", image);
                            }
                        }
                    }
                }
            }
            return dict;
        }

        public static int CalculateWinScore(int winOrder)
        {
            var score = 0;
            for (int i = 1; i <= Game.PlaceCount; i++)
            {
                if (i == winOrder) continue;
                score += GetHandsCards(i).Sum(card => card.Cost);
            }
            return score;
        }
    }
}
