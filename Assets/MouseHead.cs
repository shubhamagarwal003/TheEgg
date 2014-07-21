using UnityEngine;

namespace Assets
{
    public class MouseHead : MonoBehaviour
    {
        private GameObject _mousePointer;
        // Use this for initialization
        void Start()
        {
            _mousePointer = GameObject.Find("MousePointer");
            Screen.showCursor = false;
        }

        // Update is called once per frame
        void Update()
        {
            transform.Rotate(-Input.GetAxis("Mouse Y"), Input.GetAxis("Mouse X"), 0);
            RaycastHit hit;
            if (Physics.Raycast(new Ray(transform.position, transform.forward), out hit))
                _mousePointer.transform.position = hit.point;
        }
    }
}
