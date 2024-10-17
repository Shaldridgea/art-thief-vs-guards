using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface INodeGuid
{
    void SetGuid(System.Guid newGuid);

    System.Guid GetGuid();
}
