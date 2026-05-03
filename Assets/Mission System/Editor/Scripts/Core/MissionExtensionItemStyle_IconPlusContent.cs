using UnityEngine;
using UnityEngine.UIElements;

namespace Tomoe.MissionSystem.Editor
{
    public class MissionExtensionItemStyle_IconPlusContent : VisualElement
    {
        private Label content;
        private VisualElement icon;
        private Label value;
        
        public MissionExtensionItemStyle_IconPlusContent(Texture2D icon, string content, string value = null)
        {
            VisualTreeAsset visualTree = Resources.Load<VisualTreeAsset>("VisualTree/MissionExtensionItemStyle_IconPlusContent");
            visualTree.CloneTree(this);
            
            
            this.content = this.Q<Label>("PropertyContent");
            this.icon = this.Q<VisualElement>("PropertyIcon");
            this.value = this.Q<Label>("PropertyValue");
            
            UpdateView(icon, content, value);
        }
        
        public void UpdateView(Texture2D icon, string content, string value = null)
        {
            this.icon.style.backgroundImage = icon;
            this.content.text = content;

            if (!string.IsNullOrEmpty(value))
            {
                this.value.text = value;
                this.value.style.display = DisplayStyle.Flex;
            }
            else this.value.style.display = DisplayStyle.None;
        }
    }
}