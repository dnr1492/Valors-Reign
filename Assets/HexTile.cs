using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexTile : MonoBehaviour
{
    public (int col, int row) GridPosition { get; private set; }
    public Nullable<int> AssignedTokenKey { get; private set; } = null;

    public void Init((int, int) pos)
    {
        GridPosition = pos;
    }

    public void AssignToken(int tokenKey)
    {
        AssignedTokenKey = tokenKey;
    }

    public void ClearToken()
    {
        AssignedTokenKey = null;
    }
}
