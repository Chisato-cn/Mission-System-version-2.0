using Tomoe.MissionSystem.Runtime;
using UnityEditor;
using UnityEngine.UIElements;

namespace Tomoe.MissionSystem.Editor
{
    [CustomPropertyDrawer(typeof(Condition))]
    public class ConditionPropertyDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property) => new ConditionDrawer(property);
    }
}