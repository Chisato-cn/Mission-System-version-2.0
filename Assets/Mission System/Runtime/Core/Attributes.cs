using System;

namespace Tomoe.MissionSystem.Runtime
{
    [AttributeUsage(AttributeTargets.Class)]
    public class MCNodeAttribute : Attribute
    {
        public string SearchTreePath { get; }

        public MCNodeAttribute(string searchTreePath)
        {
            SearchTreePath = searchTreePath;
        }
    }
    
    [AttributeUsage(AttributeTargets.Class)]
    public class MCNodeViewAttribute : Attribute
    {
        public NodeType DataType { get; }

        public MCNodeViewAttribute(NodeType dataType)
        {
            DataType = dataType;
        }
    }
}