using UnityEngine;

namespace Assets.castle.Scripts
{
    public class Egg : MonoBehaviour
    {
        private Health _health;
        void Start()
        {
            _health = GetComponent<Health>();
            _health.SetClass(Health.HealthClass.Heavy);
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void DoDamage(float damage)
        {
            _health.DoDamage(damage);
        }
    }
}
