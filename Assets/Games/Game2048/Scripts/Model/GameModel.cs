using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameModel
{
    public BindableProperty<int> Score = new BindableProperty<int>()
    {
        Value = 0
    };
    public BindableProperty<int> BestScore = new BindableProperty<int>()
    {
        Value = 0
    };
    public BindableProperty<int> MaxTileIndex = new BindableProperty<int>()
    {
        Value = 2
    };
}
