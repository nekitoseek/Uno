namespace GameUno
{
    /// <summary>
    /// Класс-основа для задания особенностей игровой карты
    /// </summary>
    [Serializable]
    public abstract class Feature
    {
        public string? Name { get; set; }

        /// <summary>
        /// Допустимые операции над картой
        /// </summary>
        public abstract AllowedOperations AllowedOperations { get; }
    }
}
