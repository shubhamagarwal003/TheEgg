using System.Collections.Generic;
using System.Linq;
using Assets.castle.Scripts;
using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour
{

    private List<Ray> rays;
    private OVRCameraController cam;
    private GameObject mouse;
    // Use this for initialization
    void Start()
    {
        rays = new List<Ray>();
        cam = GetComponent<OVRCameraController>();
        mouse = GameObject.Find("MousePointer");
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            //transform.Rotate(-Input.GetAxis("Mouse Y"), Input.GetAxis("Mouse X"), 0);
            //RaycastHit hit;
            //if (Physics.Raycast(new Ray(transform.position, transform.forward), out hit))
            //    _mousePointer.transform.position = hit.point;
            //var enemies =
            //    Physics.OverlapSphere(mouse.transform.position, mouse.GetComponent<SphereCollider>().radius * mouse.transform.localScale.x).Where(a => a.collider.tag == "Enemy").Select(a => a.collider.GetComponent<Enemy>());
            //var enumerable = enemies as Enemy[] ?? enemies.ToArray();
            //Debug.Log(enumerable.Count());
            //foreach (var enemy in enumerable)
            //{
            //    enemy.DoDamage(10);
            //}
        }
    }


}
