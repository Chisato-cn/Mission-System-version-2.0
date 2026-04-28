using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Tomoe.MissionSystem.Runtime;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Tomoe.MissionSystem.Editor
{
    public class MissionChainEditor : EditorWindow
    {
        [SerializeField]
        private VisualTreeAsset m_VisualTreeAsset = default;
        
        [SerializeField] private MissionChain chain;
        
        public MissionChain MissionChain => chain;

        [MenuItem("Window/UI Toolkit/MissionChainEditor")]
        public static void ShowExample()
        {
            MissionChainEditor wnd = GetWindow<MissionChainEditor>();
            wnd.titleContent = new GUIContent("MissionChainEditor");
        }

        private MissionChainGraphView graphView;
        private VisualElement inspectorContainer;
        private VisualElement projectContainer;
        private int windowFocusChangedTimes;

        private SerializedObject serializedObject;
        
        public void CreateGUI()
        {
            m_VisualTreeAsset.CloneTree(rootVisualElement);
            serializedObject?.Dispose();
            serializedObject = new SerializedObject(chain);
            
            windowFocusChanged -= SearchWindowOnwindowFocusChanged;
            windowFocusChanged += SearchWindowOnwindowFocusChanged;
            
            graphView = new MissionChainGraphView(this);
            graphView.OnWindowFocusChanged += () => windowFocusChangedTimes = 3;
            rootVisualElement.Q<VisualElement>("GraphViewContainer").Add(graphView);
            
            inspectorContainer = rootVisualElement.Q<VisualElement>("InspectorContainer");
            projectContainer = rootVisualElement.Q<VisualElement>("ProjectContainer");
            projectContainer.RegisterCallback<PointerDownEvent>(evt =>
            {
                
            });
            
            graphView.PopulateGraph();
        }

        private void OnDisable()
        {
            windowFocusChanged -= SearchWindowOnwindowFocusChanged;
        }

        public void PopulateWindow(MissionChain chain)
        {
            this.chain = chain;
            
        }
        
        public void PopulateInspector(GraphElement element)
        {
            serializedObject.Update();
            inspectorContainer.Clear();
            
            if (element is ConnectionView connectionView) PopulateEdgePropertyView(connectionView); // 连接线
            else if (element is MissionChainNode node) PopulateNodePropertyView(node);              // 节点
            else PopulateChainPropertyView();                                                       // 链
        }

        private void PopulateEdgePropertyView(ConnectionView connectionView)
        {
            int index = chain.Connections.IndexOf(connectionView.Data);
            var edges = serializedObject.FindProperty("connections");
            var prop = edges.GetArrayElementAtIndex(index);

            HashSet<string> ignorance = new HashSet<string>(){ "m_Script", "inputMCNode", "outputMCNode", "conditions" };
            HashSet<string> disable = new HashSet<string>() { "guid" };
            var fields = typeof(Connection)                // 通过反射获取派生类全部字段
                .GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                
            foreach (FieldInfo info in fields)
            {
                if (ignorance.Contains(info.Name)) continue;
                
                bool isEnable = !disable.Contains(info.Name);
                var fieldProp = prop.FindPropertyRelative(info.Name);
                var item = new MissionChainInspectorItem(fieldProp, isEnable);
                
                item.PropField.AddToClassList("seventy-percent-width-property-field");
                inspectorContainer.Add(item);
                
                // 不用auto-width-property-field的话，bool类型的PropertyField会隔离非常近，不够美观
                // item.PropField.AddToClassList(info.Name == "guid"
                //     ? "seventy-percent-width-property-field"
                //     : "auto-width-property-field");
            }
            
            var condition = new ConditionDrawer(prop.FindPropertyRelative("conditions"));
            inspectorContainer.Add(condition);
        }

        private void PopulateNodePropertyView(MissionChainNode node)
        {
            // 找到对应的节点
            int index = chain.Nodes.IndexOf(node.Node);
            var nodes = serializedObject.FindProperty("nodes");
            var prop = nodes.GetArrayElementAtIndex(index);
                
            HashSet<string> ignorance = new HashSet<string>(){ "m_Script", "InputConnections", "outputConnections", "mission" };
            HashSet<string> disable = new HashSet<string>(){ "type", "guid", "Position",  };
            var nodeInstance = (MCNode)prop.managedReferenceValue;
            var fields = nodeInstance.GetType()                 // 通过反射获取派生类全部字段
                .GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
            var baseFields = typeof(MCNode).GetFields(          // 获取基类全部字段
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                
            // 合并字段
            var fieldNameToProperty = new Dictionary<string, SerializedProperty>();
            foreach (var field in baseFields)
            {
                if (ignorance.Contains(field.Name)) continue;
                
                var fieldProp = prop.FindPropertyRelative(field.Name);
                if (fieldProp != null) fieldNameToProperty[field.Name] = fieldProp;
            }
            foreach (var field in fields) 
            {
                if (ignorance.Contains(field.Name)) continue;
                
                var fieldProp = prop.FindPropertyRelative(field.Name); 
                if (fieldProp != null) fieldNameToProperty[field.Name] = fieldProp;
            }
                
            // 排序 基类字段在前，派生类字段在后
            var sortedProperties = fieldNameToProperty.Values
                .OrderBy(p => 
                {
                    // 判断字段属于基类还是派生类
                    var fieldName = p.name;
                    var isBaseField = baseFields.Any(f => f.Name == fieldName && 
                                                          (fields.All(df => df.Name != fieldName) || 
                                                           fieldNameToProperty[fieldName] == p));
                    return isBaseField ? 0 : 1;
                })
                .ThenBy(p => p.name)
                .ToList();
                
            foreach (SerializedProperty property in sortedProperties)
            {
                bool isEnable = !disable.Contains(property.name);
                var item = new MissionChainInspectorItem(property, isEnable);
                item.PropField.AddToClassList("seventy-percent-width-property-field");
                inspectorContainer.Add(item);
            }

            var missionProp = prop.FindPropertyRelative("mission");
            if (missionProp != null)
            {
                var item = new MissionDrawer(missionProp);
                inspectorContainer.Add(item);
            }
        }

        private void PopulateChainPropertyView()
        {
            var serializedProperty = serializedObject.GetIterator();
            serializedProperty.Next(true);      // 移动到第一个可序列化字段
            
            HashSet<string> ignorePropertiesSet = new HashSet<string>{ "m_Script", "connections", "nodes" };
            HashSet<string> disablePropertiesSet = new HashSet<string>{ "guid", "GraphPosition", "GraphScale" };
            while (serializedProperty.NextVisible(false))
            {
                if (ignorePropertiesSet.Contains(serializedProperty.name)) continue;
                
                var item = new MissionChainInspectorItem(serializedProperty, !disablePropertiesSet.Contains(serializedProperty.name));
                inspectorContainer.Add(item);
            }
        }

        private void PopulateProject()
        {
            
        }
        
        private void SearchWindowOnwindowFocusChanged()
        {
            if (windowFocusChangedTimes > 0) windowFocusChangedTimes--;
            if (windowFocusChangedTimes == 0 && graphView is { TempEdge: not null })
            {
                graphView.TempEdge.input?.Disconnect(graphView.TempEdge);
                graphView.TempEdge.output?.Disconnect(graphView.TempEdge);
                graphView.RemoveElement(graphView.TempEdge);
                graphView.TempEdge = null;
            }
        }

        public Vector2 ChangeCoordinatesToGraphView(Vector2 localPosition)
        {
            Vector2 offset = localPosition - position.position;
            Vector2 worldPosition = rootVisualElement.ChangeCoordinatesTo(rootVisualElement.parent, offset);
            return graphView.contentViewContainer.WorldToLocal(worldPosition);
        }
    }
}