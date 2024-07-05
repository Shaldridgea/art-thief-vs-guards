using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisualInterest : SenseInterest
{
    private int litCount;

    public bool IsLitUp {
        get => litCount > 0;
        set {
            if (value)
                ++litCount;
            else
                --litCount;
        }
    }

}
