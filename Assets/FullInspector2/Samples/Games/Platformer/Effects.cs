using System;
using System.Collections.Generic;
using UnityEngine;

namespace FullInspector.Samples.Games.Platformer {
    public abstract class BaseEffect {
        [Comment("Number of seconds the effect is active for")]
        public float Duration;

        public void Update(float dt) {
            Duration -= dt;
        }

        public bool IsAlive {
            get {
                return Duration > 0;
            }
        }
    }

    public class JumpBoostEffect : BaseEffect {
        public float JumpMultiplier;
    }

    public class GrowEffect : BaseEffect {
    }

    public class SpeedBoostEffect : BaseEffect {
        public float SpeedMultiplier;
    }
}