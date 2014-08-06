using System.Collections;
using System.Linq;
using Assets.castle.Scripts;
using FullInspector;
using UnityEngine;

namespace Assets
{
    public class Projectile : BaseBehavior
    {
        private float Speed = 200;
        private float Acceleration = 500;
        public float Damage;
        private float speed;
        private Enemy target;
        private bool consumed = false;
        // Use this for initialization
        void Start()
        {
            speed = 0;
            StartCoroutine(Fly());
        }

        // Update is called once per frame
        private void Update()
        {
            var particles = new ParticleSystem.Particle[1000];
            particleSystem.GetParticles(particles);
            for (int i = 0; i < particles.Count(); i++)
            {
                particles[i].lifetime *= speed / Speed;
            }
        }

        void OnTriggerEnter(Collider col)
        {
            if (!consumed)
            {
                consumed = true;
                var enemy = col.GetComponent<Enemy>();
                if (enemy != null)
                    enemy.DoDamage(Damage);

                particleSystem.Stop();
                Destroy(gameObject, 3);
            }
        }

        public void SetTarget(Enemy targetEnemy)
        {
            target = targetEnemy;
            transform.LookAt(target.transform);
        }

        IEnumerator Fly()
        {
            while (true)
            {
                if (speed < Speed)
                    speed += Acceleration * Time.deltaTime;
                var forward = transform.forward;
                if (target != null)
                {
                    var targetPos = target.transform.position;
                    var targetDisplacement = targetPos - transform.position;
                    var estimatedImpactTime = targetDisplacement.magnitude / speed;
                    var weight = speed / Speed;
                    var correctedTargetPosition = targetPos +
                                                  estimatedImpactTime * target.NavAgent.velocity * weight / 2;
                    forward = correctedTargetPosition - transform.position;
                }
                transform.Translate(forward.normalized * speed * Time.deltaTime, Space.World);
                yield return null;
            }
        }
    }
}
