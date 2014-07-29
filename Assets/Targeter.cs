using System.Collections.Generic;
using Assets.castle.Scripts;
using UnityEngine;

namespace Assets
{
    public class Targeter : MonoBehaviour
    {

        private HashSet<Enemy> AOETargets;

        // Use this for initialization
        void Start()
        {
            AOETargets = new HashSet<Enemy>();
        }


        void OnTriggerEnter(Collider col)
        {
            if (col.gameObject.tag != "Enemy") return;

            var enemy = col.gameObject.GetComponent<Enemy>();
            if (!AOETargets.Contains(enemy))
            {
                AOETargets.Add(enemy);
                col.GetComponent<Highlighter>().SetHighlight(Highlighter.HighlightState.PassiveTarget);
            }
        }

        void OnTriggerExit(Collider col)
        {
            if (col.gameObject.tag != "Enemy") return;

            var enemy = col.gameObject.GetComponent<Enemy>();
            if (AOETargets.Contains(enemy))
            {
                AOETargets.Remove(enemy);
                col.GetComponent<Highlighter>().SetHighlight(Highlighter.HighlightState.None);
            }
        }



        // Update is called once per frame
        void Update()
        {
            foreach (var aoeTarget in AOETargets)
            {
                aoeTarget.Debuffer.ModifySpeed(5, .25f);
            }
        }
    }
}
