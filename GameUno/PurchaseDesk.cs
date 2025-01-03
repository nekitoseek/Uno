
using System.Xml.Linq;

namespace GameUno
{
    public static class PurchaseDesk
    {
        private const string DeskFilePath = "Desk.xml";
        private const string PurchaseFilePath = "Purchase.xml";

        static PurchaseDesk()
        {
            if (!File.Exists(DeskFilePath))
            {
                InitializeDeskXml();
            }

            if (!File.Exists(PurchaseFilePath))
            {
                InitializePurchaseXml();
            }
        }

        private static void InitializeDeskXml()
        {
            var colors = new[] { "Red", "Yellow", "Green", "Blue" };
            var specialCards = new[] { "ActiveSkip", "ActiveRotate", "ActiveTakeTwo" };
            var blackCards = new[] { "WildColor", "WildColor, TakeFour" };

            var desk = new XElement("Desk");
            int id = 0;

            foreach (var color in colors)
            {
                // Добавляем цифровые карты (0–9)
                for (int i = 0; i <= 9; i++)
                {
                    desk.Add(new XElement("Card",
                        new XElement("Id", id++),
                        new XElement("Name", $"{color}({i})")));
                }

                // Добавляем специальные карты (ActiveSkip, ActiveRotate, ActiveTakeTwo)
                foreach (var special in specialCards)
                {
                    desk.Add(new XElement("Card",
                        new XElement("Id", id++),
                        new XElement("Name", $"{color}({special})")));
                }

                // Добавляем чёрные карты (WildColor)
                desk.Add(new XElement("Card",
                    new XElement("Id", id++),
                    new XElement("Name", "Black(WildColor)")));
            }

            // Добавляем повторные карты (1–9 и специальные карты для всех цветов)
            foreach (var color in colors)
            {
                for (int i = 1; i <= 9; i++)
                {
                    desk.Add(new XElement("Card",
                        new XElement("Id", id++),
                        new XElement("Name", $"{color}({i})")));
                }

                foreach (var special in specialCards)
                {
                    desk.Add(new XElement("Card",
                        new XElement("Id", id++),
                        new XElement("Name", $"{color}({special})")));
                }

                // Добавляем чёрные карты (WildColor, TakeFour)
                desk.Add(new XElement("Card",
                    new XElement("Id", id++),
                    new XElement("Name", "Black(WildColor, TakeFour)")));
            }

            desk.Save(DeskFilePath);
        }


        private static void InitializePurchaseXml()
        {
            var purchase = new XElement("Purchase");
            purchase.Save(PurchaseFilePath);
        }

        public static int CardsCount()
        {
            var xml = XElement.Load(PurchaseFilePath);
            return xml.Elements("Card").Count();
        }

        public static void DropThisCardId(int id)
        {
            var purchase = XElement.Load(PurchaseFilePath);
            var card = purchase.Elements("Card").FirstOrDefault(c => (int)c.Element("Id") == id);
            if (card != null)
            {
                card.Remove();
                purchase.Save(PurchaseFilePath);
            }
        }

        public static void ReshuffleCards()
        {
            var desk = XElement.Load(DeskFilePath);
            var cards = desk.Elements("Card").ToList();

            var random = new Random();
            var shuffledCards = cards.OrderBy(_ => random.Next()).ToList();

            var purchase = new XElement("Purchase");
            foreach (var card in shuffledCards)
            {
                var id = (int)card.Element("Id");
                var name = (string)card.Element("Name");
                purchase.Add(new XElement("Card",
                    new XElement("Id", id),
                    new XElement("Name", name)));
            }

            purchase.Save(PurchaseFilePath);
        }
    }
}
