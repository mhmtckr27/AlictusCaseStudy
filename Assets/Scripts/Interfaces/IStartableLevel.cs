using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IStartableLevel
{
    int Level { get; }

    void Init(List<Food> foods);
    void StartLevel();
}
