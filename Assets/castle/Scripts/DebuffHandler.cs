using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.castle.Scripts
{
    public class DebuffHandler : MonoBehaviour
    {

        public enum Debuff
        {
            Poison,
            Slow,
            Freeze,
            SelfDestruct,
            Irradiate,
            Frailty,
            WeakestLink
        }

        public enum Buff
        {
            HealthLink,
            Haste,
            Regen,
            Comradere
        }


        private HashSet<Modifier> _speedMods;
        private HashSet<Modifier> _healthMods;

        private Enemy _enemy;
        private NavMeshAgent _navAgent;
        private float _baseSpeed;
        private bool _speedModified = false;

        private float _baseHp;

        // Use this for initialization
        void Start()
        {
            _enemy = GetComponent<Enemy>();
            _navAgent = GetComponent<NavMeshAgent>();
            _baseSpeed = _navAgent.speed;

            _speedMods = new HashSet<Modifier>();
            _healthMods = new HashSet<Modifier>();

        }

        // Update is called once per frame
        void Update()
        {
            var speedsExpired = _speedMods.RemoveWhere(a => a.ExpirationTime < Time.time);
            if (speedsExpired > 0)
                UpdateSpeed();
        }


        public void AddSpeedMod(string modName, float duration, float staticAmount, float percentAmount)
        {
            if (_speedMods.All(a => a.Name != modName))
            {
                _speedMods.Add(new Modifier
                {
                    Name = modName,
                    ExpirationTime = Time.time + duration,
                    PercentModifier = percentAmount * .01f,
                    StaticModifier = staticAmount
                });
            }
            else
                _speedMods.Single(a => a.Name == modName).ExpirationTime = Time.time + duration;
            UpdateSpeed();
        }

        public void UpdateSpeed()
        {
            var newSpeed = _baseSpeed + _speedMods.Select(a => a.StaticModifier).Sum();
            var percentMod = 1 + _speedMods.Select(a => a.PercentModifier).Sum();
            newSpeed *= percentMod;
            _navAgent.speed = newSpeed;
        }

        public void DamageOverTime()
        {

        }

    }

    public class Modifier
    {
        public string Name;
        public float ExpirationTime;
        public float PercentModifier;
        public float StaticModifier;
    }


}
