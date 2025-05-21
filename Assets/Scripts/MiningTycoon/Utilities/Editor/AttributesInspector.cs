using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace MiningTycoon.Utilities.Editor
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(Object), true)]
    public class AttributesInspector : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            DrawButtons();
        }

        private void DrawButtons()
        {
            var methods = ReflectionUtility
                .GetAllMethods(target, m => m.GetCustomAttributes(typeof(ButtonAttribute), true).Length > 0);
            foreach (var method in methods)
            {
                ButtonAttribute buttonAttribute = method.GetCustomAttribute(typeof(ButtonAttribute)) as ButtonAttribute;
                if (buttonAttribute == null)
                    continue;
                string buttonName = string.IsNullOrEmpty(buttonAttribute.ButtonMame)
                    ? method.Name
                    : buttonAttribute.ButtonMame;
                if (GUILayout.Button(buttonName))
                {
                    object[] defaultParams = method.GetParameters().Select(p => p.DefaultValue).ToArray();
                    method.Invoke(target, defaultParams);
                }
            }
        }
    }
}