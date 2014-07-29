using System.Collections.Generic;
using FullInspector;
using UnityEngine;

namespace Assets.castle.Scripts
{
    public class Highlighter : BaseBehavior
    {

        private Material thisMaterial;
        private Color baseColor;
        public enum HighlightState
        {
            ActiveTarget,
            PassiveTarget,
            ChargingUp,
            None
        }

        public Dictionary<HighlightState, Color> ColorMap = new Dictionary<HighlightState, Color>
        {
            {HighlightState.ActiveTarget, Color.yellow},
            {HighlightState.PassiveTarget, Color.blue},
            {HighlightState.ChargingUp, Color.red},
            {HighlightState.None, Color.clear}
    };


        // Use this for initialization
        void Start()
        {
            thisMaterial = gameObject.renderer.material;
            baseColor = thisMaterial.color;
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void SetHighlight(HighlightState state)
        {
            thisMaterial.color = state == HighlightState.None ? baseColor : ColorMap[state];
        }
    }
}
