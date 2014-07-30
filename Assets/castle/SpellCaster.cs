using System.Collections;
using System.Linq;
using Assets.castle.Scripts;
using UnityEngine;

namespace Assets
{
    public class SpellCaster : MonoBehaviour
    {
        [SerializeField]
        private Projectile[] _spellProjectiles;

        [SerializeField]
        private float _cooldown = 1;

        private Projectile _activeSpell;

        // Use this for initialization
        void Start()
        {
            _activeSpell = _spellProjectiles.First();
            StartCoroutine(CastSpells());
        }


        IEnumerator CastSpells()
        {
            while (true)
            {
                yield return new WaitForSeconds(_cooldown);


                var target = FindObjectsOfType<Enemy>().FirstOrDefault();
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
