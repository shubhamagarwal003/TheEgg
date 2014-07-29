using System;
using System.Collections.Generic;
using UnityEngine;

namespace FullInspector.Samples.Games.Platformer {
    /// <summary>
    /// This controls the human player. It implements WASD movement and causes the player to lose
    /// the game when an enemy has collided with the player.
    /// </summary>
    [AddComponentMenu("Full Inspector Samples/Games/Platformer/Move Behavior")]
    public class SamplePlatformerMoveBehavior : BaseBehavior {
        [Comment("The speed that the player moves at")]
        public float BaseHorizontalSpeed = 1;

        [ShowInInspector]
        [NotSerialized]
        public float HorizontalSpeed {
            get {
                var container = gameObject.GetComponent<SamplePlatformerEffectContainerBehavior>();
                SpeedBoostEffect boostEffect;
                if (container != null && container.TryGetEffect(out boostEffect)) {
                    return BaseHorizontalSpeed * boostEffect.SpeedMultiplier;
                }

                return BaseHorizontalSpeed;
            }
            set { }
        }

        [Comment("How much of a force is applied when the player jumps?")]
        public float BaseJumpBurst = 2;

        [ShowInInspector]
        [NotSerialized]
        public float JumpBurst {
            get {
                var container = gameObject.GetComponent<SamplePlatformerEffectContainerBehavior>();
                JumpBoostEffect boostEffect;
                if (container != null && container.TryGetEffect(out boostEffect)) {
                    return BaseJumpBurst * boostEffect.JumpMultiplier;
                }

                return BaseJumpBurst;
            }
            set { }
        }

        [ShowInInspector]
        private bool _canJump = true;

        protected void FixedUpdate() {
            var moveDirection = new Vector2(Input.GetAxis("Horizontal") * HorizontalSpeed, 0);

            if (_canJump && Input.GetAxis("Vertical") > 0) {
                _canJump = false;
                moveDirection.y = Input.GetAxis("Vertical") * JumpBurst;

                Debug.Log("Jumping with moveDirection=" + moveDirection);
            }

            rigidbody2D.AddForce(moveDirection);
        }

        protected void OnCollisionEnter2D(Collision2D collision) {
            _canJump = true;

            var dispatch = collision.gameObject.GetComponent<SamplePlatformerEffectDispatcherBehavior>();
            if (dispatch != null) {
                dispatch.TryDispatch(gameObject);
            }
        }
    }
}