using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class HelperExtensions
{
    public static Vector3 ZeroY(this Vector3 changeVector) => new Vector3(changeVector.x, 0f, changeVector.z);
}
