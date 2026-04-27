using Tomoe.MissionSystem.Runtime;
using UnityEditor;
using UnityEngine.UIElements;

[CustomPropertyDrawer(typeof(Mission))]
public class MissionPropertyDrawer : PropertyDrawer
{
    public override VisualElement CreatePropertyGUI(SerializedProperty property)
    {
        return base.CreatePropertyGUI(property);
    }
}
