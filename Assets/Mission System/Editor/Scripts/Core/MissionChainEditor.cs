using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Tomoe.MissionSystem.Runtime;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.Experimental.GraphView;
using UnityEditor.Search;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Tomoe.MissionSystem.Editor
{
    public class MissionChainEditor : EditorWindow
    {
        private static MissionChainEditor instance;
        
        [OnOpenAsset]
        public static bool OpenAsset(int entityID, int line)
        {
            Object obj = EditorUtility.EntityIdToObject(entityID);
            if (obj is MissionChain missionChain)
            {
                if (instance == null)
                {
                    instance = GetWindow<MissionChainEditor>();
                    instance.titleContent = new GUIContent("Mission Chain Editor");
                } 
                else instance.SaveGraph();
                
                instance.PopulateWindow(missionChain);
                return true;
            }
            
            return false;
        }

        private void SaveGraph()
        {
            chain.GraphPosition = graphView.viewTransform.position;
            chain.GraphScale = graphView.viewTransform.scale;
            EditorUtility.SetDirty(chain);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
        
        private MissionChainGraphView graphView;
        private VisualElement inspectorContainer;
        private VisualElement projectContainer;
        private ToolbarSearchField projectSearchField;
        private int windowFocusChangedTimes;
        
        /// <summary>
        /// key是chain的guid
        /// </summary>
        private Dictionary<string, (MissionChain asset, SerializedObject serializedObject)> chains;
        private SerializedObject serializedObject;
        private MissionChain chain;
        private QueryEngine<MissionChain> queryEngine;
        
        public MissionChain MissionChain => chain;
        public SerializedObject SerializedObject => serializedObject;
        public bool IsChainChangedDueToClearGraph { get; set; }
        
        public void CreateGUI()
        {
            VisualTreeAsset visualTree = Resources.Load<VisualTreeAsset>("VisualTree/MissionChainEditor");
            visualTree.CloneTree(rootVisualElement);
            
            // 初始化字典
            chains = new Dictionary<string, (MissionChain asset, SerializedObject serializedObject)>();
            string[] guids = AssetDatabase.FindAssets("t:MissionChain");
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                MissionChain asset = AssetDatabase.LoadAssetAtPath<MissionChain>(path);

                if (asset != null) chains.Add(asset.Guid, (asset, new SerializedObject(asset)));
            }
            // 初始化模糊搜索
            queryEngine = new QueryEngine<MissionChain>();
            queryEngine.SetSearchDataCallback(missionChain => new List<string>(){ missionChain.Name });
            // 设置模糊搜索匹配器
            queryEngine.SetSearchWordMatcher((searchWord, isExact, comparisonOption, searchData) =>
            {
                // 参数说明：
                // searchWord: 用户输入的搜索词
                // isExact: 是否要求精确匹配（通常为false）
                // comparisonOption: 字符串比较选项
                // searchData: 从SetSearchDataCallback返回的数组中的一个元素
                
                if (string.IsNullOrEmpty(searchWord) || string.IsNullOrEmpty(searchData)) return false;
                
                // 如果要求精确匹配
                if (isExact) return string.Equals(searchData, searchWord, comparisonOption);
                // 使用 FuzzySearch.FuzzyMatch 进行模糊匹配
                return FuzzySearch.FuzzyMatch(searchWord, searchData);
            });
            
            windowFocusChanged -= SearchWindowOnwindowFocusChanged;
            windowFocusChanged += SearchWindowOnwindowFocusChanged;
            
            inspectorContainer = rootVisualElement.Q<VisualElement>("InspectorContainer");
            projectContainer = rootVisualElement.Q<VisualElement>("ProjectContainer");
            
            graphView = new MissionChainGraphView(this);
            graphView.OnWindowFocusChanged += () => windowFocusChangedTimes = 3;
            rootVisualElement.Q<VisualElement>("GraphViewContainer").Add(graphView);
            
            var bottomPanel = rootVisualElement.Q<VisualElement>("BottomPanel");
            bottomPanel.AddManipulator(new DropdownMenuManipulator(menu =>
            {
                menu.AppendAction("Create Chain Asset", action =>
                {
                    var newChain = CreateInstance<MissionChain>();
                    
                    // 弹出 Unity 的资源保存对话框（会在项目内选择）
                    string path = EditorUtility.SaveFilePanelInProject(
                        "Save Mission Chain Asset",     // 对话框标题
                        "NewMissionChain",              // 默认文件名
                        "asset",                        // 文件扩展名
                        "Save the mission chain asset"  // 提示信息
                    );
                    
                    if (!string.IsNullOrEmpty(path))    // 用户点击了保存
                    {
                        AssetDatabase.CreateAsset(newChain, path);
                        AssetDatabase.SaveAssets();
                        AssetDatabase.Refresh();
            
                        // 聚焦到新创建的资源
                        Selection.activeObject = newChain;
                        EditorUtility.FocusProjectWindow();
                        
                        // 刷新界面
                        chains[newChain.Guid] = (newChain, new SerializedObject(newChain));
                        PopulateProject();
                    }
                    // 用户点击了取消
                    else DestroyImmediate(newChain);
                });
            }, MouseButton.RightMouse, true));
            
            projectSearchField = bottomPanel.Q<ToolbarSearchField>();
            projectSearchField.RegisterValueChangedCallback(evt => PopulateProject(evt.newValue));
        }

        private void OnDisable()
        {
            SaveGraph();
            windowFocusChanged -= SearchWindowOnwindowFocusChanged;
            foreach (var tuple in chains.Values)
            {
                tuple.serializedObject?.Dispose();
            }
            chains.Clear();
        }

        private void PopulateWindow(MissionChain chain)
        {
            // 数据初始化
            this.chain = chain;
            if (!chains.TryGetValue(chain.Guid, out var tuple))
            {
                // 防止特殊情况：窗口已经打开，但是在unity中手动创建missionChain资产，然后双击打开，此时这个资产并未被缓存到字典中
                serializedObject = new SerializedObject(chain);
                chains[chain.Guid] = (chain, serializedObject);
            }
            else serializedObject = tuple.serializedObject;
            
            graphView.PopulateGraph();
            PopulateInspector(null);
            PopulateProject();
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
            int index = chain.IndexOfConnection(connectionView.Data);
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
            int index = chain.IndexOfNode(node.Node);
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

        private void PopulateProject(string missionChainName = null)
        {
            projectContainer.Clear();
            if (string.IsNullOrEmpty(missionChainName))
            {
                foreach (var pair in chains)
                {
                    var item = new MissionChainProjectItem(this, pair.Value.serializedObject);
                    if (pair.Key == chain.Guid) 
                        item.OnSelected();
                    else item.OnDeselected();
                    projectContainer.Add(item);
                }
            }
            else
            {
                Debug.Log(missionChainName);
                var parsedQuery = queryEngine.ParseQuery(missionChainName);
                if (!parsedQuery.valid) return;

                var tempList = new List<MissionChain>();
                foreach (var tuple in chains.Values)
                {
                    tempList.Add(tuple.asset);
                }

                var missionChains = parsedQuery.Apply(tempList).ToList();
                foreach (MissionChain missionChain in missionChains)
                {   
                    var item = new MissionChainProjectItem(this, chains[missionChain.Guid].serializedObject);
                    if (missionChain.Guid == chain.Guid) 
                        item.OnSelected();
                    else item.OnDeselected();
                    projectContainer.Add(item);
                }
            }
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

        public void OpenAssetBasedProjectItem(string missionChainGuid)
        {
            SaveGraph();
            IsChainChangedDueToClearGraph = true;
            // 防止跨资产撤回，清空撤销栈
            Undo.ClearAll();
            PopulateWindow(chains[missionChainGuid].asset);
        }
    }
}