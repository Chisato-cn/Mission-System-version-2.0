using UnityEngine;
using UnityEngine.UIElements;

namespace Tomoe.MissionSystem.Editor
{
    public class MissionChainProjectItem : VisualElement
    {
        public MissionChainProjectItem()
        {
            VisualTreeAsset visualTree = Resources.Load<VisualTreeAsset>("VisualTree/MissionChainProjectItem");
            visualTree.CloneTree(this);
            
            
        }
    }
}