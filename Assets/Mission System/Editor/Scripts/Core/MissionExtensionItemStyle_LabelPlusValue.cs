using UnityEngine;
using UnityEngine.UIElements;

namespace Tomoe.MissionSystem.Editor
{
    public class MissionExtensionItemStyle_LabelPlusValue : VisualElement
    {
        private Label label;
        private Label value;
        
        public MissionExtensionItemStyle_LabelPlusValue(string label, string value)
        {
            VisualTreeAsset visualTree = Resources.Load<VisualTreeAsset>("VisualTree/MissionExtensionItemStyle_LabelPlusValue");
            visualTree.CloneTree(this);
            
            this.label = this.Q<Label>("PropertyName");
            this.value = this.Q<Label>("PropertyValue");
            
            UpdateView(label, value);
        }

        public void UpdateView(string label, string value)
        {
            this.label.text = label;
            this.value.text = value;
        }
    }
}