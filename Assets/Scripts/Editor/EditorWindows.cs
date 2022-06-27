using UnityEditor;

public class EditorWindows : Editor
{
    [MenuItem("Goo.io/Control Panel", false, 0)]
    public static void OpenControlPanel()
    {
        Selection.activeObject = ControlPanel.Instance;
    }
}