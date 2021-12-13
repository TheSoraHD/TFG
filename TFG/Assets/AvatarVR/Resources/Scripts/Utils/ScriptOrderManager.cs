using System;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

#if UNITY_EDITOR
[InitializeOnLoad]
public class ScriptOrderManager 
{
    static ScriptOrderManager() 
    {
        foreach (MonoScript monoScript in MonoImporter.GetAllRuntimeMonoScripts()) 
        {
            if (monoScript.GetClass() != null) 
            {
                foreach (var a in Attribute.GetCustomAttributes(monoScript.GetClass(), typeof(ScriptOrder))) 
                {
                    var currentOrder = MonoImporter.GetExecutionOrder(monoScript);
                    var newOrder = ((ScriptOrder)a).order;
                    if (currentOrder != newOrder)
                    {
                        MonoImporter.SetExecutionOrder(monoScript, newOrder);
                    }
                }
            }
        }
    }
}
#endif
