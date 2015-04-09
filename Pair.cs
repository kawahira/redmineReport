namespace RedmineToReport
{
    public class Pair<K, V>
    {
        /// <summary>
        ///     コンストラクタ
        /// </summary>
        public Pair(K k, V v)
        {
            Key = k;
            Value = v;
        }

        /// <summary>
        ///     Key情報(First)
        /// </summary>
        public K Key { get; set; }

        /// <summary>
        ///     Value情報(Sencond)
        /// </summary>
        public V Value { get; set; }
    }
}