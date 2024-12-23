namespace GameUno
{
    /// <summary>
    /// Содержит особенности "дикой" карты
    /// </summary>
    [Serializable]
    public class WildFeature : Feature
    {

        /// <summary>
        /// Локальное поле для хранения ограничений для операций
        /// </summary>
        readonly AllowedOperations allowedOperations;

        /// <summary>
        /// Свойство возвращает определённые в конструкторе ограничения для операций
        /// </summary>
        public override AllowedOperations AllowedOperations { get { return allowedOperations; } }

        public AllowedOperations Allowed { get; }

        /// <summary>
        /// Конструктор, недоступный вне проекта UnoCards
        /// (только для внутреннего использования)
        /// </summary>
        /// <param name="allowed">набор прав для операций</param>
        internal WildFeature(AllowedOperations allowed)
        {
            Allowed = allowed;
            Name = $"Wild{allowed}";
            // запоминаем ограничения для операций в локальном поле
            allowedOperations = AllowedOperations.Drop | AllowedOperations.Wild | allowed;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
