
using System.Xml.Linq;

namespace GameUno
{
    public static class DropDesk
    {
        private const string DropFilePath = "Drop.xml";
        private const string HandsFilePath = "Hands.xml";

        public static Card? TopCard { get; private set; }

        static DropDesk()
        {
            // Инициализация XML-файлов, если они отсутствуют
            if (!File.Exists(DropFilePath))
                new XElement("Drop").Save(DropFilePath);

            if (!File.Exists(HandsFilePath))
                new XElement("Hands").Save(HandsFilePath);
        }

        public static void DropACard(Card? card)
        {
            if (card == null) return;

            var handsXml = XElement.Load(HandsFilePath);
            var dropXml = XElement.Load(DropFilePath);

            // Удаляем карту из "Hands"
            handsXml.Elements("Card")
                .Where(x => (int)x.Element("Id") == card.ID)
                .Remove();

            // Добавляем карту в "Drop"
            dropXml.Add(new XElement("Card",
                new XElement("Id", card.ID),
                new XElement("Name", card.Name)));

            // Сохраняем изменения
            handsXml.Save(HandsFilePath);
            dropXml.Save(DropFilePath);
        }

        public static void Update()
        {
            var dropXml = XElement.Load(DropFilePath);

            // Берём последнюю карту из "Drop"
            var lastCardElement = dropXml.Elements("Card").LastOrDefault();
            if (lastCardElement != null)
            {
                var id = (int)lastCardElement.Element("Id");
                var name = (string)lastCardElement.Element("Name");
                SetName(id, name);
            }
        }

        public static event EventHandler? TopCardChanged;

        private static void SetName(int id, string name)
        {
            var card = Helper.CreateACard(id, name);
            if (card != null)
            {
                TopCard = card;
                TopCardChanged?.Invoke(card, EventArgs.Empty);
            }
        }

        public static void Clear()
        {
            // Очищаем оба XML-файла
            new XElement("Drop").Save(DropFilePath);
            new XElement("Hands").Save(HandsFilePath);

            // Сбрасываем текущую верхнюю карту
            TopCard = null;
        }
    }
}
