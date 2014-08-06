using System.Collections;
using System.Linq;
using UnityEngine;

namespace Assets.castle.Scripts
{
    public class Enemy : MonoBehaviour
    {

        public NavMeshAgent NavAgent { get; private set; }
        private Vector3 _destination;
        private Egg _target;
        public Health.HealthClass HealthClass;

        private const float _attackCooldown = .5f;
        private const float _attackDamage = 5f;
        private MeshRenderer _renderer;

        public TargetManager TargetManager;

        private Health _health;
        public Health Health { get { return _health; } }

        private DebuffHandler _debuffer;
        public DebuffHandler Debuffer { get { return _debuffer; } }
        void Start()
        {
            _health = GetComponent<Health>();
            _health.SetClass(HealthClass);
            NavAgent = GetComponent<NavMeshAgent>();
            _target = GameObject.Find("The Egg").GetComponent<Egg>();
            _renderer = GetComponent<MeshRenderer>();
            _debuffer = GetComponent<DebuffHandler>();
            StartCoroutine(Attack());
            //StartCoroutine(Sprint());

            NavAgent.SetDestination(
                Physics.RaycastAll(new Ray(transform.position, _target.transform.position - transform.position))
                    .First(a => a.transform.collider.tag == "Finish")
                    .point - (_target.transform.position - transform.position).normalized * 5);
            transform.rotation = Quaternion.LookRotation(NavAgent.destination - transform.position);
            TargetManager = GameObject.Find("Castle").GetComponent<TargetManager>();
            TargetManager.Register(this);
        }


        void Update()
        {
        }

        public float DoDamage(float damage, bool hypothetical = false)
        {
            var effectiveHp = _health.DoDamage(damage, hypothetical);
            if (Health.CurrentHealth <= 0)
            {
                //TargetManager.Remove(this);
                Destroy(gameObject);
            }
            return effectiveHp;
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

        private IEnumerator Sprint()
        {
            var sprintPeriod = Random.Range(5f, 10f);
            yield return new WaitForSeconds(1);
            while (true)
            {
                Debuffer.AddSpeedMod("Sprint", 10, 0, 100);
                yield return new WaitForSeconds(sprintPeriod);
            }
        }

    }
}
