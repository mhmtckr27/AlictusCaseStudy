using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectionArea : MonoBehaviour
{
    public event Action<ICollectible> onEnterCollectionArea;
    public event Action<ICollectible> onExitCollectionArea;

    #region Singleton
    private static CollectionArea instance;
    public static CollectionArea Instance { get => instance; }
    #endregion

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


    private void OnTriggerEnter(Collider other)
    {
        ICollectible collectible = other.GetComponent<ICollectible>();
        if (collectible != null)
        {
            onEnterCollectionArea?.Invoke(collectible);
            collectible.EnterCollectionArea();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        ICollectible collectible = other.GetComponent<ICollectible>();
        if (collectible != null)
        {
            onExitCollectionArea?.Invoke(collectible);
            collectible.ExitCollectionArea();
        }
    }
}
