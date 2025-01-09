
using System.Xml.Linq;

namespace GameUno
{
    public static class Game
    {
        private static int stepOrder;
        private static bool direction;
        private static bool run;
        private static readonly string GameFilePath = "Game.xml";

        public static int PlaceCount { get; private set; } = 3;
        public static int TargetScore { get; private set; } = 500;

        public static int Round { get; set; }

        static Game()
        {
            if (!File.Exists(GameFilePath))
            {
                var doc = new XDocument(
                    new XElement("Game",
                        new XElement("StepOrder", 0),
                        new XElement("Direction", false),
                        new XElement("Run", false)));
                doc.Save(GameFilePath);
            }
        }

        public static void Clear()
        {
            Round = 0;
            var doc = new XDocument(
                new XElement("Game",
                    new XElement("StepOrder", 0),
                    new XElement("Direction", false),
                    new XElement("Run", false)));
            doc.Save(GameFilePath);
        }

        public static void Update()
        {
            var doc = XDocument.Load(GameFilePath);
            var root = doc.Element("Game");

            if (root != null)
            {
                SetStepOrder(int.Parse(root.Element("StepOrder")?.Value ?? "0"));
                SetDirection(bool.Parse(root.Element("Direction")?.Value ?? "false"));
                SetRun(bool.Parse(root.Element("Run")?.Value ?? "false"));
            }
        }

        private static void SetStepOrder(int order)
        {
            if (stepOrder == order) return;
            stepOrder = order;
            StepOrderChanged?.Invoke(stepOrder, EventArgs.Empty);
        }

        public static void ReverseDirection()
        {
            SetDirection(!direction);
        }

        private static void SetDirection(bool dir)
        {
            if (direction == dir) return;
            direction = dir;
            UpdateGameFile("Direction", direction.ToString());
            DirectionChanged?.Invoke(direction, EventArgs.Empty);
        }

        private static void SetRun(bool r)
        {
            if (run == r) return;
            run = r;
            UpdateGameFile("Run", run.ToString());
            RunChanged?.Invoke(run, EventArgs.Empty);
        }

        public static int GetPrevStepOrder(int stepOrder)
        {
            int step;
            if (Direction)
            {
                step = stepOrder;
                step++;
                if (step > PlaceCount)
                    step = 1;
            }
            else
            {
                step = stepOrder;
                step--;
                if (step == 0)
                    step = PlaceCount;
            }
            return step;
        }

        public static void PrevStep()
        {
            if (!run) return;
            StepOrder = GetPrevStepOrder(StepOrder);
        }

        public static int GetNextStepOrder(int stepOrder)
        {
            int step;
            if (!Direction)
            {
                step = stepOrder;
                step++;
                if (step > PlaceCount)
                    step = 1;
            }
            else
            {
                step = stepOrder;
                step--;
                if (step == 0)
                    step = PlaceCount;
            }
            return step;
        }

        public static void NextStep()
        {
            if (!run) return;
            StepOrder = GetNextStepOrder(StepOrder);
        }

        public static void StopGame()
        {
            Run = false;
        }

        public static void RunGame()
        {
            Round = 1;
            Run = true;
        }

        public static event EventHandler? StepOrderChanged;
        public static int StepOrder
        {
            get
            {
                var doc = XDocument.Load(GameFilePath);
                var root = doc.Element("Game");
                return int.Parse(root?.Element("StepOrder")?.Value ?? "1");
            }
            set
            {
                stepOrder = value;
                UpdateGameFile("StepOrder", stepOrder.ToString());
                StepOrderChanged?.Invoke(stepOrder, EventArgs.Empty);
            }
        }

        public static event EventHandler? DirectionChanged;
        public static bool Direction
        {
            get => direction;
            private set
            {
                if (direction == value) return;
                direction = value;
                DirectionChanged?.Invoke(direction, EventArgs.Empty);
            }
        }

        public static event EventHandler? RunChanged;
        public static bool Run
        {
            get => run;
            private set
            {
                if (run == value) return;
                run = value;
                UpdateGameFile("Run", run.ToString());
                if (run)
                {
                    Helper.EmptyHands();
                    PurchaseDesk.ReshuffleCards();
                    GetPurchaseCardToHands();
                }
                RunChanged?.Invoke(run, EventArgs.Empty);
            }
        }

        public static void GetPurchaseCardToHands()
        {
            for (var i = 1; i <= PlaceCount; i++)
            {
                Helper.GetPurchaseCardToHands(i, 7);
                PlayPlaces.SetCountCards(i, 7);
            }
        }

        private static void UpdateGameFile(string elementName, string value)
        {
            var doc = XDocument.Load(GameFilePath);
            var root = doc.Element("Game");
            if (root != null)
            {
                root.Element(elementName)?.SetValue(value);
                doc.Save(GameFilePath);
            }
        }
    }
}
