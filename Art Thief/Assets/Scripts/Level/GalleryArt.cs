using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

public class GalleryArt : MonoBehaviour
{
    [SerializeField]
    private MeshRenderer targetMesh;

    public MeshRenderer TargetMesh => targetMesh;

    [SerializeField]
    private int targetMaterialIndex;

    [SerializeField]
    private bool shouldTakeObject;

    [SerializeField]
    [HideIf("shouldTakeObject")]
    private Material woodReplaceMaterial;

    public bool ShouldTakeObject => shouldTakeObject;

    public void RemoveArtImage()
    {
        // Replace our targeted painting image with its wood texture
        var materials = targetMesh.materials;
        materials[targetMaterialIndex] = woodReplaceMaterial;
        targetMesh.materials = materials;
    }
}
