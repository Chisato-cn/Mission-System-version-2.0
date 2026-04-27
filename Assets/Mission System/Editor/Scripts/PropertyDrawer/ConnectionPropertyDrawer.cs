using Tomoe.MissionSystem.Editor;
using Tomoe.MissionSystem.Runtime;
using UnityEditor;
using UnityEngine.UIElements;

[CustomPropertyDrawer(typeof(Connection))]
public class ConnectionPropertyDrawer : PropertyDrawer
{
    public override VisualElement CreatePropertyGUI(SerializedProperty property) => new ConnectionDrawer(property);
}