using UnityEngine;

namespace Assets
{
    public class MouseHead : MonoBehaviour
    {
        private Terrain terrain;
        private Collider mouseCollider;
        // Use this for initialization
        void Start()
        {
            terrain = FindObjectOfType<Terrain>();
            Screen.showCursor = false;
            Screen.lockCursor = true;
            mouseCollider = GameObject.Find("MouseCollider").collider;
        }

        // Update is called once per frame
        void Update()
        {
            transform.Translate(Input.GetAxis("Mouse Y"), -Input.GetAxis("Mouse X"), 0);
            var bounds = terrain.collider.bounds;
            if (transform.position.x > bounds.max.x)
                transform.position = new Vector3(bounds.max.x, transform.position.y, transform.position.z);
            if (transform.position.x < bounds.min.x)
                transform.position = new Vector3(bounds.min.x, transform.position.y, transform.position.z);
            if (transform.position.z > bounds.max.z)
                transform.position = new Vector3(transform.position.x, transform.position.y, bounds.max.z);
            if (transform.position.z < bounds.min.z)
                transform.position = new Vector3(transform.position.x, transform.position.y, bounds.min.z);

            if (Input.GetKeyDown("escape"))
                Screen.lockCursor = false;
            if (Input.GetMouseButtonDown(0))
                Screen.lockCursor = true;
        }
    }
}
