// Copyright HTC Corporation All Rights Reserved.

using UnityEngine.Scripting;
using UnityEngine.XR.OpenXR.Features;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.XR;
using UnityEngine.InputSystem.Controls;
using UnityEngine.XR.OpenXR;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using UnityEngine.XR;
using UnityEngine.XR.OpenXR.Input;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.XR.OpenXR.Features;
#endif

#if USE_INPUT_SYSTEM_POSE_CONTROL // Scripting Define Symbol added by using OpenXR Plugin 1.6.0.
using PoseControl = UnityEngine.InputSystem.XR.PoseControl;
#else
using PoseControl = UnityEngine.XR.OpenXR.Input.PoseControl;
#endif

namespace Wave.OpenXR.Hand
{
    /// <summary>
    /// This <see cref="OpenXRInteractionFeature"/> enables the use of hand interaction profiles in OpenXR. It enables <see cref="ViveHandInteraction.kOpenxrExtensionString">XR_HTC_hand_interaction</see> in the underyling runtime.
    /// </summary>
#if UNITY_EDITOR
    [OpenXRFeature(UiName = "VIVE XR Hand Interaction",
        BuildTargetGroups = new[] { BuildTargetGroup.Android },
        Company = "HTC",
        Desc = "Support for enabling the hand interaction profile. Will register the controller map for hand interaction if enabled.",
        DocumentationLink = "..\\Documentation",
        Version = "1.0.0",
        OpenxrExtensionStrings = kOpenxrExtensionString,
        Category = FeatureCategory.Interaction,
        FeatureId = featureId)]
#endif
    public class ViveHandInteraction : OpenXRInteractionFeature
    {
        const string LOG_TAG = "Wave.OpenXR.Hand.ViveHandInteraction";
        void DEBUG(string msg) { Debug.Log(LOG_TAG + " " + msg); }
        void WARNING(string msg) { Debug.LogWarning(LOG_TAG + " " + msg); }

        /// <summary>
        /// OpenXR specification <see href="https://registry.khronos.org/OpenXR/specs/1.0/html/xrspec.html#XR_HTC_hand_interaction">12.69. XR_HTC_hand_interaction</see>.
        /// </summary>
        public const string kOpenxrExtensionString = "XR_HTC_hand_interaction";

        /// <summary>
        /// The feature id string. This is used to give the feature a well known id for reference.
        /// </summary>
        public const string featureId = "vive.wave.openxr.feature.hand.interaction";

        /// <summary>
        /// The interaction profile string used to reference the hand interaction input device.
        /// </summary>
        private const string profile = "/interaction_profiles/htc/hand_interaction";

        private const string leftHand = "/user/hand_htc/left";
        private const string rightHand = "/user/hand_htc/right";

        /// <summary>
        /// Constant for a float interaction binding '.../input/select/value' OpenXR Input Binding. Used by input subsystem to bind actions to physical inputs.
        /// </summary>
        public const string selectValue = "/input/select/value";

        /// <summary>
        /// Constant for a pose interaction binding '.../input/aim/pose' OpenXR Input Binding. Used by input subsystem to bind actions to physical inputs.
        /// </summary>
        private const string pointerPose = "/input/aim/pose";

        [Preserve, InputControlLayout(displayName = "Vive Hand Interaction (OpenXR)", commonUsages = new[] { "LeftHand", "RightHand" }, isGenericTypeOfDevice = true)]
        public class HandInteractionDevice : OpenXRDevice
        {
            const string LOG_TAG = "Wave.OpenXR.Hand.ViveHandInteraction.HandInteractionDevice";
            void DEBUG(string msg) { Debug.Log(LOG_TAG + " " + msg); }

            /// <summary>
            /// A [AxisControl](xref:UnityEngine.InputSystem.Controls.AxisControl) that represents the <see cref="ViveHandInteraction.selectValue"/> OpenXR binding.
            /// </summary>
            [Preserve, InputControl(aliases = new[] { "selectAxis, pinchStrength" }, usage = "Select")]
            public AxisControl selectValue { get; private set; }

            /// <summary>
            /// A <see cref="PoseControl"/> representing the <see cref="ViveHandInteraction.pointerPose"/> OpenXR binding.
            /// </summary>
			[Preserve, InputControl(offset = 0, alias = "aimPose", usage = "Pointer")]
            public PoseControl pointerPose { get; private set; }

            /// <summary>
            /// Internal call used to assign controls to the the correct element.
            /// </summary>
            protected override void FinishSetup()
            {
                /*for (int i = 0; i < InputSystem.devices.Count; i++)
                {
                    var description = InputSystem.devices[i].description;
                    DEBUG("FinishSetup() device[" + i + "], interfaceName: " + description.interfaceName
                        + ", deviceClass: " + description.deviceClass
                        + ", product: " + description.product);
                }*/

                base.FinishSetup();

                selectValue = GetChildControl<AxisControl>("selectValue");
                pointerPose = GetChildControl<PoseControl>("pointerPose");
            }
        }

#pragma warning disable
        private bool m_XrInstanceCreated = false;
#pragma warning restore
        private XrInstance m_XrInstance = 0;
        /// <summary>
        /// Called when <see href="https://registry.khronos.org/OpenXR/specs/1.0/html/xrspec.html#xrCreateInstance">xrCreateInstance</see> is done.
        /// </summary>
        /// <param name="xrInstance">The created instance.</param>
        /// <returns>True for valid <see cref="XrInstance">XrInstance</see></returns>
        protected override bool OnInstanceCreate(ulong xrInstance)
        {
            // Requires the eye tracking extension
            /*if (!OpenXRRuntime.IsExtensionEnabled(kOpenxrExtensionString))
            {
                WARNING("OnInstanceCreate() " + kOpenxrExtensionString + " is NOT enabled.");
                return false;
            }*/

            m_XrInstanceCreated = true;
            m_XrInstance = xrInstance;
            DEBUG("OnInstanceCreate() " + m_XrInstance);

            return base.OnInstanceCreate(xrInstance);
        }

        private const string kLayoutName = "ViveHandInteraction";
        private const string kDeviceLocalizedName = "Vive Hand Interaction OpenXR";
        /// <summary>
        /// Registers the <see cref="HandInteractionDevice"/> layout with the Input System.
        /// </summary>
        protected override void RegisterDeviceLayout()
        {
            InputSystem.RegisterLayout(typeof(HandInteractionDevice),
                        kLayoutName,
                        matches: new InputDeviceMatcher()
                        .WithInterface(XRUtilities.InterfaceMatchAnyVersion)
                        .WithProduct(kDeviceLocalizedName));
        }

        /// <summary>
        /// Removes the <see cref="HandInteractionDevice"/> layout from the Input System.
        /// </summary>
        protected override void UnregisterDeviceLayout()
        {
            InputSystem.RemoveLayout(kLayoutName);
        }

        /// <summary>
        /// Registers action maps to Unity XR.
        /// </summary>
        protected override void RegisterActionMapsWithRuntime()
        {
            ActionMapConfig actionMap = new ActionMapConfig()
            {
                name = "vivehandinteraction",
                localizedName = kDeviceLocalizedName,
                desiredInteractionProfile = profile,
                manufacturer = "HTC",
                serialNumber = "",
                deviceInfos = new List<DeviceConfig>()
                {
                    new DeviceConfig()
                    {
                        characteristics = InputDeviceCharacteristics.TrackedDevice | InputDeviceCharacteristics.HandTracking | InputDeviceCharacteristics.Left,
                        userPath = leftHand // "/user/hand_htc/left"
                    },
                    new DeviceConfig()
                    {
                        characteristics = InputDeviceCharacteristics.TrackedDevice | InputDeviceCharacteristics.HandTracking | InputDeviceCharacteristics.Right,
                        userPath = rightHand // "/user/hand_htc/right"
                    }
                },
                actions = new List<ActionConfig>()
                {
					// Select Axis
					new ActionConfig()
                    {
                        name = "selectValue",
                        localizedName = "Select Axis",
                        type = ActionType.Axis1D,
                        usages = new List<string>()
                        {
                            "Select"
                        },
                        bindings = new List<ActionBinding>()
                        {
                            new ActionBinding()
                            {
                                interactionPath = selectValue,
                                interactionProfileName = profile,
                            }
                        }
                    },
					// Pointer Pose
					new ActionConfig()
                    {
                        name = "pointerPose",
                        localizedName = "Pointer Pose",
                        type = ActionType.Pose,
                        usages = new List<string>()
                        {
                            "Pointer"
                        },
                        bindings = new List<ActionBinding>()
                        {
                            new ActionBinding()
                            {
                                interactionPath = pointerPose,
                                interactionProfileName = profile,
                            }
                        }
                    }
                }
            };

            AddActionMap(actionMap);
        }
    }
}