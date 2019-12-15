
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine;
namespace Gear.Runtime.UI
{
    public class ObjectPool<T> where T : new()
    {
        private readonly Stack<T> m_Stack = new Stack<T>();
        private readonly UnityAction<T> m_ActionOnGet;
        private readonly UnityAction<T> m_ActionOnRelease;
        private readonly UnityAction<T> m_ActionOnLimite;
        private int m_Limite;
        public int countAll { get; private set; }
        public int countActive { get { return countAll - countInactive; } }
        public int countInactive { get { return m_Stack.Count; } }
        public int maxCount { set { m_Limite = value; } }

        public ObjectPool(UnityAction<T> actionOnGet, UnityAction<T> actionOnRelease, UnityAction<T> actionOnLimite, int limite)
        {
            m_ActionOnGet = actionOnGet;
            m_ActionOnRelease = actionOnRelease;
            m_ActionOnLimite = actionOnLimite;
            m_Limite = limite;
        }

        public T Get()
        {
            T element;
            if (m_Stack.Count == 0)
            {
                element = new T();
                countAll++;
            }
            else
            {
                element = m_Stack.Pop();
            }
            if (m_ActionOnGet != null)
                m_ActionOnGet(element);
            return element;
        }

        public void Release(T element)
        {
            if (m_Stack.Count >= m_Limite)
            {
                if (m_ActionOnLimite != null)
                    m_ActionOnLimite(element);
                return;
            }

            if (m_Stack.Count > 0 && ReferenceEquals(m_Stack.Peek(), element))
                Debug.LogError("Internal error. Trying to destroy object that is already released to pool.");
            if (m_ActionOnRelease != null)
                m_ActionOnRelease(element);
            m_Stack.Push(element);
        }
    }
}