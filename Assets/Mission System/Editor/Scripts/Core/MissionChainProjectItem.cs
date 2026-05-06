using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Tomoe.MissionSystem.Editor
{
    public class MissionChainProjectItem : VisualElement
    {
        private MissionChainEditor editor;
        private DropdownMenuHandler dropdownMenuHandler;
        private string missionChainGuid;
        
        public MissionChainProjectItem(MissionChainEditor editor, SerializedObject serializedObject)
        {
            VisualTreeAsset visualTree = Resources.Load<VisualTreeAsset>("VisualTree/MissionChainProjectItem");
            visualTree.CloneTree(this);
            AddToClassList("mission-chain-project-item");

            this.editor = editor;

            this.Q<VisualElement>("ChainIcon").style.backgroundImage =
                Resources.Load<Texture2D>("Icon/ScriptableObject Icon");
            var chainName = this.Q<TextField>("ChainName");
            chainName.label = String.Empty;
            var property = serializedObject.FindProperty("missionChainName");
            chainName.bindingPath = property.propertyPath;
            chainName.Bind(serializedObject);
            missionChainGuid = serializedObject.FindProperty("guid").stringValue;
            
            RegisterCallback<PointerDownEvent>(OnPointerDown);
            dropdownMenuHandler = new DropdownMenuHandler(menu =>
            {
                menu.AppendAction("Delete Chain Asset", action => this.editor.DeleteChainAsset(missionChainGuid));
                menu.AppendSeparator();
                menu.AppendAction("Copy Chain Guid", action => GUIUtility.systemCopyBuffer = missionChainGuid);
            });
        }

        private void OnPointerDown(PointerDownEvent evt)
        {
            if (evt.button == 0)
            {
                if (editor.MissionChain?.Guid == missionChainGuid) return;
                editor.OpenAssetBasedProjectItem(missionChainGuid);
            }
            else if (evt.button == 1)
            {
                dropdownMenuHandler.ShowMenu(evt);
                evt.StopImmediatePropagation();
            }
        }

        public void OnSelected() => AddToClassList("selected");

        public void OnDeselected() => RemoveFromClassList("selected");
    }
}