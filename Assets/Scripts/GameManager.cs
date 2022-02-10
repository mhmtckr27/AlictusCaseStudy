using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    /*
        Workaround for non-serialized data type dictionary, there is a very nice
        plugin that implements serializable dictionary but since you requested
        me to not use any third party library, I came up this workaround.
     */
    [Header("PREFABS")]
    [SerializeField] private List<string> foodNames;
    [SerializeField] private List<GameObject> foodPrefabs;


    [Header("LAYERMASKS")]
    [SerializeField] private LayerMask foodLayer;

    #region Singleton
    private static GameManager instance;
    public static GameManager Instance
    {
        get => instance;
    }
    #endregion

    private SphereCollider oliveCollider;
    public LevelController currentLevelController;

    public Dictionary<string, GameObject> foodPrefabsDictionary;
    public Dictionary<string, int> obtainedFoods;
    public GameObject randomFood;
    public event Action<LevelController> OnCurrentLevelControllerChanged;
    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
        DontDestroyOnLoad(gameObject);
        
        if(foodNames.Count != foodPrefabs.Count)
        {
            Debug.LogError("Food names and food prefabs count does not match!!");
            return;
        }
        foodPrefabsDictionary = new Dictionary<string, GameObject>();
        for (int i = 0; i < foodNames.Count; i++)
        {
            foodPrefabsDictionary.Add(foodNames[i], foodPrefabs[i]);
        }
        obtainedFoods = new Dictionary<string, int>();
        oliveCollider = foodPrefabsDictionary["Olive"].GetComponentInChildren<SphereCollider>();
    }

    public void LoadLevel(int level)
    {
        SceneManager.LoadScene(level);
    }

    public void LoadNextLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    private void OnLevelWasLoaded(int level)
    {
        if (currentLevelController)
        {
            currentLevelController.OnLevelCompletedSuccessfully -= GiveRandomFood;
        }
        currentLevelController = FindObjectOfType<LevelController>();

        OnCurrentLevelControllerChanged?.Invoke(currentLevelController);

        currentLevelController.OnLevelCompletedSuccessfully += GiveRandomFood;
        switch (level)
        {
            case 1:
            case 2:
                List<Food> foods = CreateOlives();
                currentLevelController.Init(foods);
                currentLevelController.StartLevel();
                break;
            case 3:
                currentLevelController.Init(null);
                currentLevelController.StartLevel();
                break;
            case 4:
                currentLevelController.Init(null);
                FindObjectOfType<KillVolume>().OnPlayerDie += RestartLevel;
                currentLevelController.StartLevel();
                break;
            default:
                break;
        }

        UIManager.Instance.OnRestartButton += RestartLevel;
        UIManager.Instance.OnNextLevelButton += LoadNextLevel;
        UIManager.Instance.OnMainMenuButton += OnMainMenuButton;
    }

    private void OnMainMenuButton()
    {
        LoadLevel(0);
    }

    public void OnQuitButtonClicked()
    {
        Application.Quit();
    }
    public void RestartLevel()
    {
        LoadLevel(SceneManager.GetActiveScene().buildIndex);
    }

    public void GiveRandomFood(int completedLevel)
    {
        string randomFoodName = foodNames[UnityEngine.Random.Range(0, foodNames.Count)];
        randomFood = Instantiate(foodPrefabsDictionary[randomFoodName], new Vector3(0, -1, 0), Quaternion.identity);
        Collider[] colliders = randomFood.GetComponentsInChildren<Collider>();
        Destroy(randomFood.GetComponentInChildren<Rigidbody>());
        int len = colliders.Length;
        for(int i = 0; i < len; i++)
        {
            Destroy(colliders[i]);
        }
        if (obtainedFoods.ContainsKey(randomFoodName))
        {
            obtainedFoods[randomFoodName]++;
        }
        else
        {
            obtainedFoods.Add(randomFoodName, 1);
        }

        StartCoroutine(MoveFoodToCameraRoutine(randomFood));
        //LoadLevel(++completedLevel);
    }

    private IEnumerator MoveFoodToCameraRoutine(GameObject food)
    {
        while (food != null && Vector3.Distance(food.transform.position, Camera.main.transform.position) > 5f)
        {
            food.transform.position = Vector3.Lerp(food.transform.position, Camera.main.transform.position, 0.01f);
            food.transform.eulerAngles += new Vector3(0, 20, 0);
            yield return new WaitForSeconds(0.02f);
        }
    }

    #region Common functions for Level_1 and Level_2 (Used for olive spawning)
    public List<Food> CreateOlives()
    {
        List<Food> olives = new List<Food>();
        for (int i = 0; i < currentLevelController.OliveCount; i++)
        {
            olives.Add(Instantiate(foodPrefabsDictionary["Olive"], GetRandomPointInsidePlayground(), Quaternion.identity).GetComponentInChildren<Food>());
        }
        return olives;
    }

    private Vector3 GetRandomPointInsidePlayground()
    {
        Vector3 result = GetRandomPoint_Inner();

        if (currentLevelController.ShouldNotCollideWhenSpawning)
        {
            bool bHasFoundAvailablePositionToSpawn = IsPositionAvailable(result);
            while (bHasFoundAvailablePositionToSpawn == false)
            {
                result = GetRandomPoint_Inner();
                bHasFoundAvailablePositionToSpawn = IsPositionAvailable(result);
            }
        }
        return result;
    }


    private Vector3 GetRandomPoint_Inner()
    {
        Vector3 result = new Vector3();

        BoxCollider spawnableAreaCollider = currentLevelController.spawnableAreaCollider;

        result.x = UnityEngine.Random.Range(spawnableAreaCollider.bounds.min.x, spawnableAreaCollider.bounds.max.x);
        result.y = 0.02f;
        result.z = UnityEngine.Random.Range(spawnableAreaCollider.bounds.min.z, spawnableAreaCollider.bounds.max.z);

        return result;
    }


    private bool IsPositionAvailable(Vector3 position)
    {
        return !Physics.CheckSphere(position, oliveCollider.radius + 0.1f, foodLayer);
    }
    #endregion
}
