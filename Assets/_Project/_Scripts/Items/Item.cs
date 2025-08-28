using UnityEngine;
using DG.Tweening;

public class Item : MonoBehaviour
{
    [SerializeField] private Transform _pivot;
    [SerializeField] private Transform _model;
    [SerializeField] private Vector2Int _size;
    private Vector2Int _gridPos = Vector2Int.zero;
    private GridManager _gridManager;
    [SerializeField] private SpriteRenderer _spriteRenderer;
    public Vector2Int GridPos => _gridPos;
    public Vector2Int Size => _size;
    public Transform Model => _pivot;
    
    private Vector3 _originalModelLocalPosition;
    private bool _isHovering = false;
    private float _hoveringOffset;

    private Tween _hoverTween;

    #region ---- INITIALIZERS ----

    #endregion

    #region ---- UNITY CALLBACKS ----
    private void Awake()
    {
        _originalModelLocalPosition = _model.localPosition;
    }
    #endregion
    public void Init(GridManager gridManager, Vector2Int initialPos = default)
    {
        _gridManager = gridManager;
        _gridPos = initialPos;
        SetInitialPosition();
    }
    public void Rotate()
    {
        if(_hoverTween != null) _hoverTween.Kill();

        _size = new Vector2Int(_size.y, _size.x);
        _spriteRenderer.size = _size;
        _model.localRotation = _model.localRotation * Quaternion.Euler(0, 0, -90);
        CenterModel();
    }

    public void SetPosition(Vector2Int pos)
    {
        _gridPos = new Vector2Int(pos.x, pos.y);
        transform.localPosition = new Vector3(pos.x, pos.y, 0f);
    }

    private void SetInitialPosition()
    {
        //transform.localPosition = new Vector2(Pos.x + _size.x * .5f - 1, -(Pos.y + _size.y * .5f - 1));
        transform.localPosition = new Vector3(_gridPos.x, _gridPos.y);
    }

    private void CenterModel()
    {
        _pivot.localPosition = new Vector3(_size.x / 2.0f, -_size.y / 2.0f, 0);
        if (_isHovering)
        {
            var hoverPos = -transform.forward * _hoveringOffset;
            _model.localPosition += hoverPos;
        }
    }

    public void Hover(float offset)
    {
        // Hover animation
        _isHovering = true;
        _hoveringOffset = offset;
        
        var hoverPos = -transform.forward * _hoveringOffset;
        _hoverTween = _model.DOLocalMove(_model.localPosition + hoverPos, 0.2f);

    }

    public void Drop()
    {
        _isHovering = false;
        // Drop animation
        var hoverPos = transform.forward * _hoveringOffset;
        _model.DOLocalMove(_model.localPosition + hoverPos, 0.2f);
    }
}
