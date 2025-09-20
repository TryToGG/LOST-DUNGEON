using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public GameObject chr;
    public Vector3 offset = new Vector3(0, 3, 5);

    private void LateUpdate()
    {
        if (!chr) chr = chr.gameObject;
        transform.position = chr.transform.position + chr.transform.TransformDirection(offset);
    }
}
