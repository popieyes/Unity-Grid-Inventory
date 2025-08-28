using UnityEngine;
using GridSelector;

public class GridManager : MonoBehaviour
{
    [Header("Grid Parameters")]
    [SerializeField] private Vector2Int _gridSize;
    [Header("References")]
    [SerializeField] private SpriteRenderer _cellsRenderer;
    [SerializeField] private Selector _selector;
    [SerializeField] private Transform _itemsContainer;
    [Header("Item List")]
    [SerializeField] private GameObject[] _items;
    [Header("Debug Parameters")]
    [SerializeField] private bool _debug = true;
    private Grid<Item> _grid;
    public Grid<Item> Grid => _grid;
    public Vector2Int Size => _gridSize;
    public Transform ItemContainer => _itemsContainer;

    private Vector3 _initialDebugPoint;
    #region ---- INITIALIZERS ----
    private void InitGrid()
    {
        _grid = new Grid<Item>(_gridSize.x, _gridSize.y);
    }
    private void InitGridVisual()
    {
        _cellsRenderer.size = new Vector2(_gridSize.x, _gridSize.y);
    }
    private void InitSelector()
    {
        _selector.SetSelectorPosition(Vector2.zero);
        _selector.CheckSelectorSize();
    }

    private void InitItems()
    {
        foreach (GameObject item in _items)
        {
            GameObject itemGO = Instantiate(item, _itemsContainer);
            Item itemComponent = itemGO.GetComponent<Item>();
            if (!CheckGrid(itemComponent, out Vector2Int freeSlot))
            {
                itemGO.SetActive(false);
                Debug.Log($"{itemGO} is too big for the grid or grid is already full.");
                continue;
            }
            itemComponent.Init(this, freeSlot);
            FillGrid(itemComponent);
        }
    }
    private void InitDebugPoint() => _initialDebugPoint = transform.position;
    #endregion
    #region ---- UNITY CALLBACKS ----
    private void Awake()
    {
        InitGrid();
        InitGridVisual();
        InitItems();
        InitSelector();
        InitDebugPoint();
    }

    private void OnDrawGizmos()
    {
        DebugGrid();
    }
    #endregion

    #region ---- ITEM MANAGING ----
    public void FillGrid(Item item)
    {
        for (int row = -item.GridPos.y; row <= -item.GridPos.y + item.Size.y - 1; row++)
        {
            for (int col = item.GridPos.x; col <= item.GridPos.x + item.Size.x - 1; col++)
            {
                _grid.Array[col, row] = item;
            }
        }
    }

    public void FreeGrid(Item item)
    {
        for (int row = -item.GridPos.y; row <= -item.GridPos.y + item.Size.y - 1; row++)
        {
            for (int col = item.GridPos.x; col <= item.GridPos.x + item.Size.x - 1; col++)
            {
                _grid.Array[col, row] = null;
            }
        }
    }

    public bool CheckGrid(Item item, out Vector2Int freeSlot)
    {
        var itemSize = item.Size;
        freeSlot = Vector2Int.zero;

        for (int row = 0; row <= _gridSize.y - itemSize.y; row++)
        {
            for (int col = 0; col <= _gridSize.x - itemSize.x; col++)
            {
                // If occuppied, jump the size
                if (_grid.Array[col, row] != null)
                {
                    col += _grid.Array[col, row].Size.x - 1;
                    continue;

                }
                bool canPlace = true;
                // If not check the size of the item inside the grid

                for (int itemRow = row; itemRow < row + itemSize.y; itemRow++)
                {
                    for (int itemCol = col; itemCol < col + itemSize.x; itemCol++)
                    {
                        if (itemCol >= _gridSize.x || itemRow >= _gridSize.y || _grid.Array[itemCol, itemRow] != null)
                        {
                            canPlace = false;
                            break;
                        }
                    }
                    if (!canPlace) break;
                }

              
                
                
                if (canPlace)
                {
                    freeSlot = new Vector2Int(col, -row);
                    return true;
                }
            }
        }
        
        Vector2Int rotatedSize = new Vector2Int(itemSize.y, itemSize.x);
        for (int row = 0; row <= _gridSize.y - rotatedSize.y; row++)
        {
            for (int col = 0; col <= _gridSize.x - rotatedSize.x; col++)
            {
                // If occuppied, jump the size
                if (_grid.Array[col, row] != null)
                {
                    col += _grid.Array[col, row].Size.x - 1;
                    continue;

                }
                bool canPlace = true;
                // If not check the size of the item inside the grid

                for (int itemRow = row; itemRow < row + rotatedSize.y; itemRow++)
                {
                    for (int itemCol = col; itemCol < col + rotatedSize.x; itemCol++)
                    {
                        if (itemCol >= _gridSize.x || itemRow >= _gridSize.y || _grid.Array[itemCol, itemRow] != null)
                        {
                            canPlace = false;
                            break;
                        }
                    }
                    if (!canPlace) break;
                }

                if (canPlace)
                {
                    item.Rotate();
                    freeSlot = new Vector2Int(col, -row);
                    return true;
                }
            }
        }
        return false;  
    }

    public bool CheckGrid(Vector3 initialPosition, Vector2Int itemSize)
    {
        var currentPos = new Vector2Int((int)initialPosition.x, -(int)initialPosition.y);
        for (int row = currentPos.y; row < currentPos.y + itemSize.y ; row++)
        {
            for (int col = currentPos.x; col < currentPos.x + itemSize.x; col++)
            {
                if (col >= _gridSize.x || row >= _gridSize.y || _grid.Array[col, row] != null)
                    return false;
            }
        }
        return true;
    }

    private void DebugGrid()
    {
        if (_grid == null) return;
        if (!_debug) return;

        for (int x = 0; x < _grid.Array.GetLength(0); x++)
        {
            for (int y = 0; y < _grid.Array.GetLength(1); y++)
            {
                var hasItem = _grid.Array[x, y] != null;
                var xScaledValue = x * transform.localScale.x;
                var yScaledValue = y * -transform.localScale.y;

                Gizmos.color = hasItem ? Color.red : Color.green;
                Gizmos.DrawSphere(new Vector2(xScaledValue + _initialDebugPoint.x,
                yScaledValue + _initialDebugPoint.y), .005f);
            }
        }
    }
    #endregion
}
