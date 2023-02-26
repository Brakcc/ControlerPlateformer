using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToDust : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerMovement.instance.isHardened = false;
        }
    }
}
