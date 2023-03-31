using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Wave.OpenXR {
    public class DisableVisibilityMask
    {
        static GameObject Provider;

        [RuntimeInitializeOnLoadMethod]
        static void Start()
        {
            Provider = new GameObject();
            Provider.AddComponent<VisibilityMaskDisabler>();
        }
    }
}
