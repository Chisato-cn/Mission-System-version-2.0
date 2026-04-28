using System;
using System.Collections.Generic;
using Tomoe.MissionSystem.Runtime;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Tomoe.MissionSystem.Editor
{
    public class MissionDrawer : VisualElement
    {
        private List<(ListView listView, SerializedProperty property)> list;
        
        public MissionDrawer(SerializedProperty property)
        {
            VisualTreeAsset visualTree = Resources.Load<VisualTreeAsset>("VisualTree/MissionDrawer");
            visualTree.CloneTree(this);
            
            Undo.undoRedoPerformed += UndoRedoPerformed;
            RegisterCallback<DetachFromPanelEvent>(evt => Undo.undoRedoPerformed -= UndoRedoPerformed);
            list = new List<(ListView, SerializedProperty)>();
            
            var foldOut = this.Q<Foldout>("Container");
            foldOut.text = property.displayName;
            
            BindingProperty(this.Q<VisualElement>("Name"), property.FindPropertyRelative("missionName"));
            BindingProperty(this.Q<VisualElement>("Description"), property.FindPropertyRelative("missionDescription"));
            BindingProperty(this.Q<VisualElement>("RequirementMode"), property.FindPropertyRelative("missionRequirementMode"));
            BindingProperty(this.Q<VisualElement>("CustomRequirementCompleteCount"), property.FindPropertyRelative("customRequirementCompleteCount"));
            
            SetupAndAddPolymorphicList("Mission Requirement", typeof(MissionRequirement), "missionRequirements", "Requirements", property);
            SetupAndAddPolymorphicList("Mission Reward", typeof(MissionReward), "missionRewards", "Rewards", property);
        }
        
        private void UndoRedoPerformed()
        {
            foreach (var tuple in list)
            {
                tuple.property.serializedObject.Update();
                UpdateDataSource(tuple.listView, tuple.property);
                tuple.listView.Rebuild();
            }
        }
        
        private void SetupAndAddPolymorphicList(string title, Type baseType, string propertyPath, string containerName, SerializedProperty rootProperty)
        {
            var searchTree = ScriptableObject.CreateInstance<DerivedFromTypeSearchTree>();
            searchTree.Init(title, baseType);
            
            var targetProperty = rootProperty.FindPropertyRelative(propertyPath);
            var listView = BindingListView(this.Q<VisualElement>(containerName), targetProperty, searchTree);
            searchTree.OnEntrySelected += type => RequirementSearchTreeOnOnEntrySelected(type, targetProperty, listView);
            
            list.Add((listView, targetProperty));
        }

        private void RequirementSearchTreeOnOnEntrySelected(Type target, SerializedProperty property, ListView listView)
        {
            Undo.RecordObject(property.serializedObject.targetObject, $"Add {target.Name}");
            
            // 增加数组大小
            int originalSize = property.arraySize;
            property.arraySize = originalSize + 1;
                
            // 获取新元素，创建新实例
            var newElement = property.GetArrayElementAtIndex(originalSize);
            object newInstance = Activator.CreateInstance(target);
                
            if (newInstance != null)
            {
                newElement.managedReferenceValue = newInstance;
                property.serializedObject.ApplyModifiedProperties();
                    
                // 刷新数据源
                UpdateDataSource(listView, property);
            }
            else property.arraySize = originalSize;         // 回滚
        }

        private void BindingProperty(VisualElement root, SerializedProperty property)
        {
            var label = root.Q<Label>();
            label.text = property.displayName;
            
            var propertyField = root.Q<PropertyField>();
            propertyField.label = String.Empty;
            propertyField.bindingPath = property.propertyPath;
            propertyField.Bind(property.serializedObject);
        }

        private ListView BindingListView(VisualElement root, SerializedProperty property, DerivedFromTypeSearchTree provider)
        {
            var label = root.Q<Label>();
            label.text = property.displayName;
            
            var listview = root.Q<ListView>();
            listview.headerTitle = property.displayName;
            SetupListView(listview, property, provider);
            
            return listview;
        }

        private void SetupListView(ListView listView, SerializedProperty property, DerivedFromTypeSearchTree provider)
        {
            UpdateDataSource(listView, property);

            // 绑定模板
            listView.makeItem = () =>
            {
                var container = new VisualElement();
                container.name = "property-field-container";
                return container;
            };
            
            // 数据绑定
            listView.bindItem = (element, index) =>
            {
                var items = (List<SerializedProperty>)listView.itemsSource;
                if (items == null || index >= items.Count) return;

                var elementProperty = items[index];
                var propertyContainer = element.Q<VisualElement>("property-field-container");
                propertyContainer.Clear();
                
                var propertyField = new PropertyField(elementProperty);
                propertyField.Bind(elementProperty.serializedObject);
                propertyField.label = elementProperty.managedReferenceValue.ToString();
                propertyContainer.Add(propertyField);
            };
            
            // 监听重新排序事件
            Action<int, int> handler = null;
            handler = (oldIndex, newIndex) =>
            {
                listView.itemIndexChanged -= handler;

                try
                {
                    Undo.RecordObject(property.serializedObject.targetObject, "Reorder Items");
            
                    if (oldIndex >= 0 && oldIndex < property.arraySize &&
                        newIndex >= 0 && newIndex < property.arraySize)
                    {
                        property.MoveArrayElement(oldIndex, newIndex);
                        property.serializedObject.ApplyModifiedProperties();
                        UpdateDataSource(listView, property);
                    }
                }
                finally
                {
                    // 重新添加事件监听
                    listView.itemIndexChanged += handler;
                }
            };
            listView.itemIndexChanged += handler;
            listView.onRemove = view => DeleteSelectedItem(view, property);
            listView.onAdd = view =>
                SearchWindow.Open(new SearchWindowContext(GUIUtility.GUIToScreenPoint(Event.current.mousePosition)), provider);
        }

        private void DeleteSelectedItem(BaseListView view, SerializedProperty property)
        {
            if (view.selectedIndex < 0 || view.selectedIndex >= property.arraySize)
            {
                Debug.LogWarning("没有选中要删除的项目");
                return;
            }
            
            // 记录撤销操作
            Undo.RecordObject(property.serializedObject.targetObject, "Remove Item");

            // 删除元素
            int indexToRemove = view.selectedIndex;
            property.DeleteArrayElementAtIndex(indexToRemove);
            property.serializedObject.ApplyModifiedProperties();

            // 刷新数据源
            view.selectedIndex = -1;
            UpdateDataSource(view, property);
        }
    
        private void UpdateDataSource(BaseListView listView, SerializedProperty property)
        {
            // 重新排序
            var propertyList = new List<SerializedProperty>(property.arraySize);
            for (int i = 0; i < property.arraySize; i++)
            {
                propertyList.Add(property.GetArrayElementAtIndex(i));
            }

            listView.itemsSource = propertyList;
            listView.Rebuild();
        }
    }
}