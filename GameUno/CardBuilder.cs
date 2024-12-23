namespace GameUno
{
    /// <summary>
    /// Строит компоненты игровой карты
    /// </summary>
    public static class CardBuilder
    {
        /// <summary>
        /// Построение карт с номерами 0..9
        /// </summary>
        /// <param name="card">Карта для присвоения особенностей</param>
        public static void BuildNumericCard(Card card, int number)
        {
            card.Feature = new PrimitiveFeature(number);
            card.Cost = number;
        }

        /// <summary>
        /// Построение карты с требованием пропуска хода
        /// </summary>
        /// <param name="card">Карта для присвоения особенностей</param>
        public static void BuildSkipCard(Card card)
        {
            card.Feature = new ActiveFeature(AllowedOperations.Skip);
            card.Cost = 20;
        }

        /// <summary>
        /// Построение карты с требованием смены направления
        /// </summary>
        /// <param name="card">Карта для присвоения особенностей</param>
        public static void BuildRotateCard(Card card)
        {
            card.Feature = new ActiveFeature(AllowedOperations.Rotate);
            card.Cost = 20;
        }

        /// <summary>
        /// Построение карты с требованием взять ещё две карты
        /// </summary>
        /// <param name="card">Карта для присвоения особенностей</param>
        public static void BuildTakeTwoCard(Card card)
        {
            card.Feature = new ActiveFeature(AllowedOperations.TakeTwo);
            card.Cost = 20;
        }

        /// <summary>
        /// Построение карты с возможностью выбрать новый цвет
        /// </summary>
        public static void BuildWildColorCard(Card card)
        {
            //card.Color = CardColor.Black;
            card.Feature = new WildFeature(AllowedOperations.Color);
            card.Cost = 50;
        }

        /// <summary>
        /// Построение карты с возможностью выбрать новый цвет
        /// и с требованием взять ещё четыре карты
        /// </summary>
        /// <param name="card">Карта для присвоения особенностей</param>
        public static void BuildWildColorTakeFourCard(Card card)
        {
            //card.Color = CardColor.Black;
            card.Feature = new WildFeature(AllowedOperations.Color | AllowedOperations.TakeFour);
            card.Cost = 50;
        }
    }
}
