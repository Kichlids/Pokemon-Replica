using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Move
{
    public MoveBase _base { get; set; }
    public int PP { get; set; }

    public Move(MoveBase pBase) {
        _base = pBase;
        PP = pBase.PP;
    }
}
