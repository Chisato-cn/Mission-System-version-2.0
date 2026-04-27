using System;
using Tomoe.MissionSystem.Runtime;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Tomoe.MissionSystem.Editor
{
    public class ConnectionDrawer : VisualElement
    {
        public ConnectionDrawer(SerializedProperty property)
        {
            VisualTreeAsset visualTree = Resources.Load<VisualTreeAsset>("VisualTree/ConnectionDrawer");
            visualTree.CloneTree(this);

            var foldOut = this.Q<Foldout>("Container");
            foldOut.text = property.displayName;
            
            BindingProperty(this.Q<VisualElement>("IsValid"), property.FindPropertyRelative("isValid"));
            BindingProperty(this.Q<VisualElement>("IsParallelConnection"), property.FindPropertyRelative("isParallelConnection"));
            BindingProperty(this.Q<VisualElement>("HasCondition"), property.FindPropertyRelative("hasCondition"));
        }

        private void BindingProperty(VisualElement root, SerializedProperty property)
        {
            var label = root.Q<Label>();
            label.text = property.displayName;
            
            var propertyField = root.Q<PropertyField>();
            propertyField.label = String.Empty;
            propertyField.BindProperty(property);
        }
    }
}