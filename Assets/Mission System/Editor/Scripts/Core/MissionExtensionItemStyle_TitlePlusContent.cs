using UnityEngine;
using UnityEngine.UIElements;

namespace Tomoe.MissionSystem.Editor
{
    public class MissionExtensionItemStyle_TitlePlusContent : VisualElement
    {
        private Label title;
        private Label content;
        
        public MissionExtensionItemStyle_TitlePlusContent(string title, string content)
        {
            VisualTreeAsset visualTree = Resources.Load<VisualTreeAsset>("VisualTree/MissionExtensionItemStyle_TitlePlusContent");
            visualTree.CloneTree(this);
            
            this.title = this.Q<Label>("PropertyTitle");
            this.content = this.Q<Label>("PropertyContent");
            
            UpdateView(title, content);
        }

        public void UpdateView(string title, string content)
        {
            this.title.text = title;
            this.content.text = content;
        }
    }
}