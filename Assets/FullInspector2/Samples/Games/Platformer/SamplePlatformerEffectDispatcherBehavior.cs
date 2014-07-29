using System.Collections.Generic;
using UnityEngine;

namespace FullInspector.Samples.Games.Platformer {
    /// <summary>
    /// Interface for objects which can be dispatched to.
    /// </summary>
    public interface IOnDispatched {
        /// <summary>
        /// Do some action.
        /// </summary>
        /// <param name="dispatch">The GameObject that contains the
        /// SamplePlatformerEffectDispatcherBehavior component.</param>
        /// <param name="target">The GameObject that is being dispatched to.</param>
        void Act(GameObject dispatch, GameObject target);
    }

    public class DestroyOnDispatched : IOnDispatched {
        public void Act(GameObject dispatch, GameObject target) {
            Object.Destroy(dispatch);
        }
    }

    [AddComponentMenu("Full Inspector Samples/Games/Platformer/Effect Dispatcher Behavior")]
    public class SamplePlatformerEffectDispatcherBehavior : BaseBehavior {
        [Comment("The effects that are dispatched")]
        public List<BaseEffect> Effects;

        [Comment("Actions that are invoked when effects have been dispatched")]
        public List<IOnDispatched> OnDispatched;

        public void TryDispatch(GameObject target) {
            if (OnDispatched == null) {
                return;
            }

            var container = target.GetComponent<SamplePlatformerEffectContainerBehavior>();
            if (container == null) {
                return;
            }

            var clonedEffects = SerializationHelpers.Clone<List<BaseEffect>, JsonNetSerializer>(Effects);
            container.Effects.AddRange(clonedEffects);

            for (int i = 0; i < OnDispatched.Count; ++i) {
                OnDispatched[i].Act(gameObject, target);
            }
        }
    }
}