
using System.Collections.Generic;
using UnityEngine;

namespace FullInspector.Samples.Games.Arena {
    /// <summary>
    /// The PlayerController controls the human player. It implements WASD movement and causes the
    /// player to lose the game when an enemy has collided with the player.
    /// </summary>
    [AddComponentMenu("Full Inspector Samples/Games/Arena/Player Controller")]
    public class SampleArenaPlayerController : BaseBehavior {
        [Comment("The speed that the player moves at")]
        public float Speed = 1;

        protected void FixedUpdate() {
            Vector3 moveDirection = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
            moveDirection *= Speed;

            transform.position += moveDirection;
        }

        protected override void Awake() {
            Debug.Log("Welcome! Use WASD to move around and avoid getting hit by the enemies. " +
                "They will destroy themselves if they collide with each other");
            base.Awake();
            AllControllers.Add(this);
        }

        protected void OnDestroy() {
            AllControllers.Remove(this);
        }

        protected void OnTriggerEnter(Collider collider) {
            if (collider.gameObject.GetComponent<SampleArenaPlayerController>() == null) {
                Debug.Log("You lost! Restart the game to play again. Your score was " + Time.time + ".");
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Contains every active player controller.
        /// </summary>
        public static List<SampleArenaPlayerController> AllControllers = new List<SampleArenaPlayerController>();
    }
}