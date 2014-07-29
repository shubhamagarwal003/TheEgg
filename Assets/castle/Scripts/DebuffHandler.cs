using System.Collections;
using System.Collections.Generic;
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


        private HashSet<Debuff> _debuffs;
        private HashSet<Buff> _buffs;

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
        }

        // Update is called once per frame
        void Update()
        {

        }


        public void ModifySpeed(float duration, float percentAmount)
        {
            if (!_speedModified)
            {
                _navAgent.speed = _baseSpeed * percentAmount;
                StartCoroutine(RestoreSpeed(duration));
            }
        }

        IEnumerator RestoreSpeed(float secondsToWait)
        {
            _speedModified = true;
            yield return new WaitForSeconds(secondsToWait);
            _navAgent.speed = _baseSpeed;
            _speedModified = false;
        }


        public void DamageOverTime()
        {

        }

    }
}
