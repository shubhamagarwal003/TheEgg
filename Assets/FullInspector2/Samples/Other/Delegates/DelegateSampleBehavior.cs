using UnityEngine;

namespace FullInspector.Samples.Other.Delegates {
    [AddComponentMenu("Full Inspector Samples/Other/Delegates")]
    public class DelegateSampleBehavior : BaseBehavior {
        [Margin(10)]
        [Comment("SerializedAction and SerializedFunc are pretty nifty - they are fully type " +
            "safe (up to 9 generic arguments) while also properly supporting both contra and " +
            "covariance.")]
        [InspectorHidePrimary]
        [ShowInInspector]
        private int __inspectorVar;

        [Margin(10)]
        [Comment("Notice that this action, which takes a BaseBehavior, also works on functions " +
            "which take a supertype of BaseBehavior, such as MonoBehaviour")]
        public SerializedAction<int, BaseBehavior> IntBaseBehaviorAction;

        [Margin(10)]
        [Comment("Notice that this generator function -- which returns types of BaseBehavior, " +
            "can accept a function that also returns only DelegateSampleBehavior (which is a " +
            "subclass of BaseBehavior)")]
        public SerializedFunc<BaseBehavior> BaseBehaviorGenerator;

        public void MyIntMonoBehaviourConsumer(int a, MonoBehaviour b) {
            Debug.Log(string.Format("MyIntMonoBehaviourConsumer({0}, {1}) called", a, b));

        }

        public void MyIntBaseBehaviorConsumer(int a, BaseBehavior b) {
            Debug.Log(string.Format("MyIntBaseBehaviorConsumer({0}, {1}) called", a, b));
        }

        public BaseBehavior MyBaseBehaviorGenerator() {
            return this;
        }

        public DelegateSampleBehavior MyDelegateSampleGenerator() {
            return this;
        }
    }
}