using MiningTycoon.CoreServices;
using MiningTycoon.RuntimeData;
using UnityEngine;

namespace MiningTycoon.Visualization
{
    public class ResourcesGUI : MonoBehaviour
    {
        private ResourcesRuntimeData resourcesData;
        private int resourcesCount;
        
        private void Start()
        {
            resourcesData = Services.Resolve<ResourcesRuntimeData>();
            resourcesData.OnChanged += ResourcesChangedHandler;
        }

        private void ResourcesChangedHandler()
        {
            resourcesCount = resourcesData.ResourceCount;
        }

        private void OnGUI()
        {
            var mainCamera = Camera.main;
            if(mainCamera == null)
                return;
            float size = 100;
            var rect = new Rect(0, 0, size, size);
            var style = new GUIStyle
            {
                fontSize = 50,
                alignment = TextAnchor.UpperLeft
            };
            GUI.Label(rect, $"Resources count: {resourcesCount}", style);
        }

        private void OnDestroy()
        {
            resourcesData.OnChanged -= ResourcesChangedHandler;
        }
    }
}