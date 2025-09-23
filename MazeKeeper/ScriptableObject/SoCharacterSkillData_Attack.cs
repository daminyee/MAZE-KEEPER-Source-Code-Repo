using MazeKeeper.Component;
using UnityEngine;


namespace MazeKeeper.ScriptableObject
{
    [CreateAssetMenu(fileName = "ScoCharacterSkillData_Attack", menuName = "SoData/SoCharacterSkillData_Attack")]
    public class SoCharacterSkillData_Attack : SoCharacterSkillData
    {
        public float AttackDamage;
        public float AttackInterval;
        public int   AttackCount;
        public float StartDelay;

        public SoEnemyStatusEffect SoEnemyStatusEffect;

        public CompPoolItem DamageEffect;
        public SoAudioClip  SoDamageSound;
    }
}