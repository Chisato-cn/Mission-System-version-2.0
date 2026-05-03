using Tomoe.MissionSystem.Runtime;
using UnityEditor;
using UnityEngine.UIElements;

namespace Tomoe.MissionSystem.Editor
{
    [CustomPropertyDrawer(typeof(Action))]
    public class ActionPropertyDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property) => new ActionDrawer(property);
    }
}
