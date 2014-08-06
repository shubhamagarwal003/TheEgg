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

        public float EffectiveHp
        {
            get
            {
                var halfArmor = CurrentArmor / 2;
                if (halfArmor > CurrentHealth)
                    return halfArmor;
                return CurrentHealth + halfArmor;
            }
        }

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

        public float DoDamage(float damage, bool hypothetical = false)
        {
            var unmitigatedDamage = damage - CurrentArmor;
            var remainingArmor = CurrentArmor;
            var remainingHealth = CurrentHealth;
            if (unmitigatedDamage > 0) // did more damage than we had armor for
            {
                remainingHealth -= remainingArmor * .5f + unmitigatedDamage;
                Debug.Log(CurrentHealth + ", " + remainingHealth);
                remainingArmor = 0;
            }
            else
            {
                remainingHealth -= damage * .5f;
                remainingArmor -= damage;
            }
            if (!hypothetical)
            {
                CurrentArmor = remainingArmor;
                CurrentHealth = remainingHealth;
            }
            var halfRemainingArmor = remainingArmor / 2;
            if (halfRemainingArmor > remainingHealth)
                return halfRemainingArmor;
            return remainingHealth + halfRemainingArmor;
        }

    }
}
