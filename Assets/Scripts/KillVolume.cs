using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillVolume : MonoBehaviour
{
    public event Action OnPlayerDie;

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.collider.tag == "Olive")
        {
            OnPlayerDie?.Invoke();
        }
    }
}
