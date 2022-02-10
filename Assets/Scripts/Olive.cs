using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Olive : Food, ICollectible
{
    public CollectableType collectableType { get => CollectableType.Olive; }

    [SerializeField] private new ConstantForce constantForce;

    Rigidbody rb;
    Vector3 velocity = Vector3.zero;

    bool inContactWithPlayer;
    protected override void Awake()
    {
        base.Awake();
        foodType = FoodType.Olive;
        rb = GetComponentInChildren<Rigidbody>();
    }

    protected override void FixedUpdate()
    {
        if (inContactWithPlayer == false && constantForce.enabled == false)
        {
            rb.velocity = velocity;
            rb.angularVelocity = velocity;
        }
    }

    void ICollectible.EnterCollectionArea()
    {
        Debug.Log("I am collected!");
        constantForce.enabled = true;
    }

    void ICollectible.ExitCollectionArea()
    {
        Debug.Log("Whoops, I left the collection area!");
    }

    /*private void OnCollisionEnter(Collision collision)
    {
        if(collision.collider.tag == "Player")
        {
            inContactWithPlayer = true;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.collider.tag == "Player")
        {
            inContactWithPlayer = false;
        }
    }*/
}