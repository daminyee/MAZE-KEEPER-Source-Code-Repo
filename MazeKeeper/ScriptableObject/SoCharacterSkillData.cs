using MazeKeeper.Define;
using UnityEngine;
using UnityEngine.Serialization;


namespace MazeKeeper.ScriptableObject
{
    [CreateAssetMenu(fileName = "SoCharacterSkillData", menuName = "SoData/SoCharacterSkillData")]
    public class SoCharacterSkillData : UnityEngine.ScriptableObject
    {
        public CharacterSkillType CharacterSkillType;
        public int                Level;
        public string             Description;
        public string             Name;
        public int                Price;
        public Sprite             SkillIcon;
        public float              SplashRadius;
        public float              CoolTime;
        [FormerlySerializedAs("AttackSkillTemplate")]
        public GameObject SkillTemplate;
    }
}