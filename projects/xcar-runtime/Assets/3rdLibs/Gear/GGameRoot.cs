using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XLua;

namespace Gear.Runtime
{
    public class GGameRoot : MonoBehaviour
    {
        protected GLuaEnvironment lua;
        protected virtual void Awake()
        {
            lua = new GLuaEnvironment();
        }

        protected virtual void Start()
        {

        }

        protected virtual void FixedUpdate()
        {

        }

        protected virtual void Update()
        {

        }

        protected virtual void LateUpdate()
        {

        }
    }
}