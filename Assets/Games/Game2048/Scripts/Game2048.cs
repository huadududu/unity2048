using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game2048 : Architecture<Game2048>
{
    protected override void Init()
    {
        Register(new GameModel());
    }
}
