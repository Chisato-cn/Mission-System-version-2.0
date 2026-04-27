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
            
            graphView = new MissionChainGraphView();
            graphView.OnChangeCoordinatesToGraphView += ChangeCoordinatesToGraphView;
            graphView.OnWindowFocusChanged += () => windowFocusChangedTimes = 3;
            graphView.OnGraphElementSelected += PopulateInspector;
            rootVisualElement.Q<VisualElement>("GraphViewContainer").Add(graphView);
            
            inspectorContainer = rootVisualElement.Q<VisualElement>("InspectorContainer");
            projectContainer = rootVisualElement.Q<VisualElement>("ProjectContainer");
            
            graphView.PopulateGraph(chain);
        }

        private void OnDisable()
        {
            windowFocusChanged -= SearchWindowOnwindowFocusChanged;
        }

        public void PopulateWindow(MissionChain chain)
        {
            this.chain = chain;
            
        }
        
        private void PopulateInspector(GraphElement element)
        {
            Debug.Log("Populate Inspector");
            
            inspectorContainer.Clear();
            serializedObject.Update();
            if (element is ConnectionView connectionView)
            {
                
            }
            else if (element is MissionChainNode node)
            {
                // 找到对应的节点
                int index = chain.Nodes.IndexOf(node.Node);
                var nodes = serializedObject.FindProperty("nodes");
                var prop = nodes.GetArrayElementAtIndex(index);
                
                var nodeInstance = (MCNode)prop.managedReferenceValue;
                var fields = nodeInstance.GetType()                 // 通过反射获取派生类全部字段
                    .GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
                var baseFields = typeof(MCNode).GetFields(          // 获取基类全部字段
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                
                // 合并字段
                var fieldNameToProperty = new Dictionary<string, SerializedProperty>();
                foreach (var field in baseFields)
                {
                    var fieldProp = prop.FindPropertyRelative(field.Name);
                    if (fieldProp != null) fieldNameToProperty[field.Name] = fieldProp;
                }
                foreach (var field in fields)
                {
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
                
                // 如果是普通SerializedField，生成MissionChainInspectorItem；如果是SerializedReference，则根据其特性来生成对应的field
                foreach (SerializedProperty property in sortedProperties)
                {
                    var item = new MissionChainInspectorItem(property);
                    inspectorContainer.Add(item);
                }
                
            }
            else Debug.LogError($"未知ui元素被选中: {element}");
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

        private Vector2 ChangeCoordinatesToGraphView(Vector2 localPosition)
        {
            Vector2 offset = localPosition - position.position;
            Vector2 worldPosition = rootVisualElement.ChangeCoordinatesTo(rootVisualElement.parent, offset);
            return graphView.contentViewContainer.WorldToLocal(worldPosition);
        }
    }
}