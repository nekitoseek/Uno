
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
            var desk = new XElement("Desk",
                new XElement("Card", new XElement("Id", 0), new XElement("Name", "Red(0)")),
                new XElement("Card", new XElement("Id", 1), new XElement("Name", "Red(1)"))
            // Добавьте остальные карты аналогично
            );
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
