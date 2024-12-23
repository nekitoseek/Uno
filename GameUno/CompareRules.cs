namespace GameUno
{
    public static class CompareCardsRules
    {
        /// <summary>
        /// Правиля сравнения карт "UNO"
        /// </summary>
        /// <param name="color">Текущий установленный цвет в игре</param>
        /// <param name="origin">Карта сверху на стопке сброса</param>
        /// <param name="test">Тестируемая карта, которую хотят сбросить</param>
        /// <returns>true - сброс возможен</returns>
        public static bool Compare(CardColor color, Card origin, Card test)
        {
            return test.Feature is WildFeature || color == test.Color ||
                (origin.Feature is PrimitiveFeature num1) && (test.Feature is PrimitiveFeature num2) && num1.Number == num2.Number ||
                (origin.Feature is ActiveFeature act1) && (test.Feature is ActiveFeature act2) && act1.Allowed == act2.Allowed;
        }
    }
}
