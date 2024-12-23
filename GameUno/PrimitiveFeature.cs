
namespace GameUno
{
    /// <summary>
    /// Содержит особенности числовой карты
    /// </summary>
    [Serializable]
    public class PrimitiveFeature : Feature
    {

        /// <summary>
        /// Локальное поле для хранения ограничений для операций
        /// </summary>
        readonly AllowedOperations allowedOperations;

        /// <summary>
        /// Свойство возвращает определённые в конструкторе ограничения для операций
        /// </summary>
        public override AllowedOperations AllowedOperations { get { return allowedOperations; } }

        public int Number { get; }

        /// <summary>
        /// Конструктор, недоступный вне проекта UnoCards
        /// (только для внутреннего использования)
        /// </summary>
        /// <param name="allowed">набор прав для операций</param>
        internal PrimitiveFeature(int number)
        {
            Number = number;
            Name = $"{number}";
            // запоминаем ограничения для операций в локальном поле
            allowedOperations = AllowedOperations.Drop;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
