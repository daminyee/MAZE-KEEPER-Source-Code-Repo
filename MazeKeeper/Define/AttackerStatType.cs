namespace MazeKeeper.Define
{
    public enum AttackerStatType
    {
        /// <summary>
        /// 공격속도 1일때의 Damage량
        /// </summary>
        AttackDPS,
        /// <summary>
        /// 몇초마다 한번의 Attack(혹은 Attack 한묶음) 공격할 것인지.
        /// </summary>
        AttackInterval,
        /// <summary>
        /// 공격 범위
        /// </summary>
        AttackRange,
        /// <summary>
        /// 기본 상태에서는 1로 고정.
        /// 값이 커지면, Interval이 비율만큼 감소
        /// </summary>
        AttackBuffSpeed,
        Length,
    }
}