using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICollectible
{
    CollectableType collectableType { get; }
    void EnterCollectionArea();
    void ExitCollectionArea();
}

public enum CollectableType
{
    Olive
}