using UnityEngine;
using UnityEngine.UIElements;

namespace Tomoe.MissionSystem.Editor
{
    public class MissionChainInspectorListView : VisualElement
    {
        public ListView ListView { get; private set; }
        
        public MissionChainInspectorListView()
        {
            VisualTreeAsset visualTree = Resources.Load<VisualTreeAsset>("VisualTree/MissionChainInspectorListView");
            visualTree.CloneTree(this);
            AddToClassList("internal");
            
            ListView = this.Q<ListView>();
            ListView.RegisterCallback<FocusOutEvent>(evt => MissionChainEditor.Instance.UpdateGraphView());
        }
    }
}