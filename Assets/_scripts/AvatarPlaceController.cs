using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
public class AvatarPlaceController : MonoBehaviour
{
    List<InputDevice> inputDevices = new List<InputDevice>();
    public float x;
    public float y;
    public float speed = 0.5f;
    public GameObject obj;

    void Update()
    {
        foreach (InputDevice device in inputDevices)
        {
            // Get joystick value
            if (device.TryGetFeatureValue(CommonUsages.primary2DAxis, out Vector2 primary2DAxisValue))
            {
                // Get joystick value
                x = primary2DAxisValue.x;
                y = primary2DAxisValue.y;
                obj.transform.Translate(Time.deltaTime * speed * -x, 0, Time.deltaTime * speed * -y);
            }
        }
    }

    void Start()
    {
        //Get the right controller
        InputDevices.GetDevicesWithCharacteristics(InputDeviceCharacteristics.Right | InputDeviceCharacteristics.Controller, inputDevices);
    }
}



