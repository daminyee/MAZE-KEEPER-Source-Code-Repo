using System.Collections.Generic;
using MazeKeeper.Component;
using UnityEngine;


namespace MazeKeeper.ComponentGUI
{
    public class CompGUICellObject : MonoBehaviour
    {
        [SerializeField] List<CompGUICellObjectElement> _cellObjectElementList;
    }
}