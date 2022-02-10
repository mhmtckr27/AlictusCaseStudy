using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] private Button restartButton;
    [SerializeField] public Button inventoryButton;
    [SerializeField] private Button proceedButton;
    [SerializeField] private Button nextLevelButton;
    [SerializeField] private Slider progressBar;
    [SerializeField] private Text levelSuccessText;

    public event Action OnRestartButton;
    public event Action<bool> OnInventoryButton;
    public event Action OnProceedButton;
    public event Action OnNextLevelButton;
    public event Action OnMainMenuButton;

    #region singleton
    private static UIManager instance;
    public static UIManager Instance { get => instance; }
    #endregion

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

    }

    private void OnEnable()
    {
        GameManager.Instance.OnCurrentLevelControllerChanged += OnCurrentLevelControllerChanged;
    }
    private void OnDisable()
    {
        GameManager.Instance.OnCurrentLevelControllerChanged -= OnCurrentLevelControllerChanged;
    }

    private void OnCurrentLevelControllerChanged(LevelController levelController)
    {
        if (levelController != null)
        {
            proceedButton.gameObject.SetActive(false);
            levelSuccessText.gameObject.SetActive(false);
            progressBar.gameObject.SetActive(true);
            restartButton.gameObject.SetActive(true);
            inventoryButton.gameObject.SetActive(true);

            levelController.OnProgress += OnLevelProgress;
            levelController.OnLevelCompletedSuccessfully += LevelController_OnLevelCompletedSuccessfully;
        }
    }

    private void LevelController_OnLevelCompletedSuccessfully(int obj)
    {
        proceedButton.gameObject.SetActive(true);
        levelSuccessText.text = "You Won Level " + obj + "!";
        levelSuccessText.gameObject.SetActive(true);
        progressBar.gameObject.SetActive(false);
        restartButton.gameObject.SetActive(false);
        inventoryButton.gameObject.SetActive(false);
    }

    private void OnLevelProgress(float newProgress)
    {
        progressBar.value += newProgress;
    }

    public void OnRestartButtonClicked()
    {
        OnRestartButton?.Invoke();
    }

    public void OnInventoryButtonClicked()
    {
        bool enableInventory = progressBar.gameObject.activeInHierarchy;

        progressBar.gameObject.SetActive(!enableInventory);
        restartButton.gameObject.SetActive(!enableInventory);
        OnInventoryButton?.Invoke(enableInventory);
    }

    public void OnProceedButtonClicked()
    {
        Destroy(GameManager.Instance.randomFood);
        nextLevelButton.gameObject.SetActive(true);
        proceedButton.gameObject.SetActive(false);
        OnProceedButton?.Invoke();
    }

    public void OnNextLevelButtonClicked()
    {
        OnNextLevelButton?.Invoke();
    }

    public void OnMainMenuButtonClicked()
    {
        OnMainMenuButton?.Invoke();
    }

}
