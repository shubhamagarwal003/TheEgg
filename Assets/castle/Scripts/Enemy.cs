using System.Collections;
using System.Linq;
using UnityEngine;

namespace Assets.castle.Scripts
{
    public class Enemy : MonoBehaviour
    {

        private NavMeshAgent _navAgent;
        private Vector3 _destination;
        private Egg _target;
        public Health.HealthClass HealthClass;

        private float _attackCooldown = .5f;
        private float _attackDamage = 0f;
        private MeshRenderer _renderer;

        private Health _health;
        void Start()
        {
            _health = GetComponent<Health>();
            _health.SetClass(HealthClass);
            _navAgent = GetComponent<NavMeshAgent>();
            _target = GameObject.Find("The Egg").GetComponent<Egg>();
            _renderer = GetComponent<MeshRenderer>();

            StartCoroutine(Attack());
        }

        void Update()
        {
            _navAgent.SetDestination(
                Physics.RaycastAll(new Ray(transform.position, _target.transform.position - transform.position))
                    .First(a => a.transform.collider.tag == "Finish")
                    .point - (_target.transform.position - transform.position).normalized * 5);
            _renderer.material.color = new Color(1, _navAgent.remainingDistance / 50, _navAgent.remainingDistance / 50);
        }

        public void DoDamage(float damage)
        {
            _health.DoDamage(damage);
        }

        private IEnumerator Attack()
        {
            while (true)
            {
                if ((_target.transform.position - transform.position).sqrMagnitude < 350)
                {
                    _target.DoDamage(_attackDamage);
                }
                yield return new WaitForSeconds(_attackCooldown);
            }
        }



    }
}
