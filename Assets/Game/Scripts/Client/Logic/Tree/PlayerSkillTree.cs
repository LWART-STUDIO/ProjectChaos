using System;
using System.Collections.Generic;
using Game.Scripts.Client.Logic.Game;
using Game.Scripts.Client.Logic.Player.Stats;
using Game.Scripts.Client.Logic.Tree;
using SaintsField.Playa;
using UnityEngine;

public class PlayerSkillTree : MonoBehaviour
{
    [SerializeField] private PlayerStatsHolder stats;
    [SerializeField] private List<TreeLinkView> links;
    [SerializeField] private List<TreeNodeView> nodes;
    [SerializeField] private TreeNodeView startNode;
    [SerializeField] private CanvasGroup _canvasGroup;
    [SerializeField] private RectTransform _treeToMove;
    
    [Header("Pan & Zoom")]
    [SerializeField] private float zoomSpeed = 0.2f;
    [SerializeField] private float dragSpeed = 1f;

    [SerializeField] private Vector2 zoomLimits = new(0.5f, 2.5f);

    [Header("Movement Bounds (Local Space)")]
    [SerializeField] private Vector2 boundsMin = new(-800, -800);
    [SerializeField] private Vector2 boundsMax = new(800, 800);
    
    private GameStateLevelUp _levelUp;
    private int _abilityPoints;
    [SerializeField]private bool _isOpen = false;
    
    private Vector2 _lastMousePos;
    private float _currentZoom = 1f;
    private float defaultZoom = 2f;
    private Vector2 defaultPosition=Vector2.zero;
    

    private HashSet<int> selectedNodes = new();

    public void SetUp(PlayerStatsHolder stats)
    {
        this.stats = stats;
        UpdateTree();
    }
    public bool TrySelectNode(TreeNodeView node)
    {
        if (selectedNodes.Contains(node.Id))
            return false;

        if (!HasSelectedNeighbour(node))
            return false;
        if(_abilityPoints == 0)
            return false;
        _abilityPoints--;
        
        selectedNodes.Add(node.Id);
        
        foreach (var bonus in node.node.bonuses)
            stats.ApplyStat(bonus);

        RefreshVisuals();
        return true;
    }
    public void ResetTree()
    {
        selectedNodes.Clear();
        stats.ResetStats();

        if (startNode != null)
        {
            selectedNodes.Add(startNode.Id);
            foreach (var bonus in startNode.node.bonuses)
                stats.ApplyStat(bonus);
        }

        RefreshVisuals();
    }
    private void RefreshVisuals()
    {
        foreach (var node in nodes)
            node.Refresh(this);

        foreach (var link in links)
        {
            bool active =
                selectedNodes.Contains(link.fromId) &&
                selectedNodes.Contains(link.toId);

            link.Refresh(active);
        }
    }

    private void Update()
    {
        if(!_isOpen)
            return;
        HandleDrag();
        HandleZoom();
        
      
    }
    private void HandleDrag()
    {
        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(2))
            _lastMousePos = Input.mousePosition;

        if (Input.GetMouseButton(0) || Input.GetMouseButton(2))
        {
            Vector2 delta = (Vector2)Input.mousePosition - _lastMousePos;
            _lastMousePos = Input.mousePosition;

            _treeToMove.anchoredPosition += delta * dragSpeed;
            ClampPosition();
        }
    }
    private void HandleZoom()
    {
        float scroll = Input.mouseScrollDelta.y;
        if (Mathf.Approximately(scroll, 0f))
            return;

        _currentZoom += scroll * zoomSpeed;
        _currentZoom = Mathf.Clamp(_currentZoom, zoomLimits.x, zoomLimits.y);

        _treeToMove.localScale = Vector3.one * _currentZoom;
        ClampPosition();
    }
    private void ClampPosition()
    {
        Vector2 scaledMin = boundsMin * _currentZoom;
        Vector2 scaledMax = boundsMax * _currentZoom;

        Vector2 pos = _treeToMove.anchoredPosition;
        pos.x = Mathf.Clamp(pos.x, scaledMin.x, scaledMax.x);
        pos.y = Mathf.Clamp(pos.y, scaledMin.y, scaledMax.y);

        _treeToMove.anchoredPosition = pos;
    }
    private void OnDrawGizmosSelected()
    {
        if (_treeToMove == null)
            return;

        Gizmos.color = Color.cyan;

        float zoom = Application.isPlaying ? _currentZoom : 1f;

        Vector3 center = _treeToMove.parent.TransformPoint(Vector3.zero);
        Vector3 size = new Vector3(
            (boundsMax.x - boundsMin.x) * zoom,
            (boundsMax.y - boundsMin.y) * zoom,
            0
        );

        Gizmos.DrawWireCube(center, size);
    }
    

    private bool HasSelectedNeighbour(TreeNodeView node)
    {
        foreach (var link in links)
        {
            if (link.fromId == node.Id || link.toId == node.Id)
            {
                int otherId = link.fromId == node.Id ? link.toId : link.fromId;
                if (selectedNodes.Contains(otherId))
                    return true;
            }
        }
        return selectedNodes.Count == 0; // стартовая нода
    }
    [Button]
    public void ResetView()
    {
        _currentZoom = Mathf.Clamp(defaultZoom, zoomLimits.x, zoomLimits.y);
        _treeToMove.localScale = Vector3.one * _currentZoom;

        _treeToMove.anchoredPosition = defaultPosition;
        ClampPosition();
    }

    [Button]
    private void UpdateTree()
    {
        links.Clear();
        nodes.Clear();
        nodes.AddRange(GetComponentsInChildren<TreeNodeView>());
        foreach (var node in nodes)
        {
            node.UpdateID();
            node.Init(this);
        }
        links.AddRange(GetComponentsInChildren<TreeLinkView>());
        foreach (var link in links)
        {
            link.UpdateIds();
        }
        
    }

    public bool IsSelected(TreeNodeView node)
        => selectedNodes.Contains(node.Id);

    public void OpenWindow(int i=0,GameStateLevelUp stateLevelUp =null)
    {
        ResetView();
        _isOpen = true;
        _levelUp = stateLevelUp;
        _abilityPoints += i;
        _canvasGroup.alpha = 1;
        _canvasGroup.interactable = true;
        _canvasGroup.blocksRaycasts = true;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    public void CloseWindow()
    {
        _isOpen = false;
        if(_levelUp != null)
            _levelUp.SetReady();
        _canvasGroup.alpha = 0;
        _canvasGroup.interactable = false;
        _canvasGroup.blocksRaycasts = false;
    }
}
