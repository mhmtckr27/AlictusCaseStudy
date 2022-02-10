using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Level_1_Controller : LevelController
{
    [Space] [Header("COUNTS")]
    [SerializeField] private int explodeParticleCount;


    [Space] [Header("BOOLEANS")]
    [SerializeField] private bool shouldNotCollideWhenSpawning;

    private int collectedOliveCount;

    public override bool ShouldNotCollideWhenSpawning { get => shouldNotCollideWhenSpawning; }
    public override int Level { get => 1; }
    public override event Action<float> OnProgress;

    public override void Init(List<Food> foods)
    {
        this.foods = foods;
        collectedOliveCount = 0;
        explodeParticleCount = Mathf.Clamp(explodeParticleCount, 0, oliveCount);
        player = Instantiate(playerPrefab).GetComponent<Player>();
        Instantiate(playerFoodPrefab, player.transform);
        player.Init(FoodType.Banana);
    }

    public override void StartLevel()
    {
        player.inputManager.EnableInput(true);
    }

    private void OnEnable()
    {
        CollectionArea.Instance.onEnterCollectionArea += OnObjectEnterCollectionArea;
        CollectionArea.Instance.onExitCollectionArea += OnObjectExitCollectionArea;
    }

    private void OnDisable()
    {
        CollectionArea.Instance.onEnterCollectionArea -= OnObjectEnterCollectionArea;
        CollectionArea.Instance.onExitCollectionArea -= OnObjectExitCollectionArea;
    }

    private void OnObjectEnterCollectionArea(ICollectible collectible)
    {
        collectedOliveCount++;
        OnProgress?.Invoke((float)1 / oliveCount);
        if (collectedOliveCount == foods.Count)
        {
            //GAMEOVER
            StartCoroutine(GameOverRoutine());
        }
    }

    private void OnObjectExitCollectionArea(ICollectible collectible)
    {
        collectedOliveCount--;
        OnProgress?.Invoke((float)-1 / oliveCount);
    }

    private IEnumerator GameOverRoutine()
    {
        List<Vector3> positions = GetRandomOlivesForParticleCreation();
        for (int i = 0; i < positions.Count; i++)
        {
            Instantiate(explodeParticlePrefab, positions[i], Quaternion.identity);
        }
        yield return new WaitForSeconds(explodeParticlePrefab.GetComponent<ParticleSystem>().main.startLifetime.constant);
        CompleteLevel();
    }

    private List<Vector3> GetRandomOlivesForParticleCreation()
    {
        List<Vector3> positions = new List<Vector3>();
        List<int> selectedOliveIndices = new List<int>();

        for(int i = 0; i < oliveCount; i++)
        {
            selectedOliveIndices.Add(i);
        }

        for(int i = 0; i < explodeParticleCount; i++)
        {
            int rand = UnityEngine.Random.Range(0, selectedOliveIndices.Count);
            positions.Add(foods[selectedOliveIndices[rand]].transform.position);
            selectedOliveIndices.RemoveAt(rand);
        }

        return positions;
    }
}
