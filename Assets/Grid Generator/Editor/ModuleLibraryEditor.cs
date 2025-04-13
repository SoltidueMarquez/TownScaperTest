using Grid_Generator.Modules;
using UnityEditor;
using UnityEngine;

namespace Grid_Generator.Editor
{
    [CustomEditor(typeof(ModuleLibrary))]
    public class ModuleLibraryEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            var so = (ModuleLibrary)target;
            
            if (GUILayout.Button ("Import Module"))
            {
                so.ImportModule();
            }
        }
    }
}