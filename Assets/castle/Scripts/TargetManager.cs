using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.castle.Scripts
{
    public class TargetManager : MonoBehaviour
    {

        // Use this for initialization
        private void Start()
        {
            _remainingHealths = new LinkedList<Enemy>();
        }

        private LinkedList<Enemy> _remainingHealths;


        // Update is called once per frame
        void Update()
        {
        }

        public void Register(Enemy enemy)
        {
            _remainingHealths.AddLast(enemy);
        }

        public void Remove(Enemy enemy)
        {
            _remainingHealths.Remove(enemy);
        }

        public Enemy GetTarget(float damage)
        {
            if (_remainingHealths.Count == 0)
                return null;
            var target = _remainingHealths.First();
            if (target.DoDamage(damage, true) <= 0)
                _remainingHealths.Remove(target);
            return target;
        }
    }
}
