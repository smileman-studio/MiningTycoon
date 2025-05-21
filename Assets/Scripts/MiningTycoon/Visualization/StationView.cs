using System;
using MiningTycoon.GraphEntities;
using UnityEngine;

namespace MiningTycoon.Visualization
{
    [ExecuteInEditMode]
    public class StationView : MonoBehaviour
    {
        [SerializeField] private int multiplier;
        
        private Station station;
        public Station Station => station;
        public event Action<StationView> OnChanged;
        public event Action<StationView> OnDestroyed;
        
        public void SetStation(Station station)
        {
            this.station = station;
            multiplier = station.Multiplier;
        }
        
        private void Update()
        {
            if (!Mathf.Approximately(multiplier, station.Multiplier))
            {
                station.Multiplier = multiplier;
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
            GUI.Label(rect, $"x{station.Multiplier}", style);
        }
        
        private void OnDestroy()
        {
            OnDestroyed?.Invoke(this);
        }
    }
}