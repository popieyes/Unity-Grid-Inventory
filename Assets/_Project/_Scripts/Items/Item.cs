using UnityEngine;
using DG.Tweening;

public class Item : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform _model;
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [Header("Item Parameters")]
    [SerializeField]private Vector2Int _size;
    private Vector2Int _gridPos = Vector2Int.zero;

    private bool _isHovering = false;
    private float _hoveringOffset;
    private Tween _hoverTween;

    #region ---- PROPERTIES ----
    public Vector2Int GridPos => _gridPos;
    public Vector2Int Size => _size;
    #endregion

    #region ---- INITIALIZERS ----
    private void InitCellsSize() => _spriteRenderer.size = _size;
   
    public void Init(Vector2Int initialPos = default)
    {
        _gridPos = initialPos;
        transform.localPosition = new Vector3(_gridPos.x, _gridPos.y);
    }
    #endregion

    #region ---- UNITY CALLBACKS ----
    private void Awake()
    {
        InitCellsSize();
    }
    #endregion
    #region ---- ITEM ACTIONS ----
    public void Rotate()
    {
        if (_hoverTween != null) _hoverTween.Kill();

        TransposeSizeMatrix();
        InitCellsSize();
        RotateModel();
        CenterModel();
    }

    public void Hover(float offset, Transform parent)
    {
        _isHovering = true;

        _hoveringOffset = offset;

        transform.parent = parent;

        DOOffsetAnimation(-transform.forward);
    }

    public void Drop(Vector2Int pos, Transform parent)
    {
        _isHovering = false;

        transform.parent = parent;

        DOOffsetAnimation(transform.forward);

        SetPosition(pos);
    }

    #endregion
    #region ---- POSITIONING & ROTATING FUNCTIONS ----
    public void SetPosition(Vector2Int pos)
    {
        _gridPos = new Vector2Int(pos.x, pos.y);
        transform.localPosition = new Vector3(pos.x, pos.y, 0f);
    }

    private void CenterModel()
    {
        _model.localPosition = new Vector3(_size.x / 2.0f, -_size.y / 2.0f, 0);
        if (_isHovering)
        {
            var hoverPos = -transform.forward * _hoveringOffset;
            _model.localPosition += hoverPos;
        }
    }

    private void RotateModel()
    {
        _model.localRotation = _model.localRotation * Quaternion.Euler(0, 0, -90);
    }
    #endregion
    #region ---- ANIMATIONS ----
    private void DOOffsetAnimation(Vector3 offsetDir)
    {
        var hoverPos = offsetDir * _hoveringOffset;
        _model.DOLocalMove(_model.localPosition + hoverPos, 0.2f);
    }
    #endregion
    #region ---- UTILITIES ----
    private void TransposeSizeMatrix() => _size = new Vector2Int(_size.y, _size.x);
    #endregion
}
