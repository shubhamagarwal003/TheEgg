using UnityEngine;

namespace Assets.castle.Scripts
{
    public class ClickDetector : MonoBehaviour
    {

        private Player _player;

        // Use this for initialization
        void Start()
        {
            _player = FindObjectOfType<Player>();
        }


        // Update is called once per frame
        void Update()
        {

        }
    }
}
