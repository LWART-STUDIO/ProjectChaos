using System.Collections.Generic;
using Sisus.Init;
using UnityEngine;

namespace Game.Scripts.Services.ResourceLoader
{
    [Service]
    public class ResourceLoaderService : MonoBehaviour
    {
        private Dictionary<string, Object> _cache = new Dictionary<string, Object>();
        public T Load<T>(string path, bool useCache = true) where T : Object
        {
            if (useCache && _cache.ContainsKey(path))
            {
                return _cache[path] as T;
            }

            T resource = Resources.Load<T>(path);
            if (resource == null)
            {
                Debug.LogError($"Resource not found at path: {path}");
                return null;
            }

            if (useCache)
            {
                _cache[path] = resource;
            }

            return resource;
        }
        public void ClearCache()
        {
            _cache.Clear();
        }
        public void RemoveFromCache(string path)
        {
            if (_cache.ContainsKey(path))
            {
                _cache.Remove(path);
            }
        }
        
    }
}
