// Copyright © 2015 – Property of Tobii AB (publ) - All Rights Reserved

namespace Tobii.VR
{
    using UnityEditor;
    using UnityEngine;

    // Credit http://squabpie.com/blog/2015/11/13/scripts-execution-order.html
    [InitializeOnLoad]
    public class TobiiVR_ExecutionOrderValidator
    {

        const int ReaderExecOrder = -10000;

        /// This functions ensure that TobiiVR_Host is called first of all scripts.
        /// The benefit by doing this is that every scripts that is dependent on gaze 
        /// will use the exact same data as any other script.
        static TobiiVR_ExecutionOrderValidator()
        {
            var temp = new GameObject();

            var reader = temp.AddComponent<TobiiVR_Host>();
            MonoScript readerScript = MonoScript.FromMonoBehaviour(reader);
            if (MonoImporter.GetExecutionOrder(readerScript) != ReaderExecOrder)
            {
                MonoImporter.SetExecutionOrder(readerScript, ReaderExecOrder);
                Debug.Log("Fixing exec order for " + readerScript.name);
            }

            Object.DestroyImmediate(temp);
        }
    }
}