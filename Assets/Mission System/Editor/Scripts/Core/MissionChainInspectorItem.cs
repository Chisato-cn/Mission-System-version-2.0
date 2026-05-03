using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Tomoe.MissionSystem.Editor
{
    public class MissionChainInspectorItem : VisualElement
    {
        private Label propName;
        private PropertyField propField;
        
        public PropertyField PropField => propField;

        public MissionChainInspectorItem(SerializedProperty serializedProperty, bool enable)
        {
            VisualTreeAsset visualTree = Resources.Load<VisualTreeAsset>("VisualTree/MissionChainInspectorItem");
            visualTree.CloneTree(this);
            AddToClassList("default");
            
            propName = this.Q<Label>("PropertyName");
            propField = this.Q<PropertyField>();
            
            propName.text = serializedProperty.displayName;
            propField.label = String.Empty;
            propField.bindingPath = serializedProperty.propertyPath;
            propField.Bind(serializedProperty.serializedObject);
            propField.SetEnabled(enable);
            propField.RegisterCallback<FocusOutEvent>(evt => MissionChainEditor.Instance.UpdateGraphView());
        }
    }
}