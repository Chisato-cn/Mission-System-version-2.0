using System;
using System.Collections.Generic;
using System.Reflection;
using Tomoe.MissionSystem.Runtime;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Tomoe.MissionSystem.Editor
{
    public class MissionChainSearchTree : ScriptableObject, ISearchWindowProvider
    {
        public event Action<Type, Vector2> OnEntrySelected; 
        
        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            List<SearchTreeEntry> results = new List<SearchTreeEntry>()
                { new SearchTreeGroupEntry(new GUIContent("Mission Chain Node")) };

            var types = TypeCache.GetTypesWithAttribute<MCNodeAttribute>();
            foreach (var type in types)
            {
                if (type.IsAbstract || !type.IsSubclassOf(typeof(MCNode))) continue;

                string absolutePath = type.GetCustomAttribute<MCNodeAttribute>().SearchTreePath;
                string[] path = absolutePath.Split('/');

                HashSet<string> existPath = new HashSet<string>();
                string buildingPath = string.Empty;
                for (int i = 0; i < path.Length; i++)
                {
                    if (string.IsNullOrEmpty(buildingPath)) buildingPath = path[i];
                    else buildingPath += '/' + path[i];

                    if (!existPath.Contains(buildingPath))
                    {
                        bool isLeafPath = i == path.Length - 1;
                        SearchTreeEntry entry = isLeafPath
                            ? new SearchTreeEntry(new GUIContent("     " + path[i])) { level = i + 1, userData = type }
                            : new SearchTreeGroupEntry(new GUIContent(path[i])) { level = i + 1 };
                        results.Add(entry);
                        existPath.Add(buildingPath);
                    }
                }
            }

            return results;
        }

        public bool OnSelectEntry(SearchTreeEntry SearchTreeEntry, SearchWindowContext context)
        {
            OnEntrySelected?.Invoke((Type)SearchTreeEntry.userData, context.screenMousePosition);
            return true;
        }
    }
}