using System;
using System.Collections.Generic;
using Game.Scripts.Client.Logic.Game;
using Game.Scripts.Client.Logic.Location;
using Game.Scripts.Client.Logic.Player.Stats;
using PurrNet;
using SaintsField;
using UnityEngine;

namespace Game.Scripts.Client.Logic.Events
{
    public class EventsManager :  NetworkBehaviour
    {
        [FieldLabelText("$" + nameof(StatsLabels))]
        [SerializeField] private List<EventData> _events;
        private string StatsLabels(EventData _, int index) => $"<color=gray>[{_.eventType.ToString()}][{_.startTime}]";
        [SerializeField] private ProceduralTerrain _terrain;
        private bool _spawned = false;

        private float CurrentTime => GameStatisticCollector.GameTime.value;

        protected override void OnSpawned(bool asServer)
        {
            base.OnSpawned(asServer);
            _spawned = true;
        }

        private void Update()
        {
            if(!isServer)
                return;
            if(!_spawned)
                return;
            foreach (var eventData in _events)
            {
                if (eventData.state != EventState.Pending)
                    continue;
                if (CurrentTime < eventData.startTime)
                    continue;
                StartEvent(eventData);
                break;
            }
        }

        private void StartEvent(EventData eventData)
        {
            Debug.Log("Starting event " + eventData.eventType);
            eventData.state = EventState.WaitingForPosition;

            switch (eventData.eventType)
            {
                case EventType.PassiveGrant:
                    StartPassiveGrantEvent(eventData);
                    break;
            }
        }

        private void StartPassiveGrantEvent(EventData eventData)
        {
            Debug.Log("Starting passive grant event");
            _terrain.GetPositionForEvent(pos =>
            {
                // ⚠️ Terrain мог не вернуть позицию
                if (eventData.state != EventState.WaitingForPosition)
                    return;
                if (pos == Vector3.zero)
                {
                    eventData.state = EventState.Pending;
                    return;
                }
                GameObject instance = Instantiate(
                    eventData.eventObject,
                    pos,
                    Quaternion.identity
                );
                
                eventData.state = EventState.Active;
                StartCoroutine(FinishEventAfterTime(eventData));
            });
        }

        private System.Collections.IEnumerator FinishEventAfterTime(EventData eventData)
        {
            yield return new WaitForSeconds(eventData.duration);
            eventData.state = EventState.Completed;
        }
    }
    [System.Serializable]
    public enum EventType
    {
        PassiveGrant=0,
    }
    [System.Serializable]
    public enum EventState
    {
        Pending,
        WaitingForPosition,
        Active,
        Completed
    }
    [System.Serializable]
    public class EventData
    {
        public EventType eventType;
        public float duration;
        public float startTime;
        public GameObject eventObject;
        [HideInInspector] public EventState state = EventState.Pending;
    }
    
}
