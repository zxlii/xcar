using UnityEngine;
namespace Gear.Runtime
{
    public class GCoroutineHolder : MonoBehaviour
    {
        private static GCoroutineHolder m_Instance;
        public static GCoroutineHolder Instance
        {
            get
            {
                string hodlerName = "__CoroutineHolder";
                if (!m_Instance)
                {
                    GameObject go = GameObject.Find(hodlerName);
                    if (!go)
                    {
                        go = new GameObject(hodlerName);
                    }
                    m_Instance = go.GetComponent<GCoroutineHolder>();
                    if (!m_Instance)
                        m_Instance = go.AddComponent<GCoroutineHolder>();
                }
                return m_Instance;
            }
        }
    }
}