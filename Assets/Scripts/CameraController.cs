using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public GameObject player;
    public bool freezeCamera = false;

    private Vector3 offset;

    void Start()
    {
        offset = transform.position - player.transform.position;
    }

    void LateUpdate()
    {
        // Stop updating camera position when freeze is active
        if (freezeCamera) return;

        transform.position = player.transform.position + offset;
    }
}
