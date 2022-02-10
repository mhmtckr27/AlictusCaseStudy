using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Food : MonoBehaviour, IConvertable
{
    public FoodBehaviour foodBehaviour;
    public FoodType foodType;

    Rigidbody rb;
    Vector3 velocity = Vector3.zero;

    bool inContactWithPlayer;
    public event Action<Food, Food> OnFoodsCollide;
    protected virtual void Awake()
    {
        foodBehaviour = (FoodBehaviour)SceneManager.GetActiveScene().buildIndex;

        switch (foodType)
        {
            case FoodType.Banana:
                GetComponentInChildren<Rigidbody>().isKinematic = false;
                break;
            default:
                break;
        }

        rb = GetComponentInChildren<Rigidbody>();
    }

    public void Convert(Food otherFood)
    {
        OnFoodsCollide?.Invoke(this, otherFood);
    }
    protected virtual void FixedUpdate()
    {
        if (rb != null && inContactWithPlayer == false)
        {
            rb.velocity = velocity;
            rb.angularVelocity = velocity;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        Food otherFood = collision.collider.GetComponentInParent<Food>();
        if ((otherFood == null) || (foodBehaviour != FoodBehaviour.Convert))
        {
            if (collision.collider.tag == "Player")
            {
                inContactWithPlayer = true;
            }
            return;
        }
        if (otherFood.foodType == foodType)
        {
            Convert(otherFood);
        }
    }
    private void OnCollisionExit(Collision collision)
    {
        if (collision.collider.tag == "Player")
        {
            inContactWithPlayer = false;
        }
    }
}

public enum FoodBehaviour
{
    Collect = 1,
    Convert
}
public enum FoodType
{
    Olive, 
    Cherry,
    Banana,
    Hotdog,
    Hamburger,
    Cheese,
    Watermelon,
    None
}