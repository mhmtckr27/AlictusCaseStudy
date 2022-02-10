using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Level_3_Controller : LevelController
{
    [Header("FILEPATHS")]
    [SerializeField] private string jsonFileName;
    [SerializeField] private int rowCount = 6;
    [SerializeField] private int columnCount = 5;
    [SerializeField] private GameObject worldCanvas;

    public override int Level => 3;

    public override bool ShouldNotCollideWhenSpawning { get => false; }


    Dictionary<string, Food> foodsDictionary;
    //used for quick look-up for coordinates of foods
    Dictionary<Food, string> reverseFoodsDictionary;

    Food selectedFood;

    public override event Action<float> OnProgress;
    public override void Init(List<Food> foods)
    {
        base.Init(foods);
        foodsDictionary = new Dictionary<string, Food>();
        reverseFoodsDictionary = new Dictionary<Food, string>();
        ReadFromJSON();
        player = Instantiate(playerPrefab).GetComponent<Player>();
        player.inputManager.OnFoodSelected += SelectFood;
        player.inputManager.OnTrySwitchFoods += TrySwitch;
    }

    public override void StartLevel()
    {
        player.inputManager.isInputEnabled = true;
    }

    private void SelectFood(Food newSelectedFood)
    {
        selectedFood = newSelectedFood;
        worldCanvas.transform.SetParent(selectedFood.transform, true);
        worldCanvas.transform.position = new Vector3(selectedFood.transform.position.x, 0.01f, selectedFood.transform.position.z);
        worldCanvas.SetActive(true);
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

    public void TrySwitch(Food food1, Direction direction)
    {
        Food food2 = GetFoodInDirection(food1, direction);
        StartCoroutine(TrySwitchRoutines(food1, food2, direction));
    }

    private IEnumerator TrySwitchRoutines(Food food1, Food food2, Direction direction)
    {
        player.inputManager.EnableInput(false);
        yield return StartCoroutine(SwitchRoutine(food1, food2));
        List<int> toCheckRows = new List<int>();
        List<int> toCheckColumns = new List<int>();

        toCheckRows.Add(GetCoordinates(reverseFoodsDictionary[food1]).x);

        switch (direction)
        {
            case Direction.Up:
            //intentional fall off! dont add break;
            case Direction.Down:
                toCheckRows.Add(GetCoordinates(reverseFoodsDictionary[food2]).x);
                toCheckColumns.Add(GetCoordinates(reverseFoodsDictionary[food2]).y);
                break;
            case Direction.Left:
            //intentional fall off! dont add break;
            case Direction.Right:
                toCheckColumns.Add(GetCoordinates(reverseFoodsDictionary[food1]).y);
                toCheckColumns.Add(GetCoordinates(reverseFoodsDictionary[food2]).y);
                break;
            default:
                break;
        }

        List<Food> toExplodes = MarkFoodsThatWillExplode(toCheckRows, toCheckColumns);

        int len = toExplodes.Count;

        if(len > 0)
        {
            worldCanvas.transform.SetParent(null);
            worldCanvas.SetActive(false);
        }
        else
        {
            yield return StartCoroutine(SwitchRoutine(food1, food2));
        }

        for (int i = 0; i < len; i++)
        {
            Food toExplode = toExplodes[i];
            ExplodeFood(toExplode);
            if(foodsDictionary.Keys.Count == 0)
            {
                Invoke(nameof(CompleteLevel), 1f);
            }
        }

        player.inputManager.EnableInput(true);
    }

    private void ExplodeFood(Food toExplode)
    {
        Instantiate(explodeParticlePrefab, toExplode.transform.position, Quaternion.identity);
        foodsDictionary.Remove(reverseFoodsDictionary[toExplode]);
        reverseFoodsDictionary.Remove(toExplode);
        Destroy(toExplode.transform.parent.gameObject);
        OnProgress?.Invoke((float)1 / (rowCount * columnCount));
    }

    private List<Food> MarkFoodsThatWillExplode(List<int> toCheckRows, List<int> toCheckColumns)
    {
        List<Food> toExplodes = new List<Food>();
        MarkFoodsThatWillExplodeInRows(toCheckRows, toExplodes);
        MarkFoodsThatWillExplodeInColumns(toCheckColumns, toExplodes);
        return toExplodes;
    }

    private void MarkFoodsThatWillExplodeInColumns(List<int> toCheckColumns, List<Food> toExplodes)
    {
        int sameFoodCount;
        FoodType currentFoodType;
        foreach (int columnIndex in toCheckColumns)
        {
            sameFoodCount = 1;
            int k = -1;
            do
            {
                k++;
            } while (foodsDictionary.ContainsKey(GetCoordinates(new Vector2Int(k, columnIndex))) == false);

            currentFoodType = foodsDictionary[GetCoordinates(new Vector2Int(k, columnIndex))].foodType;
            for (int i = k + 1; i < rowCount; i++)
            {
                if (foodsDictionary.ContainsKey(GetCoordinates(new Vector2Int(i, columnIndex))) == false)
                {
                    continue;
                }
                if (foodsDictionary[GetCoordinates(new Vector2Int(i, columnIndex))].foodType == currentFoodType)
                {
                    sameFoodCount++;
                }
                else
                {
                    sameFoodCount = 1;
                    currentFoodType = foodsDictionary[GetCoordinates(new Vector2Int(i, columnIndex))].foodType;
                }
                if (sameFoodCount == 3)
                {
                    AddFoodToExplodesList(i, columnIndex, toExplodes);
                    AddFoodToExplodesList(i - 1, columnIndex, toExplodes);
                    AddFoodToExplodesList(i - 2, columnIndex, toExplodes);
                    currentFoodType = FoodType.None;
                }
            }
        }
    }

    private void MarkFoodsThatWillExplodeInRows(List<int> toCheckRows, List<Food> toExplodes)
    {
        int sameFoodCount;
        FoodType currentFoodType;
        foreach (int rowIndex in toCheckRows)
        {
            sameFoodCount = 1;
            int k = -1;
            do
            {
                k++;
            } while (foodsDictionary.ContainsKey(GetCoordinates(new Vector2Int(rowIndex, k))) == false);

            currentFoodType = foodsDictionary[GetCoordinates(new Vector2Int(rowIndex, k))].foodType;
            for (int j = k + 1; j < columnCount; j++)
            {
                if(foodsDictionary.ContainsKey(GetCoordinates(new Vector2Int(rowIndex, j))) == false)
                {
                    continue;
                }
                if (foodsDictionary[GetCoordinates(new Vector2Int(rowIndex, j))].foodType == currentFoodType)
                {
                    sameFoodCount++;
                }
                else
                {
                    sameFoodCount = 1;
                    currentFoodType = foodsDictionary[GetCoordinates(new Vector2Int(rowIndex, j))].foodType;
                }
                if (sameFoodCount == 3)
                {
                    AddFoodToExplodesList(rowIndex, j, toExplodes);
                    AddFoodToExplodesList(rowIndex, j - 1, toExplodes);
                    AddFoodToExplodesList(rowIndex, j - 2, toExplodes);
                    currentFoodType = FoodType.None;
                }
            }
        }
    }

    private void AddFoodToExplodesList(int row, int column, List<Food> toExplodes)
    {
        Food temp = foodsDictionary[GetCoordinates(new Vector2Int(row, column))];
        if (toExplodes.Contains(temp) == false)
        {
            toExplodes.Add(temp);
        }
    }

    private IEnumerator SwitchRoutine(Food food1, Food food2)
    {
        Vector3 pos1 = food1.transform.position;
        Vector3 pos2 = food2.transform.position;

        while(Vector3.Distance(food1.transform.position, pos2) > 0.1f)
        {
            yield return new WaitForSeconds(0.01f);
            food1.transform.position = Vector3.Lerp(food1.transform.position, pos2, 0.1f);
            food2.transform.position = Vector3.Lerp(food2.transform.position, pos1, 0.1f);
        }
        food1.transform.position = pos2;
        food2.transform.position = pos1;

        string coord1 = reverseFoodsDictionary[food1];
        string coord2 = reverseFoodsDictionary[food2];

        foodsDictionary[coord1] = food2;
        foodsDictionary[coord2] = food1;

        reverseFoodsDictionary[food1] = coord2;
        reverseFoodsDictionary[food2] = coord1;
    }

    private Food GetFoodInDirection(Food food1, Direction direction)
    {
        Vector2Int coords = GetCoordinates(reverseFoodsDictionary[food1]);

        Vector2Int newFoodCoords = new Vector2Int();
        switch (direction)
        {
            case Direction.Up:
                newFoodCoords.x = coords.x + 1;
                newFoodCoords.y = coords.y;
                break;
            case Direction.Down:
                newFoodCoords.x = coords.x - 1;
                newFoodCoords.y = coords.y;
                break;
            case Direction.Left:
                newFoodCoords.x = coords.x;
                newFoodCoords.y = coords.y - 1;
                break;
            case Direction.Right:
                newFoodCoords.x = coords.x;
                newFoodCoords.y = coords.y + 1;
                break;
            default:
                break;
        }
        return foodsDictionary[GetCoordinates(newFoodCoords)];
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

public enum FoodJSON
{
    Cherry,
    Banana,
    Watermelon,
    Hamburger,
    Cheese
}

public enum Direction
{
    Up,
    Down,
    Left,
    Right
}

public class FoodMatrixClass
{
    public int[] _matrix;
}