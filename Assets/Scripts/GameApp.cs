using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameApp:Architecture<GameApp>
{
    protected override void Init()
    {
        Register(new GameModel());
    }
}
