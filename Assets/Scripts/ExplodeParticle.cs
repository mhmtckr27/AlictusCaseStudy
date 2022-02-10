using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplodeParticle : MonoBehaviour
{
    [SerializeField] private ParticleSystem explodeParticleSystem;
    
    private void Awake()
    {
        Invoke("DestroySelf", explodeParticleSystem.main.startLifetime.constant);
    }

    private void DestroySelf()
    {
        Destroy(gameObject);
    }
}
