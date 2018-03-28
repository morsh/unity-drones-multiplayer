using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts {
    class EnableXROnLoad : MonoBehaviour {
        public void Start() {
            Debug.Log("Device Active: " + UnityEngine.XR.XRSettings.isDeviceActive.ToString());
            UnityEngine.XR.XRSettings.enabled = true;
            UnityEngine.XR.XRSettings.LoadDeviceByName("WindowsMR");
            Debug.Log("Device Active: " + UnityEngine.XR.XRSettings.isDeviceActive.ToString());
            Debug.Log("Device Active: " + UnityEngine.XR.XRSettings.loadedDeviceName);
        }
    }
}
