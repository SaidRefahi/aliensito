#pragma warning disable 0414
using UnityEngine;
using System.Collections;

public class LookCamera : MonoBehaviour 
{
    [SerializeField] private float speedNormal = 10.0f;
    [SerializeField] private float speedFast   = 50.0f;

    [SerializeField] private float mouseSensitivityX = 5.0f;
    [SerializeField] private float mouseSensitivityY = 5.0f;
    
	float rotY = 0.0f;
    
	void Start()
	{
		if (GetComponent<Rigidbody>())
			GetComponent<Rigidbody>().freezeRotation = true;
	}

	void Update()
	{	
        // rotation        
        if (Input.GetMouseButton(1)) 
        {
            float rotX = transform.localEulerAngles.y + Input.GetAxis("Mouse X") * mouseSensitivityX;
            rotY += Input.GetAxis("Mouse Y") * mouseSensitivityY;
            rotY = Mathf.Clamp(rotY, -89.5f, 89.5f);
            transform.localEulerAngles = new Vector3(-rotY, rotX, 0.0f);
        }
		
		if (Input.GetKey(KeyCode.U))
		{
			gameObject.transform.localPosition = new Vector3(0.0f, 3500.0f, 0.0f);
		}

	}
}
