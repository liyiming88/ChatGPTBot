using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FaceToCam : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject cam;

    // Update is called once per frame
    void Update()
    {
        transform.LookAt(cam.transform);
        transform.localEulerAngles = new Vector3(0, transform.eulerAngles.y, 0);
    }
}
