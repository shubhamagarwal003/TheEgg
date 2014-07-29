using System;
using System.Collections.Generic;
using UnityEngine;

namespace FullInspector.Samples.Games.Platformer {
    [AddComponentMenu("Full Inspector Samples/Games/Platformer/Effect Container Behavior")]
    public class SamplePlatformerEffectContainerBehavior : BaseBehavior {
        public List<BaseEffect> Effects;

        protected void FixedUpdate() {
            float dt = Time.deltaTime;

            int i = 0;
            while (i < Effects.Count) {
                BaseEffect effect = Effects[i];
                effect.Update(dt);

                if (effect.IsAlive == false) {
                    Effects.RemoveAt(i);
                }
                else {
                    ++i;
                }
            }
        }

        /// <summary>
        /// Returns the first effect of the given type.
        /// </summary>
        /// <typeparam name="TEffect">The effect type to retrieve.</typeparam>
        /// <param name="effect">If this returns true, then this will contain the discovered
        /// effect.</param>
        /// <returns>True if an effect of the given type was found, false otherwise.</returns>
        public bool TryGetEffect<TEffect>(out TEffect effect)
            where TEffect : BaseEffect {

            if (Effects == null) {
                effect = null;
                return false;
            }

            for (int i = 0; i < Effects.Count; ++i) {
                if (typeof(TEffect).IsInstanceOfType(Effects[i])) {
                    effect = (TEffect)Effects[i];
                    return true;
                }
            }

            effect = null;
            return false;
        }
    }
}