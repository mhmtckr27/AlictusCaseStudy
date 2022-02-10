using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ObtainedCanvas : MonoBehaviour
{
    [SerializeField] private GameObject backgroundImage;
    [SerializeField] private GameObject amountText;

    private static ObtainedCanvas instance;
    public static ObtainedCanvas Instance { get => instance; }

    new BoxCollider collider;
    List<GameObject> endLevelScreenObjects;
    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        else if(instance != this)
        {
            Destroy(gameObject);
        }

        DontDestroyOnLoad(gameObject);
    }


    // Start is called before the first frame update
    void Start()
    {
        collider = backgroundImage.GetComponent<BoxCollider>();
        GameManager.Instance.OnCurrentLevelControllerChanged += Instance_OnCurrentLevelControllerChanged;
    }

    private void Instance_OnCurrentLevelControllerChanged(LevelController obj)
    {
        GetComponent<Canvas>().worldCamera = Camera.main;
        if (obj != null)
        {
            endLevelScreenObjects = new List<GameObject>();
            backgroundImage.SetActive(false);
            GameManager.Instance.currentLevelController.OnLevelCompletedSuccessfully += CurrentLevelController_OnLevelCompletedSuccessfully;
            UIManager.Instance.OnProceedButton += OnProceedButton;
            UIManager.Instance.OnNextLevelButton += OnNextLevelButton;
            UIManager.Instance.OnInventoryButton += OnInventoryButton;
        }
    }

    private void OnInventoryButton(bool enableInventory)
    {
        if (enableInventory)
        {
            backgroundImage.SetActive(true);
            OnProceedButton();
        }
        else
        {
            StopAllCoroutines();
            DestroyEndLevelScreenObjects();
            backgroundImage.SetActive(false);
        }
    }

    private void OnNextLevelButton()
    {
        DestroyEndLevelScreenObjects();
    }

    private void DestroyEndLevelScreenObjects()
    {
        int len = endLevelScreenObjects.Count;
        for (int i = 0; i < len; i++)
        {
            Destroy(endLevelScreenObjects[i]);
        }
    }

    private void CurrentLevelController_OnLevelCompletedSuccessfully(int obj)
    {
        backgroundImage.SetActive(true);
    }

    public void OnProceedButton()
    {
        float columnStartPos = collider.bounds.min.x + 0.25f;
        float rowStartPos = collider.bounds.min.z + 0.25f;
        float columnEndPos = collider.bounds.max.x - 0.25f;
        float rowEndPos = collider.bounds.max.z - 0.25f;

        for (int i = 0; i < GameManager.Instance.obtainedFoods.Count; i++)
        {
            for (int j = 0; j < 2; j++)
            {
                Vector3 spawnPos = new Vector3();
                spawnPos.x = Mathf.Lerp(columnStartPos, columnEndPos, (float)j / (2 - 1));
                spawnPos.y = collider.bounds.max.y + 1;
                spawnPos.z = Mathf.Lerp(rowStartPos, rowEndPos, (float)i / (5 - 1));

                if (j == 0)
                {
                    GameObject toInstantiate = GameManager.Instance.foodPrefabsDictionary[GameManager.Instance.obtainedFoods.Keys.ElementAt(i)];
                    GameObject temp = Instantiate(toInstantiate, spawnPos, Quaternion.identity);
                    endLevelScreenObjects.Add(temp);
                    Collider[] colliders = temp.GetComponentsInChildren<Collider>();
                    Destroy(temp.GetComponentInChildren<Rigidbody>());
                    int len = colliders.Length;
                    for (int k = 0; k < len; k++)
                    {
                        Destroy(colliders[k]);
                    }
                    StartCoroutine(RotateRoutine(temp.transform));
                }
                else
                {
                    Text text = Instantiate(amountText, spawnPos, Quaternion.identity, transform).GetComponent<Text>();
                    endLevelScreenObjects.Add(text.gameObject);
                    text.text = "x" + GameManager.Instance.obtainedFoods[GameManager.Instance.obtainedFoods.Keys.ElementAt(i)];
                    text.GetComponent<RectTransform>().localRotation = Quaternion.Euler(Vector3.zero);
                }
            }
        }
    }

    private IEnumerator RotateRoutine(Transform foodObj)
    {
        while (foodObj != null)
        {
                    Debug.LogWarning("GIRDI");
            yield return new WaitForSeconds(0.02f);
            if (foodObj != null)
            {
                foodObj.eulerAngles += new Vector3(0, 8, 0);
            }
        }
        yield return null;
    }
}
