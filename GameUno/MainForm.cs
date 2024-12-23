using Microsoft.VisualBasic.Logging;

namespace GameUno
{
    public partial class MainForm : Form
    {
        private int ThisOrder;
        private readonly Dictionary<string, Image> Images;
        private Card TopCard = null;
        private bool BotsMode = false;

        public MainForm()
        {
            InitializeComponent();
            Images = Helper.GetResourceImages();
            Game.DirectionChanged += Game_DirectionChanged;
            Game.RunChanged += Game_RunChanged;
            Game.StepOrderChanged += Game_StepOrderChanged;
            PlayPlaces.PlayerChanged += Players_PlayerChanged;
            DropDesk.TopCardChanged += DropDesk_TopCardChanged;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            PurchaseDesk.ReshuffleCards();
            Game.Update();
            PlayPlaces.Update();
            tsslStatusMessage.Text = ThisOrder == 1
                ? "Начните игру при комплекте игроков..."
                : "Ожидайте начала игры...";
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (ThisOrder == 1)
            {
                Game.StopGame();
                Game.Clear();
                PlayPlaces.Clear();
                DropDesk.Clear();
                Log.Clear();
            }
            else if (ThisOrder > 1)
            {
                PlayPlaces.ClearPlace(ThisOrder);
            }
        }

        private void Players_PlayerChanged(object sender, PlayerEventArgs args)
        {
            while (lvPlayers.Items.Count < args.StepOrder)
            {
                var lvi = new ListViewItem();
                lvi.SubItems.Add("0");
                lvi.SubItems.Add("0");
                lvPlayers.Items.Add(lvi);
            }
            var index = args.StepOrder - 1;
            var name = args.Name;
            lvPlayers.Items[index].Text = BotsMode && string.IsNullOrWhiteSpace(name) ? $"Бот {args.StepOrder}" : name;
            lvPlayers.Items[index].SubItems[1].Text = args.CountCards == 1 ? "uno" : args.CountCards.ToString();
            lvPlayers.Items[index].SubItems[2].Text = args.PlayScore.ToString();
        }

        /// <summary>
        /// Сверху новая карта сброшена
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DropDesk_TopCardChanged(object sender, EventArgs e)
        {
            var topCard = (Card)sender;
            if (TopCard != null && TopCard.ID == topCard.ID) return;
            TopCard = topCard;
            panelDirection.Invalidate();
            UpdateHandsCards();
            if (TopCard == null) return;
            UpdateAroundArrows(TopCard);
        }

        /// <summary>
        /// Очерёдность хода случилась
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Game_StepOrderChanged(object sender, EventArgs e)
        {
            var stepOrder = (int)sender;
            UpdatePlayerPositionAndStatus(stepOrder);
        }

        private void UpdatePlayerPositionAndStatus(int stepOrder)
        {
            if (stepOrder >= 1 && stepOrder <= Game.PlaceCount && stepOrder - 1 < lvPlayers.Items.Count)
            {
                var lvi = lvPlayers.Items[stepOrder - 1];
                lvi.Selected = true;
                lvPlayers.FocusedItem = lvi;
                var otherPlayerName = PlayPlaces.GetPlayerName(stepOrder);
                var isBot = BotsMode && otherPlayerName.StartsWith("Бот");
                var thisPlayerName = PlayPlaces.GetPlayerName(ThisOrder);

                var total = PlayPlaces.GetPlayScore(stepOrder);
                if (total > Game.TargetScore)
                    tsslStatusMessage.Text = $"{PlayPlaces.GetPlayerName(stepOrder)} выиграл эту игру со счётом: {total}";
                else
                    tsslStatusMessage.Text =
                    stepOrder > 0
                    ? stepOrder == ThisOrder ? $"Ваш ход, {thisPlayerName}" : $"{otherPlayerName} ходит..."
                    : "Ожидание начала игры...";
                timerStepAsBot.Enabled = Game.Run && isBot;
            }
        }

        private void timerStepAsBot_Tick(object sender, EventArgs e)
        {
            timerStepAsBot.Enabled = false;
            var waitTime = 0;
            var rand = new Random();
            waitTime = rand.Next(500, 1000);
            var botOrder = Game.StepOrder;
            var cards = Helper.GetHandsCards(botOrder);
            if (CanDropAnyCardsFromHands(botOrder))
            {
                CalculateBotCard(botOrder, cards);
                if (waitTime > 0) Thread.Sleep(waitTime);
                Game.NextStep();
            }
            else
            {
                botOrder = Game.StepOrder;
                // бот берёт карту из колоды прикупа
                GetACardFromPurchase(botOrder, isBot: true);
                if (CanDropAnyCardsFromHands(botOrder))
                {
                    cards = Helper.GetHandsCards(botOrder);
                    CalculateBotCard(botOrder, cards);
                    Game.NextStep();
                }
                if (waitTime > 0) Thread.Sleep(waitTime);
            }
        }

        private void CalculateBotCard(int botOrder, List<Card> cards)
        {
            var card = cards.Where(crd => CompareCardsRules.Compare(TopCard.Color, TopCard, crd)).First(); // здесь стратегия бота при выборе карты
            Log.Add($"{PlayPlaces.GetPlayerName(botOrder)} кладёт карту {card.Name}");
            UpdateCardActions(card, botOrder, isBot: true);
            PlayPlaces.AddOrSubCountCards(botOrder, -1);
            DropDesk.DropACard(card);
            DropDesk.Update();
            if (Helper.GetHandsCards(botOrder).Count() == 1)
                Log.Add($"{PlayPlaces.GetPlayerName(botOrder)} говорит UNO!");
            if (Helper.GetHandsCards(botOrder).Count() == 0)
                CalculateWinScore(botOrder);
        }

        private void CalculateWinScore(int order)
        {
            var score = Helper.CalculateWinScore(order);
            var total = PlayPlaces.AddPlayScore(order, score);
            PlayPlaces.Update();
            Log.Add($"{PlayPlaces.GetPlayerName(order)} выиграл этот раунд со счётом: {score}");
            if (total >= Game.TargetScore)
            {
                timerStepAsBot.Enabled = false;
                Game.StopGame();
                ThisOrder = 0;
                Log.Add($"{PlayPlaces.GetPlayerName(order)} выиграл игру со счётом: {total}");
                tsslStatusMessage.Text = $"{PlayPlaces.GetPlayerName(order)} выиграл эту игру со счётом: {total}";
                return;
            }
            Log.Add();
            DropDesk.Clear();
            DropDesk.DropACard(TopCard);
            DropDesk.Update();
            PurchaseDesk.ReshuffleCards();
            PurchaseDesk.DropThisCardId(TopCard.ID);
            Game.GetPurchaseCardToHands();
            UpdateHandsCards();
            Game.Round++;
            Log.Add($"=== Раунд: {Game.Round} ===");
            Log.Add($"Первая карта карта {TopCard.Name}");
        }

        private void Game_RunChanged(object sender, EventArgs e)
        {
            var run = (bool)sender;
            if (!run)
            {
                panelDirection.BackgroundImage = null;
                return;
            }
            StartPlayerGame();
        }

        private void StartPlayerGame()
        {
            if (!BotsMode)
                UpdateHandsCards();
            if (ThisOrder == 1)
            {
                if (TopCard == null)
                {
                    Helper.GetPurchaseCardToDropFirstCard();
                    Log.Clear();
                }
                DropDesk.Update();
                if (TopCard != null)
                {
                    Log.Add($"=== Раунд: {Game.Round} ===");
                    Log.Add($"Первая карта карта {TopCard.Name}");
                    if (TopCard.Feature.AllowedOperations.HasFlag(AllowedOperations.Wild))
                    {
                        var rand = new Random();
                        TopCard.ChangeWildColor((CardColor)rand.Next(4));
                        UpdateAroundArrows(TopCard);
                    }
                }
            }
        }

        /*
         * TODO:
         * 10. Механизм пополнения колоды прикупа при израсходовании колоды
         */

        private bool CanDropAnyCardsFromHands(int stepOrder)
        {
            if (TopCard != null)
            {
                var card = TopCard;
                var cards = Helper.GetHandsCards(stepOrder);
                return cards.Any(crd => CompareCardsRules.Compare(card.Color, card, crd));
            }
            return false;
        }

        private bool CanDropTheCardsFromHands(Card droping)
        {
            if (TopCard != null && droping != null)
            {
                var card = TopCard;
                return CompareCardsRules.Compare(card.Color, card, droping);
            }
            return false;
        }

        private void UpdateHandsCards()
        {
            lvCards.Items.Clear();
            imageListCards.Images.Clear();
            var cards = Helper.GetHandsCards(ThisOrder);
            if (cards.Count() < 8)
                imageListCards.ImageSize = new Size(80, 120);
            else
                imageListCards.ImageSize = new Size(40, 60);
            foreach (var card in cards)
            {
                imageListCards.Images.Add(card.Name, Images[card.Name]);
                var lvi = new ListViewItem(card.Name) { Tag = card };
                lvi.ImageKey = card.Name;
                lvCards.Items.Add(lvi);
            }
        }

        private void Game_DirectionChanged(object sender, EventArgs e)
        {
            if (TopCard == null) return;
            UpdateAroundArrows(TopCard);
        }

        /// <summary>
        /// Указатель цвета и направления игры приводим в соответствие с последней сброшенной картой
        /// </summary>
        /// <param name="card"></param>
        private void UpdateAroundArrows(Card card)
        {
            switch (card.Color)
            {
                case CardColor.Red:
                    panelDirection.BackgroundImage = !Game.Direction
                        ? Properties.Resources.Красные_стрелки_по_часовой
                        : Properties.Resources.Красные_стрелки_против_часовой;
                    break;
                case CardColor.Yellow:
                    panelDirection.BackgroundImage = !Game.Direction
                        ? Properties.Resources.Желтые_стрелки_по_часовой
                        : Properties.Resources.Желтые_стрелки_против_часовой;
                    break;
                case CardColor.Green:
                    panelDirection.BackgroundImage = !Game.Direction
                        ? Properties.Resources.Зеленые_стрелки_по_часовой
                        : Properties.Resources.Зеленые_стрелки_против_часовой;
                    break;
                case CardColor.Blue:
                    panelDirection.BackgroundImage = !Game.Direction
                        ? Properties.Resources.Синие_стрелки_по_часовой
                        : Properties.Resources.Синие_стрелки_против_часовой;
                    break;
            }
        }

        /// <summary>
        /// Опрос состояния игры по сети
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timerUpdate_Tick(object sender, EventArgs e)
        {
            Game.Update();
            PlayPlaces.Update();
            DropDesk.Update();
            lbPurchaseCardsCount.Text = PurchaseDesk.CardsCount().ToString();
            lvCards.Enabled = Game.Run && ThisOrder == Game.StepOrder;
            UpdatePlayerPositionAndStatus(Game.StepOrder);
            getCardToolStripMenuItem.Enabled = btnGetACardFromPurchase.Visible =
                Game.Run && Game.StepOrder == ThisOrder && !CanDropAnyCardsFromHands(ThisOrder);
            UpdateMessages();
        }

        private void UpdateMessages()
        {
            tbLog.Lines = Log.SelectLastMessages(100);
            tbLog.SelectionStart = tbLog.TextLength;
            tbLog.ScrollToCaret();
        }

        /// <summary>
        /// Меню "Очистить игру"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void clearGameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Game.StopGame();
            ClearGame();
            //ConnectToGame();
            ThisOrder = 0;
        }

        private static void ClearGame()
        {
            Log.Clear();
            Game.Clear();
            Game.Update();
            PlayPlaces.Clear();
            PlayPlaces.Update();
        }

        private void connectToGameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ConnectToGame();
        }

        private void ConnectToGame()
        {
            PlayPlaces.DefinePlaceOrder("Игрок");
            PlayPlaces.Update();
            ThisOrder = PlayPlaces.Order;
            var name = $"Игрок {ThisOrder}";
            PlayPlaces.SetPlayerName(ThisOrder, name);
            if (ThisOrder > 0)
                Text = $"Игра \"UNO\" - {name}";
        }

        private void gameToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
        {
            Game.Update();
            PlayPlaces.Update();

            connectToGameToolStripMenuItem.Enabled = ThisOrder == 0;
            gameWithBotsToolStripMenuItem.Enabled = runTheGameToolStripMenuItem.Enabled = ThisOrder == 1 && !Game.Run;
            stopTheGameToolStripMenuItem.Enabled = ThisOrder == 1 && Game.Run;
        }

        private void runTheGameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (ThisOrder == 1)
            {
                BotsMode = false;
                PurchaseDesk.ReshuffleCards();
                Game.RunGame();
                Game.StepOrder = 1;
                TopCard = null;
                StartPlayerGame();
            }
        }

        private void stopTheGameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (ThisOrder == 1)
            {
                Game.StopGame();
                ClearGame();
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void btnGetACardFromPurchase_Click(object sender, EventArgs e)
        {
            GetACardFromPurchase(ThisOrder);
        }

        private void GetACardFromPurchase(int stepOrder, bool isBot = false)
        {
            var lastCard = Helper.GetPurchaseCardToHands(stepOrder, 1);
            PlayPlaces.AddOrSubCountCards(stepOrder, 1);
            if (!isBot)
                UpdateHandsCards();
            Log.Add($"{PlayPlaces.GetPlayerName(stepOrder)} берёт карту из колоды");
            Game.Update();
            if (CanDropTheCardsFromHands(lastCard))
            {
                if (!isBot)
                {
                    var lvi = lvCards.Items.Cast<ListViewItem>().FirstOrDefault(item => item.Tag == lastCard);
                    if (lvi != null)
                    {
                        lvi.Selected = true;
                        lvCards.FocusedItem = lvi;
                        lvi.EnsureVisible();
                    }
                }
            }
            else
            {
                Log.Add($"{PlayPlaces.GetPlayerName(stepOrder)} пропускает ход");
                Game.NextStep();
            }
        }

        private void lvCards_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (lvCards.SelectedItems.Count == 0) return;
            var card = (Card)lvCards.SelectedItems[0].Tag;
            if (CanDropTheCardsFromHands(card))
            {
                Log.Add($"{PlayPlaces.GetPlayerName(ThisOrder)} кладёт карту {card.Name}");
                UpdateCardActions(card, ThisOrder);
                PlayPlaces.AddOrSubCountCards(ThisOrder, -1);
                DropDesk.DropACard(card);
                DropDesk.Update();
                UpdateHandsCards();
                btnSayUnoIfOneCard.Visible = Helper.GetHandsCards(ThisOrder).Count() == 1;
                if (Helper.GetHandsCards(ThisOrder).Count() == 0)
                    CalculateWinScore(ThisOrder);
                else
                    Game.NextStep();
            }
            else
            {
                tsslStatusMessage.BackColor = Color.Red;
                timerErrorStatus.Tag = tsslStatusMessage.Text;
                tsslStatusMessage.Text = $"Карту \"{card.Name}\" нельзя положить";
                timerErrorStatus.Enabled = true;
            }
        }

        private void timerErrorStatus_Tick(object sender, EventArgs e)
        {
            timerErrorStatus.Enabled = false;
            tsslStatusMessage.BackColor = Color.Transparent;
            tsslStatusMessage.Text = (string)timerErrorStatus.Tag;
        }

        /// <summary>
        /// Обработка активных карт
        /// </summary>
        /// <param name="card"></param>
        private void UpdateCardActions(Card card, int stepOrder, bool isBot = false)
        {
            var nextStepOrder = Game.GetNextStepOrder(stepOrder);
            if (card.Feature.AllowedOperations.HasFlag(AllowedOperations.Rotate))
            {
                Log.Add($"{PlayPlaces.GetPlayerName(nextStepOrder)} меняет направление игры и пропускает ход");
                Game.ReverseDirection();
                Game.PrevStep();
            }
            else
            if (card.Feature.AllowedOperations.HasFlag(AllowedOperations.Skip))
            {
                Log.Add($"{PlayPlaces.GetPlayerName(nextStepOrder)} пропускает ход");
                Game.NextStep();
            }
            else
            if (card.Feature.AllowedOperations.HasFlag(AllowedOperations.TakeTwo))
            {
                Log.Add($"{PlayPlaces.GetPlayerName(nextStepOrder)} берёт две карты и пропускает ход");
                Helper.GetPurchaseCardToHands(nextStepOrder, 2);
                PlayPlaces.AddOrSubCountCards(nextStepOrder, 2);
                Game.NextStep();
            }
            else
            if (card.Feature.AllowedOperations.HasFlag(AllowedOperations.Wild))
            {
                if (card.Feature.AllowedOperations.HasFlag(AllowedOperations.Color) &&
                    !card.Feature.AllowedOperations.HasFlag(AllowedOperations.TakeFour))
                {
                    CardColor selected;
                    if (!isBot)
                    {
                        var frm = new SelectColorForm();
                        frm.ShowDialog();
                        selected = frm.Color;
                    }
                    else
                    {
                        var rand = new Random();
                        selected = (CardColor)rand.Next(4); // здесь стратегия бота при выборе цвета
                    }
                    card.ChangeWildColor(selected);
                    UpdateAroundArrows(card);
                    Log.Add($"{PlayPlaces.GetPlayerName(stepOrder)} выбрал цвет: {selected}");
                }
                else if (card.Feature.AllowedOperations.HasFlag(AllowedOperations.Color | AllowedOperations.TakeFour))
                {
                    CardColor selected;
                    if (!isBot)
                    {
                        var frm = new SelectColorForm();
                        frm.ShowDialog();
                        selected = frm.Color;
                    }
                    else
                    {
                        var rand = new Random();
                        selected = (CardColor)rand.Next(4); // здесь стратегия бота при выборе цвета
                    }
                    card.ChangeWildColor(selected);
                    UpdateAroundArrows(card);
                    Log.Add($"{PlayPlaces.GetPlayerName(stepOrder)} выбрал цвет: {selected}");
                    Log.Add($"{PlayPlaces.GetPlayerName(nextStepOrder)} берёт четыре карты и пропускает ход");
                    Helper.GetPurchaseCardToHands(nextStepOrder, 4);
                    PlayPlaces.AddOrSubCountCards(nextStepOrder, 4);
                    Game.NextStep();
                }
            }
        }

        private void panelDirection_Paint(object sender, PaintEventArgs e)
        {
            if (TopCard == null || !Images.ContainsKey(TopCard.Name)) return;
            var g = e.Graphics;
            var image = Images[TopCard.Name];
            var srcRect = new Rectangle(0, 0, image.Width, image.Height);
            var kw = 1f * panelDirection.Width / image.Width;
            var kh = 1f * panelDirection.Height / image.Height;
            var newWidth = (kw < kh ? image.Width * kw : image.Width * kh) * 0.75f;
            var newHeight = (kw < kh ? image.Height * kw : image.Height * kh) * 0.75f;
            var destRect = new Rectangle((panelDirection.Width - (int)newWidth) / 2,
                (panelDirection.Height - (int)newHeight) / 2, (int)newWidth, (int)newHeight);
            g.DrawImage(image, destRect, srcRect, GraphicsUnit.Pixel);
        }

        private void showLogToolStripMenuItem_Click(object sender, EventArgs e)
        {
            splitContainer3.Panel2Collapsed = !showLogToolStripMenuItem.Checked;
        }

        private void btnSayUnoIfOneCard_VisibleChanged(object sender, EventArgs e)
        {
            if (btnSayUnoIfOneCard.Visible)
                timerSayUNOwait.Enabled = true;
        }

        private void btnSayUnoIfOneCard_Click(object sender, EventArgs e)
        {
            timerSayUNOwait.Enabled = false;
            btnSayUnoIfOneCard.Visible = false;
            Log.Add($"{PlayPlaces.GetPlayerName(ThisOrder)} сказал: UNO!");
        }

        private void timerSayUNOwait_Tick(object sender, EventArgs e)
        {
            timerSayUNOwait.Enabled = false;
            btnSayUnoIfOneCard.Visible = false;
            Log.Add($"{PlayPlaces.GetPlayerName(ThisOrder)} не успел сказать UNO и берёт две карты");
            Helper.GetPurchaseCardToHands(ThisOrder, 2);
            PlayPlaces.AddOrSubCountCards(ThisOrder, 2);
            UpdateHandsCards();
            PlayPlaces.Update();
        }

        /// <summary>
        /// Запуск игры с ботами
        /// Пустые имена игроков будут интерпретироваться как боты 
        /// и за них ходы будет делать программа
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void gameWithBotsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (ThisOrder == 1)
            {
                BotsMode = true;
                PurchaseDesk.ReshuffleCards();
                Game.RunGame();
                PlayPlaces.ClearScores();
                PlayPlaces.Update();
                TopCard = null;
                timerStepAsBot.Enabled = PlayPlaces.GetPlayerName(ThisOrder).StartsWith("Бот");
            }
        }
    }
}
