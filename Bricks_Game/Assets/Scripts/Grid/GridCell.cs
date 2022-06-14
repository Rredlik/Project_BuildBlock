using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridCell : MonoBehaviour
{
    public List<Brick> bricks;

    [HideInInspector] public GridVector gridPosition;
    [HideInInspector] public BoxCollider boxCollider;

    [HideInInspector] public GridCellsContainer container;

    void Awake()
    {
        bricks = new List<Brick>();
    }

    public bool IsFree(int orderIndex)
    {
        return bricks == null || bricks.Count <= orderIndex || bricks[orderIndex] == null;
    }

    public Brick AddBrickOnOrder(int orderIndex)
    {
        Brick brick = GameController.Instance.CreateBrick();
        brick.ownerCell = this;
        brick.orderIndex = orderIndex;

        Vector3 cellSize = boxCollider.size;
        Vector3 cellCornerPosition = transform.position;
        cellCornerPosition.z -= cellSize.z / 2f;
        cellCornerPosition.x -= cellSize.x / 2f;

        Vector3 brickPointOffset = brick.positionPoint.position - brick.transform.position;
        Vector3 position = cellCornerPosition - brickPointOffset;

        Vector3 brickSize = brick.meshRenderer.bounds.size;
        float height = brickSize.y;
        position.y = height * orderIndex;
        brick.transform.position = position;

        AddBrickOnOrder(orderIndex, brick);

        return brick;
    }

    public void AddBrickOnOrder(int orderIndex, Brick brick)
    {
        if (bricks.Count > orderIndex)
        {
            if (bricks[orderIndex] == null)
            {
                bricks[orderIndex] = brick;
            }
            else
            {
                print("brick is already exists");
            }

        }
        else
        {
            for (int i = bricks.Count; i < orderIndex; i++)
            {
                bricks.Add(null);
            }

            bricks.Add(brick);
        }
    }

    public void RemoveBrick(Brick brick, bool destroy = true)
    {
        bricks[brick.orderIndex] = null;
        if (destroy)
        {
            Destroy(brick.gameObject);
        }

        while(bricks.Count > 0)
        {
            if(bricks[bricks.Count - 1] == null)
            {
                bricks.RemoveAt(bricks.Count - 1);
            }
            else
            {
                break;
            }
        }
    }
}
