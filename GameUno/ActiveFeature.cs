namespace GameUno
{
    /// <summary>
    /// Содержит особенности активной карты
    /// </summary>
    [Serializable]
    public class ActiveFeature : Feature
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
        internal ActiveFeature(AllowedOperations allowed)
        {
            Allowed = allowed;
            Name = $"Active{allowed}";
            // запоминаем ограничения для операций в локальном поле
            allowedOperations = AllowedOperations.Drop | allowed;
        }

        public override string ToString()
        {
            return Name ?? "Unnamed Feature";
        }
    }
}
