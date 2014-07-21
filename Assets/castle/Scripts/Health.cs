using System;
using UnityEngine;

namespace Assets.castle.Scripts
{
    public class Health : MonoBehaviour
    {
        private float _maxHealth;
        private float _remainingHealth;
        private float _maxArmor;
        private float _remainingArmor;

        public float MaxHealth { get { return _maxHealth; } }
        public float MaxArmor { get { return _maxArmor; } }
        public float CurrentHealth { get { return _remainingHealth; } }
        public float CurrentArmor { get { return _remainingArmor; } }
        public float PercentHp { get { return _remainingHealth / _maxHealth; } }

        public enum HealthClass
        {
            Heavy,
            Normal,
            Light
        }

        void Start()
        {
            SetClass(HealthClass.Normal);
        }

        void Update()
        {

        }

        public void SetClass(HealthClass hClass)
        {
            switch (hClass)
            {
                case HealthClass.Heavy:
                    _maxHealth = 1000;
                    _maxArmor = 2000;
                    break;
                case HealthClass.Normal:
                    _maxHealth = 100;
                    _maxArmor = 100;
                    break;
                case HealthClass.Light:
                    _maxHealth = 75;
                    _maxArmor = 50;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("hClass");
            }
            FullHeal();
        }


        public void FullHeal()
        {
            _remainingHealth = _maxHealth;
            _remainingArmor = _maxArmor;
        }

        public void DoDamage(float damage)
        {
            var damageReduction = _remainingArmor / _maxArmor;
            _remainingArmor -= damage;
            if (_remainingArmor > 0)
                _remainingArmor = 0;
            _remainingHealth -= damage * (1 - damageReduction);
            if (_remainingHealth < 0)
                Destroy(gameObject);
            //Debug.Log(gameObject.name + " took damage: " + CurrentArmor + "/" + MaxArmor + "|" + CurrentHealth + "/" + MaxHealth);

        }


    }
}
