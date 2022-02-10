using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Level_2_Controller : LevelController
{
    public override int Level { get => 2; }

    public override bool ShouldNotCollideWhenSpawning { get => true; }

    private int totalConversionCount;
    private int currentConversionCount;
    public override event Action<float> OnProgress;

    public override void Init(List<Food> foods)
    {
        this.foods = foods;
        player = Instantiate(playerPrefab).GetComponent<Player>();
        Instantiate(playerFoodPrefab, player.transform, true);
        player.Init(FoodType.Cheese);
        foreach (Food food in foods)
        {
            food.OnFoodsCollide += Convert;
        }
        totalConversionCount = foods.Count;
    }

    public void Convert(Food food1, Food food2)
    {
        Debug.Log("TWO FOODS COLLIDED, CONVERTING...");

        Vector3 convertPos = Vector3.Lerp(food1.transform.position, food2.transform.position, 0.5f);
        Instantiate(explodeParticlePrefab, food1.transform.position, Quaternion.identity);
        Instantiate(explodeParticlePrefab, food2.transform.position, Quaternion.identity);

        FoodType newFoodType = food1.foodType + 1;

        food1.OnFoodsCollide -= Convert;
        food2.OnFoodsCollide -= Convert;
        foods.Remove(food1);
        foods.Remove(food2);
        Destroy(food1.transform.parent.gameObject);
        Destroy(food2.transform.parent.gameObject);
        convertPos.y = 0;
        Food newFood = Instantiate(GameManager.Instance.foodPrefabsDictionary[newFoodType.ToString()], convertPos, Quaternion.identity).GetComponentInChildren<Food>();
        newFood.OnFoodsCollide += Convert;
        foods.Add(newFood);
        currentConversionCount++;

        OnProgress?.Invoke((float)1 / totalConversionCount);

        if(newFood.foodType == FoodType.Watermelon)
        {
            Invoke(nameof(CompleteLevel), 1f);
        }
    }

    public override void StartLevel()
    {
        player.inputManager.EnableInput(true);
    }
}
