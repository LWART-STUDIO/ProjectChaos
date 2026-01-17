using UnityEngine;

namespace Game.Scripts.Client.Logic.Location
{
    public class LocationSpawner : MonoBehaviour
    {
        [SerializeField] private GameObject _startLocation;
        [SerializeField] private GameObject _mainLocation;
        private BuildNavMesh _navMeshBuilder;
        
        public void DisableStartLocation()
        {
            _startLocation.SetActive(false);
            _mainLocation.GetComponent<BuildNavMesh>().GenerateWalls();
        }

        public void EnableStartLocation()
        {
            _startLocation.SetActive(true);
        }

        public void SpawnLocation()
        {
            _mainLocation.SetActive(true);
        }
    }
}
