using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour
{

    private List<Ray> rays;
    private OVRCameraController cam;
    // Use this for initialization
    void Start()
    {
        rays = new List<Ray>();
        cam = GetComponent<OVRCameraController>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            //var clickRay =.(Input.mousePosition);
            //rays.Add(clickRay);
        }
        foreach (var ray in rays)
        {
            Debug.DrawRay(ray.origin, ray.direction * 100);
        }
    }


}
