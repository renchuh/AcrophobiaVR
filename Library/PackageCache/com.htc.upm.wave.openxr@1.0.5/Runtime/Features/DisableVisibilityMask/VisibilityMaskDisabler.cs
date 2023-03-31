using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

namespace Wave.OpenXR
{
    public class VisibilityMaskDisabler : MonoBehaviour
    {
        
        void Start()
        {
            StartCoroutine(DisablingOcclusion());
            DontDestroyOnLoad(gameObject);
        }


        private void OnApplicationPause(bool pause)
        {
            if (!pause)
            {
                StartCoroutine(DisablingOcclusion());
            }    
        }
      
        IEnumerator DisablingOcclusion()
        {
            float _StartTime = Time.time;
            bool _Done = false;
            while (!_Done)
            {
                if (NeedWorkAround() && XRSettings.occlusionMaskScale != 0.0f)
                {
                    XRSettings.occlusionMaskScale = 0.0f;
                    _Done = true;
                }
                else if(Time.time - _StartTime > 1)
                {
                    _Done = true;
                }
                yield return new WaitForEndOfFrame();
            }
        }
        bool NeedWorkAround()
        {
            if (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Vulkan && (XRSettings.stereoRenderingMode == XRSettings.StereoRenderingMode.SinglePass || XRSettings.stereoRenderingMode == XRSettings.StereoRenderingMode.SinglePass || XRSettings.stereoRenderingMode == XRSettings.StereoRenderingMode.SinglePassMultiview))
            {
                return true;
            }
            return false;
        }
    }


}

