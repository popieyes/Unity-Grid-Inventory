using System;
using DG.Tweening;
using UnityEngine;

namespace GridSelector
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class Selector : MonoBehaviour
    {
        #region ---- EVENTS ----
        public Action NavigateEvent;
        public Action NavigateWithItemEvent;
        public Action GrabItemEvent;
        public Action DropItemEvent;

        #endregion

        [Header("Selector Parameters")]
        [SerializeField] private float _selectorOffset = 1f;
        [Header("Grab Item Parameters")]
        [SerializeField] private float _grabbedItemOffset = 0.2f;
        [SerializeField] private Color _unavailableSlotColor = Color.red;
        [SerializeField] private Color _defaultSlotColor = Color.white;

        [Header("References")]
        [SerializeField] private InputProcessorSO _inputProcessor;
        [SerializeField] private GridManager _grid;

        private SpriteRenderer _cursorRenderer;
        private Item _hoveredItem;
        private Item _grabbedItem;
        private bool _canPlace = true;

        private bool HasItem => _grabbedItem;

        #region ---- INITIALIZERS ----
        private void InitRenderer() => _cursorRenderer = GetComponent<SpriteRenderer>();
        private void InitInput()
        {
            _inputProcessor.Enable();
            _inputProcessor.OnNavigateEvent += OnNavigate;
            _inputProcessor.OnActionEvent += OnAction;
            _inputProcessor.OnRotateEvent += OnRotate;
        }
        private void DisableInput()
        {
            _inputProcessor.OnNavigateEvent -= OnNavigate;
            _inputProcessor.OnActionEvent -= OnAction;
            _inputProcessor.OnRotateEvent -= OnRotate;
            _inputProcessor.Disable();
        }
        #endregion

        #region ---- UNITY CALLBACKS ----
        private void Awake()
        {
            InitRenderer();
            InitInput();
        }

        private void OnDisable()
        {
            DisableInput();
        }
        #endregion

        #region ---- SELECTOR VISUAL FUNCTIONS ----
        public void SetSelectorPosition(Vector2 position)
        {
            transform.localPosition = position;

            Vector3 hoverDir = -transform.forward * _selectorOffset;
            transform.localPosition += hoverDir;
        }

        public void SetSize(Vector2 size)
        {
            _cursorRenderer.size = size;
        }
        #endregion

        #region ---- INPUT CALLBACKS ----
        private void OnNavigate(Vector2 movement)
        {
            MoveSelector(movement);
        }

        private void OnAction()
        {
            if (HasItem)
                DropItem();
            else
                GrabItem();
        }

        private void OnRotate()
        {
            if (_grabbedItem)
            {
                _grabbedItem.Rotate();
                SetSize(_grabbedItem.Size);
                SetSelectorPosition(ClampPositionInGrid(transform.localPosition, _grabbedItem));
                _canPlace = _grid.CheckGrid(transform.localPosition, _grabbedItem.Size);
                if (!_canPlace) DebugSelector(_unavailableSlotColor);
                else DebugSelector(_defaultSlotColor);

                NavigateWithItemEvent?.Invoke();
            }
        }
        #endregion

        #region ---- NAVIGATION FUNCTIONS ----
        private void MoveSelector(Vector2 movement)
        {
            Vector3 targetPosition = CheckMovement(movement);
            targetPosition = ClampPositionInGrid(targetPosition, _grabbedItem);
            SetSelectorPosition(targetPosition);
            CheckSelectorSize();
            if (_grabbedItem)
            {
                _canPlace = _grid.CheckGrid(transform.localPosition, _grabbedItem.Size);
                if (!_canPlace) DebugSelector(_unavailableSlotColor);
                else DebugSelector(_defaultSlotColor);
                NavigateWithItemEvent?.Invoke();
                return;
            }
            NavigateEvent?.Invoke();        
        }
        private Vector3 ClampPositionInGrid(Vector3 position, Item grabbedItem = null)
        {
            int itemXSize = 0, itemYSize = 0;
            if (grabbedItem != null)
            {
                itemXSize = grabbedItem.Size.x - 1; itemYSize = grabbedItem.Size.y - 1;
            }
            int xPosClamped = (int)Mathf.Floor(Mathf.Clamp(position.x, 0, _grid.Size.x - 1 - itemXSize));
            int yPosClamped = (int)Mathf.Floor(Mathf.Clamp(position.y, -_grid.Size.y + 1 + itemYSize, 0));

            return new Vector3(xPosClamped, yPosClamped);
        }

        #endregion

        public void CheckSelectorSize()
        {
            var item = DetectItems();
            if (item != null && _hoveredItem != item && _grabbedItem == null)
            {
                SetSize(item.Size);
                SetSelectorPosition(item.transform.localPosition);
                _hoveredItem = item;
            }
            else if (item == null)
            {
                if (_grabbedItem) SetSize(_grabbedItem.Size);
                else SetSize(Vector2Int.one);
                _hoveredItem = null;
            }
        }
        private Vector3 CheckMovement(Vector2 movement)
        {
            Vector3 targetPosition = transform.localPosition;
            if (_hoveredItem != null)
            {
                if (movement.x > 0f)
                {
                    if (targetPosition.x + _hoveredItem.Size.x < _grid.Size.x)
                        targetPosition += new Vector3(_hoveredItem.Size.x, 0f);
                }
                else if (movement.x < 0f)
                {
                     targetPosition += new Vector3(movement.x, movement.y);
                }

                if (movement.y < 0f)
                {
                    if (Mathf.Abs(targetPosition.y - _hoveredItem.Size.y) < _grid.Size.y)
                        targetPosition += new Vector3(0f, -_hoveredItem.Size.y);
                }
                else if (movement.y > 0f)
                {
                    //if (Mathf.Abs(targetPosition.y + _hoveredItem.Size.y) <= 0)
                    // targetPosition += new Vector3(0f, _hoveredItem.Size.y);
                            targetPosition += new Vector3(movement.x, movement.y);
                    }
            }
            else
                targetPosition += new Vector3(movement.x, movement.y);
            return targetPosition;
        }
        private Item DetectItems()
        {
            var gridPos = transform.localPosition;
            Item item = _grid.Grid.Array[(int)gridPos.x, (int)-gridPos.y];
            if (item) return item;
            return null;
        }

        private void GrabItem()
        {
            if (_hoveredItem == null) return;
            _grid.FreeGrid(_hoveredItem);
            _grabbedItem = _hoveredItem;
            _grabbedItem.transform.parent = transform;

            _grabbedItem.Hover(_grabbedItemOffset);

            //var hoverPos = -transform.forward * (Mathf.Abs(_grabbedItemOffset - _selectorOffset));
            //_hoverTween = _model.DOLocalMove(_model.localPosition + hoverPos, 0.2f);

            _hoveredItem = null;
            GrabItemEvent?.Invoke();
        }

        private void DropItem()
        {
            if (_grabbedItem == null) return;
            if (!_canPlace) return;

            _grabbedItem.transform.parent = _grid.ItemContainer;

            _grabbedItem.Drop();

            //var hoverPos = transform.forward * _hoveringOffset;
            //_model.DOLocalMove(_model.localPosition + hoverPos, 0.2f);

            _grabbedItem.SetPosition(new Vector2Int((int)transform.localPosition.x, (int)transform.localPosition.y));
            _grid.FillGrid(_grabbedItem);

            _hoveredItem = _grabbedItem;
            _grabbedItem = null;

            DropItemEvent?.Invoke();
            
        }

        private void DebugSelector(Color color)
        {
            _cursorRenderer.color = color;
        }
    }
}

