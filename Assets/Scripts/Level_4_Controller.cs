using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class Level_4_Controller : LevelController
{
    [SerializeField] private Vector3 oliveSpawnPos;
    [SerializeField] private string jsonFileName;
    [SerializeField] private int rowCount = 3;
    [SerializeField] private int columnCount = 3;
    public override int Level => 4;

    public override bool ShouldNotCollideWhenSpawning => false;

    public override event Action<float> OnProgress;

    private Ball ball;

    Dictionary<string, Food> foodsDictionary;
    //used for quick look-up for coordinates of foods
    Dictionary<Food, string> reverseFoodsDictionary;
    public override void Init(List<Food> foods)
    {
        base.Init(foods);
        foodsDictionary = new Dictionary<string, Food>();
        reverseFoodsDictionary = new Dictionary<Food, string>();
        ReadFromJSON();
        player = Instantiate(playerPrefab, new Vector3(0, 0, bordersMovableArea.bounds.center.z), Quaternion.identity).GetComponent<Player>();
        Instantiate(playerFoodPrefab, player.transform);
        player.Init(FoodType.Hotdog);
        ball = Instantiate(GameManager.Instance.foodPrefabsDictionary["Olive"], oliveSpawnPos, Quaternion.identity).transform.GetChild(0).gameObject.AddComponent<Ball>();
        Destroy(ball.GetComponentInChildren<Olive>());
        ball.OnCollidedWithFood += OnCollidedWithFood;
    }

    private void OnCollidedWithFood(Food food)
    {
        switch (food.foodType)
        {
            case FoodType.Cherry:
                ball.velocity *= 1.25f;
                break;
            case FoodType.Banana:
                player.transform.localScale = new Vector3(1.5f, 1, 1);
                break;
            case FoodType.Hamburger:
                player.inputManager.isControlsReversed = !player.inputManager.isControlsReversed;
                break;
            case FoodType.Cheese:
                ball.StartCoroutine(ball.WobbleRoutine());
                break;
            case FoodType.Watermelon:
                ball.transform.parent.localScale = new Vector3(1.25f, 1.25f, 1.25f);
                break;
            default:
                break;
        }
        Instantiate(explodeParticlePrefab, food.transform.position, Quaternion.identity);
        Destroy(food.transform.parent.gameObject);
        OnProgress?.Invoke((float)1 / (rowCount * columnCount));
        foodsDictionary.Remove(reverseFoodsDictionary[food]);
        if(foodsDictionary.Count == 0)
        {
            ball.velocity = Vector3.zero;
            Invoke(nameof(CompleteLevel), 1f);
        }
    }

    public override void StartLevel()
    {
        base.StartLevel();
        player.inputManager.EnableInput(true);
    }


    public void ReadFromJSON()
    {
        string jsonStr = Resources.Load<TextAsset>(jsonFileName).text;

        int[] jsonMatrix = JsonUtility.FromJson<FoodMatrixClass>(jsonStr)._matrix;

        float columnStartPos = spawnableAreaCollider.bounds.min.x + 0.25f;
        float rowStartPos = spawnableAreaCollider.bounds.min.z + 0.25f;
        float columnEndPos = spawnableAreaCollider.bounds.max.x - 0.25f;
        float rowEndPos = spawnableAreaCollider.bounds.max.z - 0.25f;

        for (int i = 0; i < rowCount; i++)
        {
            GameObject temp;
            for (int j = 0; j < columnCount; j++)
            {
                Vector3 spawnPos = new Vector3();
                spawnPos.x = Mathf.Lerp(columnStartPos, columnEndPos, (float)j / (columnCount - 1));
                spawnPos.y = 0;
                spawnPos.z = Mathf.Lerp(rowStartPos, rowEndPos, (float)i / (rowCount - 1));
                temp = Instantiate(GameManager.Instance.foodPrefabsDictionary[((FoodJSON)jsonMatrix[i * columnCount + j]).ToString()], spawnPos, Quaternion.identity);
                Collider[] colliders = temp.GetComponentsInChildren<Collider>();
                int len = colliders.Length;
                for (int k = 0; k < len; k++)
                {
                    Destroy(colliders[k]);
                }

                Destroy(temp.GetComponentInChildren<Rigidbody>());
                temp.transform.GetChild(0).localScale /= 2;
                temp.transform.rotation = Quaternion.identity;
                temp.transform.GetChild(0).gameObject.AddComponent<BoxCollider>();

                foodsDictionary.Add(GetCoordinates(new Vector2Int(i, j)), temp.GetComponentInChildren<Food>());
                reverseFoodsDictionary.Add(temp.GetComponentInChildren<Food>(), GetCoordinates(new Vector2Int(i, j)));
            }
        }
    }


    private Vector2Int GetCoordinates(string coords)
    {
        Vector2Int result = new Vector2Int();
        char[] seperators = new char[3];
        seperators[0] = '_';
        seperators[1] = '\n';
        seperators[2] = '\0';

        string[] arrs = coords.Split(seperators);
        result.x = int.Parse(arrs[0]);
        result.y = int.Parse(arrs[1]);
        return result;
    }

    private string GetCoordinates(Vector2Int coords)
    {
        return coords.x + "_" + coords.y;
    }
}
