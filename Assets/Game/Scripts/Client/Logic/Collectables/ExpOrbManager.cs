using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Game.Scripts.Client.Logic.Colectables;
using PurrNet;
using UnityEngine;

namespace Game.Scripts.Client.Logic.Collectables
{
    public class ExpOrbManager : NetworkBehaviour
    {
        public static List<ExpOrb> AllOrbs = new();

        [SerializeField] private float CombineInterval = 1f;
        [SerializeField] private float CombineRadius = 2f;
       

        private HashSet<ExpOrb> _combinedThisFrame = new(); // Орбы, участвующие в объединении

        protected override void OnSpawned(bool asServer)
        {
            base.OnSpawned(asServer);
            if (!asServer)
                return;
            AllOrbs.Clear();
            InvokeRepeating(nameof(CombineNearbyOrbs), CombineInterval, CombineInterval);
        }

        [ServerRpc(requireOwnership: false)]
        private void CombineNearbyOrbs()
        {
            if (AllOrbs.Count < 2) return;

            var orbs = new List<ExpOrb>(AllOrbs);
            var combinedThisFrame = new HashSet<ExpOrb>();

            foreach (var mainOrb in orbs)
            {
                if (mainOrb == null || !mainOrb.isSpawned || combinedThisFrame.Contains(mainOrb) || mainOrb.State != ExpOrbState.Free)
                    continue;

                Vector3 pos = mainOrb.transform.position;

                var nearby = orbs
                    .Where(o => o != mainOrb &&
                                o != null &&
                                o.isSpawned &&
                                o.State == ExpOrbState.Free && // Только свободные!
                                !combinedThisFrame.Contains(o) &&
                                Vector3.Distance(o.transform.position, pos) <= CombineRadius)
                    .ToList();

                if (nearby.Count == 0) continue;

                // Суммируем опыт
                int totalExp = mainOrb.GetExp();
                foreach (var orb in nearby)
                {
                    totalExp += orb.GetExp();
                    combinedThisFrame.Add(orb);
                }

                // Магниты: направляем соседние орбы к главному
                foreach (var orb in nearby)
                    orb.SetMagnetTarget(mainOrb.transform); // вызовет SetMagnetTarget -> установит состояние

                // Удаляем их через задержку
                foreach (var orb in nearby)
                    StartCoroutine(DestroyAfterDelay(orb, 0.5f));

                // Обновляем опыт главного орба
                mainOrb.SetUpExpServer(totalExp);
                combinedThisFrame.Add(mainOrb);
            }
        }

        private IEnumerator DestroyAfterDelay(ExpOrb orb, float delay)
        {
            yield return new WaitForSeconds(delay);
            if (orb != null && orb.isSpawned)
                orb.DestroyOrbServer();
        }

        // Чтобы использовать магниты к игроку
        [ServerRpc(requireOwnership: false)]
        public static void MagnetAllOrbsToPlayer(Transform player)
        {
            foreach (var orb in AllOrbs)
            {
                if (orb != null && orb.isSpawned)
                    orb.SetMagnetTarget(player);
            }
        }

        protected override void OnDespawned(bool asServer)
        {
            base.OnDespawned(asServer);
            if (!asServer) return;
            AllOrbs.Clear();
        }
    }
}