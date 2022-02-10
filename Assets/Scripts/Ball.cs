using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball : MonoBehaviour
{
    public Vector3 velocity = new Vector3(1f, 0f, -2f);
    private Rigidbody rb;

    private Vector3 savedVelocity;

    public event Action<Food> OnCollidedWithFood;
    
    private void Awake()
    {
        rb = GetComponentInChildren<Rigidbody>();
    }
    private void OnEnable()
    {
        UIManager.Instance.OnInventoryButton += OnInventoryButton;
    }

    private void OnDisable()
    {
        UIManager.Instance.OnInventoryButton -= OnInventoryButton;
    }

    private void OnInventoryButton(bool enableInventory)
    {
        if (enableInventory)
        {
            savedVelocity = velocity;
            velocity = Vector3.zero;
        }
        else
        {
            velocity = savedVelocity;
        }
    }

    void FixedUpdate()
    {
        rb.velocity = velocity;
    }

    private void OnCollisionEnter(Collision collision)
    {
        velocity = Vector3.Reflect(velocity, collision.GetContact(0).normal);
        Food collidedFood = collision.collider.gameObject.GetComponentInChildren<Food>();
        if(collidedFood != null)
        {
            OnCollidedWithFood?.Invoke(collidedFood);
        }
    }

    public IEnumerator WobbleRoutine()
    {
        float elapsedTime = 0;
        while(elapsedTime < 1f)
        {
            yield return new WaitForSeconds(0.1f);
            velocity.x += UnityEngine.Random.Range(-2f, 2f);
            elapsedTime += 0.1f;
        }
    }

}
