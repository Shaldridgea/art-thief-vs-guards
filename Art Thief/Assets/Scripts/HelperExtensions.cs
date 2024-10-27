using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class HelperExtensions
{
    /// <summary>
    /// Returns the vector with its Y component set to zero. Useful for removing height from distance checks
    /// </summary>
    public static Vector3 ZeroY(this Vector3 changeVector) => new Vector3(changeVector.x, 0f, changeVector.z);
}
