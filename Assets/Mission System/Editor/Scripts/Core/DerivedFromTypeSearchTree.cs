using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEditor.Experimental.GraphView;

namespace Tomoe.MissionSystem.Editor
{
    public class DerivedFromTypeSearchTree : ScriptableObject, ISearchWindowProvider
    {
        private string title;
        private Type derivedFromType;
        
        public event Action<Type> OnEntrySelected;

        public void Init(string title, Type derivedFromType)
        {
            this.title = title;
            this.derivedFromType = derivedFromType;
        }
        
        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            List<SearchTreeEntry> results = new List<SearchTreeEntry>()
                { new SearchTreeGroupEntry(new GUIContent(title)) };

            var types = TypeCache.GetTypesDerivedFrom(derivedFromType);
            foreach (Type type in types)
            {
                if (type.IsAbstract) continue;
                results.Add(new SearchTreeEntry(new GUIContent(type.Name)) { level = 1, userData = type });
            }
            
            return results;
        }

        public bool OnSelectEntry(SearchTreeEntry SearchTreeEntry, SearchWindowContext context)
        {
            OnEntrySelected?.Invoke((Type)SearchTreeEntry.userData);
            return true;
        }
    }
}