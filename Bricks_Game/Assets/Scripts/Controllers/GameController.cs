using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameMode
{
    Add, Remove
}

[System.Serializable]
public struct GridVector
{
    public int x, z;

    public GridVector(int x, int z)
    {
        this.x = x;
        this.z = z;
    }

    public GridVector Add(int x, int z)
    {
        return new GridVector(this.x + x, this.z + z);
    }

    public bool IsCorrect()
    {
        return x >= 0 && z >= 0;
    }

    public static GridVector None()
    {
        return new GridVector(-1, -1);
    }
}

public class GameController : MonoBehaviour
{
    public GridCellsContainer cellsContainerSample;

    [Space]
    public int gridSize = 10;

    [HideInInspector] public GameMode mode;
    
    bool allowCast = false;
    int rotationIndex = 0;

    Camera mainCamera;
    Transform bricksParent;

    GridCell[,] gridCells;

    const int rotationMaxIndex = 3;
    const float rotationAngleStep = 90f;

    public static GameController Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
        mode = GameMode.Add;
    }

    void Start()
    {
        mainCamera = Camera.main;

        DrawGrid();

        allowCast = true;
    }

    public void DrawGrid()
    {
        bricksParent = new GameObject("Bricks").transform;
        bricksParent.transform.SetParent(cellsContainerSample.transform.parent.parent);
        bricksParent.transform.localPosition = Vector3.zero;

        gridCells = new GridCell[gridSize * cellsContainerSample.size.z, gridSize * cellsContainerSample.size.x];

        float brickHeight = GetBrickPrefab().meshRenderer.bounds.size.y;
        float cellSize = cellsContainerSample.spriteRenderer.bounds.size.x;
        float gridWorldSize = gridSize * cellSize;
        float startCoord = -(gridWorldSize / 2f) + (cellSize / 2f);
        Vector3 currentPosition = new Vector3(startCoord, -(brickHeight / 2f), startCoord);
        int zIndex = 0, xIndex;
        for (int containerZIndex = 0; containerZIndex < gridSize; containerZIndex++)
        {
            currentPosition.x = startCoord;
            xIndex = 0;
            for (int containerXIndex = 0; containerXIndex < gridSize; containerXIndex++)
            {
                GridCellsContainer newCellsContainer = Instantiate(cellsContainerSample, Vector3.zero, cellsContainerSample.transform.rotation, cellsContainerSample.transform.parent);
                newCellsContainer.transform.localPosition = currentPosition;
                newCellsContainer.name = "Container " + containerZIndex.ToString() + " " + containerXIndex.ToString();

                newCellsContainer.CreateCells(zIndex, xIndex);
                GridCell[] _cells = newCellsContainer.cells;
                for (int k = 0; k < _cells.Length; k++)
                {
                    GridVector pos = _cells[k].gridPosition;
                    gridCells[pos.z, pos.x] = _cells[k];
                }
                xIndex += newCellsContainer.size.x;

                newCellsContainer.gameObject.SetActive(true);

                currentPosition.x += 1f;
            }

            zIndex += cellsContainerSample.size.z;

            currentPosition.z += 1f;
        }

        Destroy(cellsContainerSample.gameObject);
        cellsContainerSample = null;
    }

    private void Update()
    {
        if (allowCast && InputController.Instance.TouchedSingleStationary() && InputController.Instance.TouchIsOverControlPanel())
        {
            Ray ray = mainCamera.ScreenPointToRay(InputController.GetTouchPosition());
            if(Physics.Raycast(ray, out RaycastHit raycastHit))
            {
                GridCell gridCell = raycastHit.collider.GetComponent<GridCell>();
                if(gridCell != null)
                {
                    if (mode == GameMode.Add)
                    {
                        //if(gridCell.container.size.z == GetBrickPrefab().size.z)
                        //{
                        //    gridCell = gridCell.container.cells[0];
                        //}

                        if (CanPlaceBrick(gridCell.gridPosition, GetPrefabSizeWithRotation(), 0))
                        {
                            Brick newBrick = gridCell.AddBrickOnOrder(0);
                            AddBrickOnCells(newBrick);
                        }
                    }
                }
                else
                {
                    Brick brick = raycastHit.collider.GetComponent<Brick>();

                    if(brick != null)
                    {
                        if (mode == GameMode.Add)
                        {
                            GridVector brickPrefabSize = GetPrefabSizeWithRotation();

                            Vector3 normal = raycastHit.normal;
                            if (normal.y > 0f)
                            {
                                if (CanPlaceBrick(brick.ownerCell.gridPosition, brickPrefabSize, brick.orderIndex + 1))
                                {
                                    Brick newBrick = brick.ownerCell.AddBrickOnOrder(brick.orderIndex + 1);
                                    AddBrickOnCells(newBrick);
                                }
                            }
                            else if(normal.y == 0f)
                            {
                                GridVector gridPosition = GetSiblingPosition(brick.ownerCell.gridPosition, brick.orderIndex, brickPrefabSize, normal);
                                if(CanPlaceBrick(gridPosition, brickPrefabSize, brick.orderIndex))
                                {
                                    GridCell siblingCell = GetCell(gridPosition);
                                    Brick newBrick = siblingCell.AddBrickOnOrder(brick.orderIndex);
                                    AddBrickOnCells(newBrick);
                                    ////////////////////////////////////////////////////////////////////////////////////
                                }
                            }
                        }
                        else if(mode == GameMode.Remove)
                        {
                            brick.Remove();
                        }
                    }
                }
            }
        }
    }

    void AddBrickOnCells(Brick brick)
    {
        for (int z = 0; z < brick.size.z; z++)
        {
            for (int x = 0; x < brick.size.x; x++)
            {
                if(x == 0 && z == 0)
                {
                    continue;
                }

                GridVector cellPos = brick.ownerCell.gridPosition.Add(x, z);
                GetCell(cellPos).AddBrickOnOrder(brick.orderIndex, brick);
            }
        }
    }

    bool CanPlaceBrick(GridVector startCellPosition, GridVector size, int orderIndex)
    {
        for (int z = 0; z <size.z; z++)
        {
            for (int x = 0; x < size.x; x++)
            {
                GridVector cellPos = startCellPosition.Add(x, z);
                if (!IsFreeCell(cellPos, orderIndex))
                {
                    return false;
                }
            }
        }

        return true;
    }

    bool IsFreeCell(GridVector position, int orderIndex)
    {
        if (!GridPositionIsCorrect(position))
        {
            return false;
        }

        return GetCell(position).IsFree(orderIndex);
    }

    GridCell GetCell(GridVector gridPosition)
    {
        return gridCells[gridPosition.z, gridPosition.x];
    }

    GridVector GetSiblingPosition(GridVector gridPosition, int orderIndex, GridVector newBrickSize, Vector3 normalOffset)
    {
        GridVector newPosition = GetSiblingCellPosition(gridPosition, newBrickSize, orderIndex, normalOffset);
        if (!newPosition.IsCorrect())
        {
            return GridVector.None();
        }

        return FindAvailablePosition(gridPosition, orderIndex, newBrickSize, -normalOffset);
    }

    GridVector FindAvailablePosition(GridVector gridPosition, int orderIndex, GridVector newBrickSize, Vector3 normalOffset)
    {
        if (normalOffset.x != 0f)
        {
            int directionX = (int)normalOffset.x;
            bool[] xNotFreeCells = GetBoolArray(newBrickSize.x);
            for (int x = 0; x < newBrickSize.x; x++)
            {
                if (xNotFreeCells[x])
                {
                    continue;
                }

                for (int z = 0; z < newBrickSize.z; z++)
                {
                    GridVector cellPos = gridPosition.Add(x * directionX, z);
                    if (!IsFreeCell(cellPos, orderIndex))
                    {
                        xNotFreeCells[x] = true;
                    }
                }
            }

            gridPosition.x -= GetBoolValuesCount(xNotFreeCells, true) * directionX;
            return gridPosition;
        }
        else if (normalOffset.z != 0f)
        {
            int directionZ = (int)normalOffset.z;
            bool[] zNotFreeCells = GetBoolArray(newBrickSize.z);
            for (int z = 0; z < newBrickSize.z; z++)
            {
                if (zNotFreeCells[z])
                {
                    continue;
                }

                for (int x = 0; x < newBrickSize.x; x++)
                {
                    GridVector cellPos = gridPosition.Add(x * directionZ, z);
                    if (!IsFreeCell(cellPos, orderIndex))
                    {
                        zNotFreeCells[z] = true;
                    }
                }
            }

            gridPosition.z -= GetBoolValuesCount(zNotFreeCells, true) * directionZ;
            return gridPosition;
        }
        else
        {
            return GridVector.None();
        }
    }

    GridVector GetSiblingCellPosition(GridVector gridPosition, GridVector size, int orderIndex, Vector3 normalOffset)
    {
        if (normalOffset.x != 0f)
        {
            int offsetX = (int)normalOffset.x;
            for (int i = 0; i <= size.x; i++)
            {
                gridPosition.x += offsetX;
                if (!GridPositionIsCorrect(gridPosition))
                {
                    return GridVector.None();
                }
                if (IsFreeCell(gridPosition, orderIndex))
                {
                    return gridPosition;
                }
            }

            return GridVector.None();
        }
        else if (normalOffset.z != 0f)
        {
            int offsetZ = (int)normalOffset.z;
            for (int i = 0; i <= size.z; i++)
            {
                gridPosition.z += offsetZ;
                if (!GridPositionIsCorrect(gridPosition))
                {
                    return GridVector.None();
                }
                if (IsFreeCell(gridPosition, orderIndex))
                {
                    return gridPosition;
                }
            }

            return GridVector.None();
        }
        else
        {
            return GridVector.None();
        }

        //int xIndex = gridPosition.x;
        //if(normalOffset.x < 0f)
        //{
        //    xIndex -= newBrickSize.x;
        //}
        //else
        //{
        //    xIndex += ((int)normalOffset.x * brickSize.x);
        //}

        //int zIndex = gridPosition.z;
        //if (normalOffset.z < 0f)
        //{
        //    zIndex -= newBrickSize.z;
        //}
        //else
        //{
        //    zIndex += ((int)normalOffset.z * brickSize.z);
        //}

        //return new GridVector(xIndex, zIndex);
    }

    //int GetNotFreeCellsCount(GridVector startPosition, GridVector size, int orderIndex)
    //{
    //    int count = 0;
    //    for (int z = 0; z < size.z; z++)
    //    {
    //        for (int x = 0; x < size.x; x++)
    //        {
    //            GridVector cellPos = startPosition.Add(x, z);
    //            if (!IsFreeCell(cellPos, orderIndex))
    //            {
    //                count++;
    //            }
    //        }
    //    }

    //    return count;
    //}

    bool GridPositionIsCorrect(GridVector position)
    {
        return IsCorrectPositionX(position.x) && IsCorrectPositionZ(position.z);
    }

    bool IsCorrectPositionZ(int z)
    {
        return z < gridCells.GetLength(0) && z >= 0;
    }


    bool IsCorrectPositionX(int x)
    {
        return x < gridCells.GetLength(1) && x >= 0;
    }

    public Brick CreateBrick()
    {
        Brick brick = Instantiate(GetBrickPrefab(), Vector3.zero, Quaternion.identity, bricksParent);
        /*var _renderer = brick.GetComponent<Renderer>();
        _renderer.material.color = Color.black;*/

        float rotationAngle = GetRotationAngle();
        brick.CreatePositionPoint(rotationAngle);
        Quaternion rotation = GetBrickPrefab().transform.rotation;
        Vector3 eulers = rotation.eulerAngles;
        eulers.y += rotationAngle;
        rotation.eulerAngles = eulers;
        brick.transform.rotation = rotation;
        brick.size = GetSizeWithRotation(brick.size);

        brick.gameObject.SetActive(true);
        return brick;
    }

    Brick GetBrickPrefab()
    {
        return DataController.Instance.GetSelectedBrickPrefab();
    }

    GridVector GetSizeWithRotation(GridVector defaultSize)
    {
        float rotationAngle = GetRotationAngle();

        if (rotationAngle == 90f || rotationAngle == 270f)
        {
            return new GridVector(defaultSize.z, defaultSize.x);
        }
        else
        {
            return defaultSize;
        }
    }

    GridVector GetPrefabSizeWithRotation()
    {
        return GetSizeWithRotation(GetBrickPrefab().size);
    }

    public void NextRotation()
    {
        rotationIndex++;
        if(rotationIndex > rotationMaxIndex)
        {
            rotationIndex = 0;
        }
    }

    public float GetRotationAngle()
    {
        return rotationAngleStep * (float)rotationIndex;
    }

    public void AllowCast(bool v)
    {
        if (v)
        {
            if (!allowCast)
            {
                StartCoroutine(AllowingCast());
            }
        }
        else
        {
            allowCast = false;
        }
    }

    IEnumerator AllowingCast()
    {
        yield return new WaitForEndOfFrame();
        allowCast = true;
    }

    static int GetBoolValuesCount(bool[] array, bool value)
    {
        int count = 0;
        for (int i = 0; i < array.Length; i++)
        {
            if (array[i])
            {
                count++;
            }
        }
        return count;
    }

    static bool[] GetBoolArray(int count)
    {
        bool[] array = new bool[count];
        for (int i = 0; i < array.Length; i++)
        {
            array[i] = false;
        }
        return array;
    }
}
