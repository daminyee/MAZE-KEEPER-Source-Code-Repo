using System.Collections.Generic;
using MazeKeeper.Class;
using MazeKeeper.Interface;
using MazeKeeper.Manager;
using MazeKeeper.ScriptableObject;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.Serialization;


namespace MazeKeeper.Component
{
    public abstract class CompCellObject : MonoBehaviour
    {
        [SerializeField] List<NavMeshModifierVolume> _enemyNavModList;
        [SerializeField] List<NavMeshModifierVolume> _arrowNavModList;

        public SoCellObjectData SoCellObjectData => m_soCellObjectData;
        SoCellObjectData        m_soCellObjectData;

        protected bool       m_IsPlacing;
        protected Vector2Int m_PlacedCellPos;

        PlayablePlayer m_PlayablePlayer_Placing;
        PlayablePlayer m_PlayablePlayer_Placed;


        public virtual void Init(SoCellObjectData soCellObjectData)
        {
            m_soCellObjectData       = soCellObjectData;
            m_PlayablePlayer_Placing = new(gameObject, "Placing");
            m_PlayablePlayer_Placed  = new(gameObject, "Placed");
        }


        /// <summary>
        /// 설치 중인지 아닌지를 세팅
        /// </summary>
        public virtual void SetPlacingMode(bool isPlacing)
        {
            if (m_soCellObjectData.BlockPath)
            {
                if (isPlacing)
                {
                    SetActiveNavMeshModifiers(false, true);
                }
                else
                {
                    SetActiveNavMeshModifiers(true, true);
                }
            }
            else
            {
                SetActiveNavMeshModifiers(false, false);
            }
            var result = GameManager.Instance.GameMazeController.GetCellPosFromWorldPosition(transform.position);
            m_PlacedCellPos = result.IsValid ? result.CellPos : Vector2Int.zero;
            m_IsPlacing     = isPlacing;
        }


        public void SetActiveNavMeshModifiers(bool enemy, bool arrow)
        {
            for (int i = 0; i < _enemyNavModList.Count; i++)
            {
                var enemyMod = _enemyNavModList[i];
                enemyMod.gameObject.SetActive(enemy);
            }
            for (int i = 0; i < _arrowNavModList.Count; i++)
            {
                var arrowMod = _arrowNavModList[i];
                arrowMod.gameObject.SetActive(arrow);
            }
        }


        public void Play_Placing() => m_PlayablePlayer_Placing.Play();
        public void Play_Placed()  => m_PlayablePlayer_Placed.Play();
    }
}
