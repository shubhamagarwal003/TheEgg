using System;
using FullInspector;
using UnityEngine;

namespace Assets.castle.Scripts
{
    public class Health : BaseBehavior<JsonNetSerializer>
    {
        public float MaxHealth { get; private set; }
        public float MaxArmor { get; private set; }
        public float CurrentHealth { get; private set; }
        public float CurrentArmor { get; private set; }
        public float PercentHp { get { return CurrentHealth / MaxHealth; } }

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
                    MaxHealth = 1000;
                    MaxArmor = 2000;
                    break;
                case HealthClass.Normal:
                    MaxHealth = 100;
                    MaxArmor = 100;
                    break;
                case HealthClass.Light:
                    MaxHealth = 75;
                    MaxArmor = 50;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("hClass");
            }
            FullHeal();
        }


        public void FullHeal()
        {
            CurrentHealth = MaxHealth;
            CurrentArmor = MaxArmor;
        }

        public void DoDamage(float damage)
        {
            var damageReduction = CurrentArmor / MaxArmor;
            CurrentArmor -= damage;
            if (CurrentArmor < 0)
                CurrentArmor = 0;
            CurrentHealth -= damage * (1 - damageReduction);
            if (CurrentHealth < 0)
                Destroy(gameObject);

        }


    }
}
