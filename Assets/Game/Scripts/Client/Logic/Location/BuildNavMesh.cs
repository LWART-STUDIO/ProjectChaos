using System;
using System.Collections.Generic;
using FlexiblePathfindingSystem3D;
using Unity.AI.Navigation;
using UnityEngine;

namespace Game.Scripts.Client.Logic.Location
{
    [RequireComponent(typeof(NavMeshSurface))]
    public class BuildNavMesh : MonoBehaviour
    {
        private NavMeshSurface _navMeshSurface;

        private void Awake()
        {
         //   _navMeshSurface = GetComponent<NavMeshSurface>();
        }

        private void Start()
        {
          //  _navMeshSurface.BuildNavMesh();
        }

        public void GenerateWalls()
        {
           // NavLinkManager.Instance.GetComponent<NavMeshLinksGenerator>().CreateLinks();
        }
    }
}
