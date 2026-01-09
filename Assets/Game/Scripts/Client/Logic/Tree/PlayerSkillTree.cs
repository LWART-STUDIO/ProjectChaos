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
    private GameStateLevelUp _levelUp;
    private int _abilityPoints;

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
        _levelUp = stateLevelUp;
        _abilityPoints += i;
        _canvasGroup.alpha = 1;
        _canvasGroup.interactable = true;
        _canvasGroup.blocksRaycasts = true;
        Cursor.visible = true;
    }
    public void CloseWindow()
    {
        if(_levelUp != null)
            _levelUp.SetReady();
        _canvasGroup.alpha = 0;
        _canvasGroup.interactable = false;
        _canvasGroup.blocksRaycasts = false;
    }
}
