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

            itemComponent.Init(freeSlot);
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
    public void FillGrid(Item item) => RunGridSegment(item.GridPos, item.Size, item);

    public void FreeGrid(Item item) => RunGridSegment(item.GridPos, item.Size);

    #endregion
    #region ---- GRID CHECKING ----
    private void RunGridSegment(Vector2Int itemPos, Vector2Int itemSize, Item value = null)
    {
        for (int row = -itemPos.y; row < -itemPos.y + itemSize.y; row++)
        {
            for (int col = itemPos.x; col < itemPos.x + itemSize.x; col++)
            {
                _grid.Array[col, row] = value;
            }
        }
    }

    public bool CheckGrid(Item item, out Vector2Int freeSlot)
    {
        var itemSize = item.Size;

        // Check horizontal orientation
        if (ScanForFreeSlot(itemSize, out freeSlot))
            return true;

        // Check vertical orientation
        itemSize = new Vector2Int(itemSize.y, itemSize.x);
        // If it fits vertically, rotate the item
        if (ScanForFreeSlot(itemSize, out freeSlot))
        {
            item.Rotate();
            return true;
        }

        return false;
    }
    public bool CheckGrid(Vector2Int initialPos, Vector2Int itemSize)
    {
        for (int row = -initialPos.y; row < -initialPos.y + itemSize.y; row++)
        {
            for (int col = initialPos.x; col < initialPos.x + itemSize.x; col++)
            {
                if (col >= _gridSize.x || row >= _gridSize.y || _grid.Array[col, row] != null)
                    return false;
            }
        }
        return true;
    }

    private bool ScanForFreeSlot(Vector2Int itemSize, out Vector2Int freeSlot)
    {
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

                // If not check the size of the item inside the grid
                bool canPlace = CheckGrid(new Vector2Int(col, -row), itemSize);

                if (canPlace)
                {
                    freeSlot = new Vector2Int(col, -row);
                    return true;
                }
            }
        }
        return false;
    }

    #endregion
    #region ---- DEBUGGING ----
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
