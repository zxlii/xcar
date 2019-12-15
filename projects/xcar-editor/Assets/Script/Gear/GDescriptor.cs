
using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
namespace Gear.Runtime
{
    public class GDescriptor : UIBehaviour
    {
        [SerializeField]
        private List<GameObject> m_Objects = new List<GameObject>();
        public void AddObject(GameObject obj)
        {
            m_Objects.Add(obj);
        }
        public T GetObject<T>(string eleName) where T : UnityEngine.Object
        {
            var obj = m_Objects.Find(ele => ele.name.Equals(eleName));
            if (obj == null)
            {
                Debug.LogError(string.Format("'{0}' can not be found.", eleName));
                return null;
            }
            var type = typeof(T);
            if (type.Equals(typeof(GameObject)))
            {
                return obj as T;
            }
            else
            {
                var com = obj.GetComponent(type);
                if (com == null)
                {
                    com = obj.AddComponent(type);
                    Debug.LogError(string.Format("the object '{0}' doesn't has the target component '{1}',but we auto added it.", eleName, typeof(T).Name));
                }
                return com as T;
            }
        }
        public void Clear()
        {
            m_Objects.Clear();
        }
        public List<GameObject> GetAllGameObjects()
        {
            return m_Objects;
        }

    }
}