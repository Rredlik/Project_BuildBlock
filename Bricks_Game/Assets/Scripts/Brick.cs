using System.Collections.Generic;
using UnityEngine;

public class Brick : MonoBehaviour
{
    public GridVector size;

    public MeshRenderer meshRenderer;
    //public Renderer material;
    public Material[] blocksMaterials;

    [HideInInspector] public Transform positionPoint;
    public GridCell ownerCell;
    public List<GridCell> cells;
    [HideInInspector] public int orderIndex;

    public void SetMaterial()
    {
        //Renderer material = GetComponent();
        
    }

    public void CreatePositionPoint(float rotationAngle)
    {
        Vector3 size = meshRenderer.bounds.size;
        float halfSizeX = size.x / 2f;
        float halfSizeZ = size.z / 2f;
        Vector3 pos;
        if(rotationAngle == 0f)
        {
            pos = new Vector3(-halfSizeX, 0f, -halfSizeZ);
        }
        else if (rotationAngle == 90f)
        {
            pos = new Vector3(halfSizeX, 0f, -halfSizeZ);
        }
        else if (rotationAngle == 180f)
        {
            pos = new Vector3(halfSizeX, 0f, halfSizeZ);
        }
        else
        {
            pos = new Vector3(-halfSizeX, 0f, halfSizeZ);
        }
        positionPoint = new GameObject("PositionPoint").transform;
        positionPoint.SetParent(transform);
        positionPoint.rotation = Quaternion.identity;
        positionPoint.localPosition = pos;
    }

    public void Remove()
    {
        if(cells.Count > 0)
        {
            for (int i = 0; i < cells.Count; i++)
            {
                cells[i].RemoveBrick(this, false);
            }

            cells.Clear();
        }

        ownerCell.RemoveBrick(this);
    }
}