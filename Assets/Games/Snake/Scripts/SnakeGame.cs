using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnakeGame : Architecture<SnakeGame>
{
    protected override void Init()
    {
        Register(new SnakeModel());
    }
}
