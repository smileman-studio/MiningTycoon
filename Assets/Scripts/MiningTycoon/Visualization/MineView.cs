using System;
using MiningTycoon.GraphEntities;
using UnityEngine;

namespace MiningTycoon.Visualization
{
    [ExecuteInEditMode]
    public class MineView : MonoBehaviour
    {
        [SerializeField] private float multiplier;
        
        private Mine mine;
        public Mine Mine => mine;
        public event Action<MineView> OnChanged;
        public event Action<MineView> OnDestroyed;
        
        public void SetMine(Mine mine)
        {
            this.mine = mine;
            multiplier = mine.Multiplier;
        }
        
        private void Update()
        {
            if (!Mathf.Approximately(multiplier, mine.Multiplier))
            {
                mine.Multiplier = multiplier;
                OnChanged?.Invoke(this);
            }
        }
        
        private void OnGUI()
        {
            var mainCamera = Camera.main;
            if(mainCamera == null)
                return;
            float size = 100;
            var viewportPoint = mainCamera.WorldToViewportPoint(transform.position + new Vector3(0.8f, 0, 0));
            var rect = new Rect(Screen.width * viewportPoint.x - size * 0.5f,
                Screen.height * (1 - viewportPoint.y) - size * 0.5f, size, size);
            var style = new GUIStyle
            {
                fontSize = 30,
                alignment = TextAnchor.MiddleLeft
            };
            GUI.Label(rect, $"x{mine.Multiplier}", style);
        }
        
        private void OnDestroy()
        {
            OnDestroyed?.Invoke(this);
        }
    }
}