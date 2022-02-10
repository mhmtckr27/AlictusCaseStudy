using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

abstract public class LevelController : MonoBehaviour, IStartableLevel
{
    [Header("PREFABS")]
    [SerializeField] protected GameObject playerPrefab;
    [SerializeField] protected GameObject playerFoodPrefab;
    [SerializeField] public BoxCollider bordersMovableArea;
    [SerializeField] protected GameObject explodeParticlePrefab;

    [Header("COUNTS")]
    [SerializeField] protected int oliveCount;

    [Header("COMPONENTS")]
    [SerializeField] public BoxCollider spawnableAreaCollider;

    public event Action<int> OnLevelCompletedSuccessfully;

    public virtual event Action<float> OnProgress;

    abstract public int Level { get; }

    public int OliveCount { get => oliveCount; }

    abstract public bool ShouldNotCollideWhenSpawning { get; }

    protected List<Food> foods;

    protected Player player;

    public virtual void StartLevel()
    {
    }

    public virtual void Init(List<Food> foods)
    {
    }


    public void CompleteLevel()
    {
        Debug.Log("GAME OVER YOU WON LEVEL " + Level + "!");
        OnLevelCompletedSuccessfully?.Invoke(Level);
        Destroy(gameObject);
    }

    private static LevelController instance;
    public static LevelController Instance;
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
    }
}
