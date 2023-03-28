using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.XR.PXR;

public class SeeThrough : MonoBehaviour
{
    void Awake()
    {
        PXR_Boundary.EnableSeeThroughManual(true);
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
