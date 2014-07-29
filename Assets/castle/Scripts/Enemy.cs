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

        private const float _attackCooldown = .5f;
        private const float _attackDamage = 5f;
        private MeshRenderer _renderer;

        private Health _health;
        public Health Health { get { return _health; } }

        private DebuffHandler _debuffer;
        public DebuffHandler Debuffer { get { return _debuffer; } }
        void Start()
        {
            _health = GetComponent<Health>();
            _health.SetClass(HealthClass);
            _navAgent = GetComponent<NavMeshAgent>();
            _target = GameObject.Find("The Egg").GetComponent<Egg>();
            _renderer = GetComponent<MeshRenderer>();
            _debuffer = GetComponent<DebuffHandler>();
            StartCoroutine(Attack());
        }


        void Update()
        {
            _navAgent.SetDestination(
                Physics.RaycastAll(new Ray(transform.position, _target.transform.position - transform.position))
                    .First(a => a.transform.collider.tag == "Finish")
                    .point - (_target.transform.position - transform.position).normalized * 5);
        }

        public void DoDamage(float damage)
        {
            _health.DoDamage(damage);
        }

        private IEnumerator Attack()
        {
            while (true)
            {
                if ((_target.transform.position - transform.position).sqrMagnitude < 400)
                {
                    _target.DoDamage(_attackDamage);
                    _health.DoDamage(9999999);
                }
                yield return new WaitForSeconds(_attackCooldown);
            }
        }

    }
}
