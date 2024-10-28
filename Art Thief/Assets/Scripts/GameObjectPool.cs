using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Pooling class for GameObjects that have the passed component type
/// </summary>
public class GameObjectPool<T>
{
    private List<T> returnList;

    private List<GameObject> objectList;

    private GameObject templateObject;

    private int getIndex;

    public GameObjectPool(GameObject newTemplate, int poolSizeEstimate = 10)
    {
        templateObject = newTemplate;
        returnList = new(poolSizeEstimate);
        objectList = new(poolSizeEstimate);

        for (int i = 0; i < poolSizeEstimate; ++i)
        {
            GameObject newObject = GameObject.Instantiate(templateObject);
            newObject.SetActive(false);
            newObject.transform.SetAsLastSibling();
            returnList.Add(newObject.GetComponent<T>());
            objectList.Add(newObject);
        }
    }

    public T GetFromPool(bool dontSetActive = false)
    {
        T returnTarget;
        GameObject holdingObject;
        if (getIndex >= returnList.Count)
        {
            holdingObject = GameObject.Instantiate(templateObject);
            objectList.Add(holdingObject);

            returnTarget = holdingObject.GetComponent<T>();
            returnList.Add(returnTarget);

            getIndex = returnList.Count - 1;
        }
        else
        {
            returnTarget = returnList[getIndex];
            holdingObject = objectList[getIndex];
        }

        if (!dontSetActive)
        {
            if (!holdingObject.activeSelf)
            {
                holdingObject.SetActive(true);
            }
        }

        ++getIndex;
        return returnTarget;
    }

    public void ReturnToPool(GameObject returnedObject)
    {
        int index = objectList.FindIndex(0, (f) => f == returnedObject);
        if (index < 0)
            return;

        objectList.RemoveAt(index);
        T returnComponent = returnList[index];
        returnList.RemoveAt(index);

        if (getIndex >= objectList.Count)
            getIndex = objectList.Count - 1;

        objectList.Insert(getIndex, returnedObject);
        returnList.Insert(getIndex, returnComponent);
        returnedObject.SetActive(false);
    }

    /// <summary>
    /// Reset the internal counter for getting from the pool, starting from the top again
    /// </summary>
    public void ResetCounter()
    {
        getIndex = 0;
    }

    public void DeactivateUnused()
    {
        if (getIndex >= objectList.Count)
            return;

        for (int i = getIndex; i < objectList.Count; ++i)
            objectList[i].SetActive(false);
    }

    public void ResetAll()
    {
        ResetCounter();
        DeactivateUnused();
    }
}
