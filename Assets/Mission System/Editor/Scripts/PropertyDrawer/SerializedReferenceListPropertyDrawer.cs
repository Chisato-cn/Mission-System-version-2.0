using System;
using System.Collections.Generic;
using System.Reflection;
using Tomoe.MissionSystem.Runtime;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Tomoe.MissionSystem.Editor
{
    public class SerializedReferenceListPropertyDrawer : VisualElement
    {
        private ListView listView;
        private SerializedProperty property;
        
        /// <param name="property">必须是数组类型的字段</param>
        public SerializedReferenceListPropertyDrawer(SerializedProperty property, FieldInfo info)
        {
            this.property = property;
            
            Undo.undoRedoPerformed += UndoRedoPerformed;
            RegisterCallback<DetachFromPanelEvent>(evt => Undo.undoRedoPerformed -= UndoRedoPerformed);
            
            SetupAndAddPolymorphicList(this, property, info.FieldType.GetElementType());
        }
        
        private void UndoRedoPerformed()
        {
            property.serializedObject.Update();
            UpdateDataSource(listView, property);
            listView.Rebuild();
        }
        
        private void SetupAndAddPolymorphicList(VisualElement root, SerializedProperty property, Type baseType)
        {
            var searchTree = ScriptableObject.CreateInstance<DerivedFromTypeSearchTree>();
            searchTree.Init(property.displayName, baseType);
            
            listView = BindingListView(root, property, searchTree);
            searchTree.OnEntrySelected += type => SearchTreeOnOnEntrySelected(type, property, listView);
        }

        private void SearchTreeOnOnEntrySelected(Type target, SerializedProperty property, ListView listView)
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
        
        private ListView BindingListView(VisualElement root, SerializedProperty property, DerivedFromTypeSearchTree provider)
        {
            var item = new MissionChainInspectorListView();
            item.ListView.headerTitle = property.displayName;
            root.Add(item);
            
            SetupListView(item.ListView, property, provider);
            return item.ListView;
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
                propertyContainer.Add(new Label(elementProperty.managedReferenceValue.ToString()));

                // 获取真实类型的所有字段
                var type = elementProperty.managedReferenceValue.GetType();
                var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                foreach (FieldInfo info in fields)
                {
                    var attribute = info.GetCustomAttribute<CustomPropertyDrawerTypeAttribute>();
                    var prop = elementProperty.FindPropertyRelative(info.Name);

                    if (attribute != null && !string.IsNullOrEmpty(attribute.DrawerType)) 
                        propertyContainer
                            .Add((VisualElement)Activator.CreateInstance(Type.GetType(attribute.DrawerType), prop));
                    else
                    {
                        var propertyField = new PropertyField(prop);
                        propertyField.Bind(elementProperty.serializedObject);
                        propertyField.RegisterCallback<FocusOutEvent>(evt =>
                            MissionChainEditor.Instance.UpdateGraphView());
                        propertyContainer.Add(propertyField);
                    }
                }
            };
            
            // 监听重新排序事件
            Action<int, int> handler = null;
            handler = (oldIndex, newIndex) =>
            {
                listView.itemIndexChanged -= handler;
                
                Undo.RecordObject(property.serializedObject.targetObject, "Reorder Items");
                if (oldIndex >= 0 && oldIndex < property.arraySize &&
                    newIndex >= 0 && newIndex < property.arraySize)
                {
                    property.MoveArrayElement(oldIndex, newIndex);
                    property.serializedObject.ApplyModifiedProperties();
                    UpdateDataSource(listView, property);
                }
                
                // 重新添加事件监听
                listView.itemIndexChanged += handler;
            };
            listView.itemIndexChanged += handler;
            listView.onRemove = view => DeleteSelectedItem(view, property);
            listView.onAdd = view =>
                SearchWindow.Open(new SearchWindowContext(GUIUtility.GUIToScreenPoint(Event.current.mousePosition)), provider);
        }
        
        private void UpdateDataSource(BaseListView listView, SerializedProperty property)
        {
            try
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
            catch (Exception e) { return; }
        }
        
        private void DeleteSelectedItem(BaseListView view, SerializedProperty property)
        {
            if (view.selectedIndex < 0 || view.selectedIndex >= property.arraySize) return;
            
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
    }
}