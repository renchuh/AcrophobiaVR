﻿// "Wave SDK 
// © 2020 HTC Corporation. All Rights Reserved.
//
// Unless otherwise required by copyright law and practice,
// upon the execution of HTC SDK license agreement,
// HTC grants you access to and use of the Wave SDK(s).
// You shall fully comply with all of HTC’s SDK license agreement terms and
// conditions signed by you and all SDK and API requirements,
// specifications, and documentation provided by HTC to You."

using System.Collections.Generic;
using UnityEditor;
using UnityEngine.XR.OpenXR;
using UnityEngine.XR.OpenXR.Features;
using AOT;
using UnityEngine;
using System;
using System.Runtime.InteropServices;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor.XR.OpenXR.Features;
#endif

namespace Wave.OpenXR.CompositionLayer
{
#if UNITY_EDITOR
	[OpenXRFeature(UiName = "VIVE XR Composition Layer (Cylinder)",
		Desc = "Enable this feature to enable the Composition Layer Cylinder Extension",
		Company = "HTC",
		DocumentationLink = "..\\Documentation",
		OpenxrExtensionStrings = kOpenXRCylinderExtensionString,
		Version = "1.0.0",
		BuildTargetGroups = new[] { BuildTargetGroup.Android },
		FeatureId = featureId
	)]
#endif
	public class ViveCompositionLayerCylinder : OpenXRFeature
	{
		const string LOG_TAG = "Wave.OpenXR.ViveCompositionLayer.Cylinder";
		static void DEBUG(string msg) { Debug.Log(LOG_TAG + " " + msg); }
		static void WARNING(string msg) { Debug.LogWarning(LOG_TAG + " " + msg); }
		static void ERROR(string msg) { Debug.LogError(LOG_TAG + " " + msg); }

		/// <summary>
		/// The feature id string. This is used to give the feature a well known id for reference.
		/// </summary>
		public const string featureId = "vive.wave.openxr.feature.compositionlayer.cylinder";

		private const string kOpenXRCylinderExtensionString = "XR_KHR_composition_layer_cylinder";

		private bool m_CylinderExtensionEnabled = true;
		public bool CylinderExtensionEnabled
		{
			get { return m_CylinderExtensionEnabled; }
		}


		#region OpenXR Life Cycle
		protected override bool OnInstanceCreate(ulong xrInstance)
		{
			if (!OpenXRRuntime.IsExtensionEnabled(kOpenXRCylinderExtensionString))
			{
				WARNING("OnInstanceCreate() " + kOpenXRCylinderExtensionString + " is NOT enabled.");

				m_CylinderExtensionEnabled = false;
				return false;
			}

			return true;
		}
		#endregion

		#region Wrapper Functions
		private const string ExtLib = "waveopenxr";

		[DllImportAttribute(ExtLib, EntryPoint = "submit_CompositionLayerCylinder")]
		public static extern void WaveOpenXR_Submit_CompositionLayerCylinder(XrCompositionLayerCylinderKHR cylinder, LayerType layerType, uint compositionDepth, int layerID);
		public void Submit_CompositionLayerCylinder(XrCompositionLayerCylinderKHR cylinder, LayerType layerType, uint compositionDepth, int layerID)
		{
			if (!CylinderExtensionEnabled)
			{
				ERROR("Submit_CompositionLayerCylinder: " + kOpenXRCylinderExtensionString + " is NOT enabled.");
			}

			WaveOpenXR_Submit_CompositionLayerCylinder(cylinder, layerType, compositionDepth, layerID);
		}
		#endregion
	}
}
