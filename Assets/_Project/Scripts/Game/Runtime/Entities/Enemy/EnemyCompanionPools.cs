namespace Game
{
    /// <summary>One PoolOfObject&lt;EnemyCompanion&gt; per EnemyType, indexed by that enum - bound once in GameInstaller.</summary>
    public class EnemyCompanionPools
    {
        private readonly PoolOfObject<EnemyCompanion>[] _byType;

        public EnemyCompanionPools(PoolOfObject<EnemyCompanion>[] byType)
        {
            _byType = byType;
        }

        public PoolOfObject<EnemyCompanion> this[EnemyType type] => _byType[(int)type];
    }
}
