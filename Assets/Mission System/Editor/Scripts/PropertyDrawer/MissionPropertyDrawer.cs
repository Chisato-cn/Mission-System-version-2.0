using Tomoe.MissionSystem.Runtime;
using UnityEditor;
using UnityEngine.UIElements;

namespace Tomoe.MissionSystem.Editor
{
    [CustomPropertyDrawer(typeof(Mission))]
    public class MissionPropertyDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property) => new MissionDrawer(property);
    }
}