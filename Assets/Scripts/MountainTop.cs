using System;
using UnityEngine;

public class MountainTop : MonoBehaviour
{
    bool occured = false;
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player") && !occured)
        {
            occured = true;
            //player reached top of mountain
            SpawnerController.Completed();

            //add text maybe?
        }
    }
}
