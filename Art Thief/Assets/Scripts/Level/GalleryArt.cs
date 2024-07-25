using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GalleryArt : MonoBehaviour
{
    [SerializeField]
    private MeshRenderer targetMesh;

    public MeshRenderer TargetMesh => targetMesh;

    [SerializeField]
    private int targetMaterialIndex;

    [SerializeField]
    private Material woodReplaceMaterial;

    [SerializeField]
    private bool shouldTakeObject;

    public bool ShouldTakeObject => shouldTakeObject;

    public void RemoveArtImage()
    {
        var materials = targetMesh.materials;
        materials[targetMaterialIndex] = woodReplaceMaterial;
        targetMesh.materials = materials;
    }
}
