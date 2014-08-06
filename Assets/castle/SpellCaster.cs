using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.castle.Scripts;
using MoreLinq;
using UnityEngine;

namespace Assets.castle
{
    public class SpellCaster : MonoBehaviour
    {
        public Projectile[] SpellProjectiles;
        [SerializeField]
        private float _cooldown = 1;

        private TargetManager _targetManager;

        private Projectile _activeSpell;

        // Use this for initialization
        void Start()
        {
            _targetManager = GameObject.Find("Castle").GetComponent<TargetManager>();
            _activeSpell = SpellProjectiles.First();
            StartCoroutine(CastSpells());
        }


        IEnumerator CastSpells()
        {
            while (true)
            {
                yield return new WaitForSeconds(_cooldown);
                var target = _targetManager.GetTarget(_activeSpell.Damage);
                if (target != null)
                {
                    var spellProjectile = (Projectile)Instantiate(_activeSpell, transform.position, Quaternion.LookRotation(transform.position - target.transform.position));
                    spellProjectile.SetTarget(target);
                }
            }
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}
