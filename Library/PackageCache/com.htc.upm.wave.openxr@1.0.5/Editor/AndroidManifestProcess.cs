// Copyright HTC Corporation All Rights Reserved.
using System;
using System.IO;
using System.Text;
using System.Xml;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.XR.OpenXR.Features;
using UnityEngine;
using UnityEngine.XR.OpenXR;
using UnityEngine.XR.OpenXR.Features;
using Wave.OpenXR.Tracker;

// Reference to Unity's OpenXR package

namespace Wave.OpenXR.Editor
{
    internal class ModifyAndroidManifest : OpenXRFeatureBuildHooks
    {
        public override int callbackOrder => 1;

        public override Type featureType => typeof(VIVEFocus3Feature);

        protected override void OnPreprocessBuildExt(BuildReport report)
        {
        }

        protected override void OnPostGenerateGradleAndroidProjectExt(string path)
        {
            var androidManifest = new AndroidManifest(GetManifestPath(path));
            //androidManifest.AddWaveCategory();
            androidManifest.AddOpenXRPermission();
			androidManifest.AddOpenXRFeatures();
			androidManifest.Save();
        }

        protected override void OnPostprocessBuildExt(BuildReport report)
        {
        }

        private string _manifestFilePath;

        private string GetManifestPath(string basePath)
        {
            if (!string.IsNullOrEmpty(_manifestFilePath)) return _manifestFilePath;

            var pathBuilder = new StringBuilder(basePath);
            pathBuilder.Append(Path.DirectorySeparatorChar).Append("src");
            pathBuilder.Append(Path.DirectorySeparatorChar).Append("main");
            pathBuilder.Append(Path.DirectorySeparatorChar).Append("AndroidManifest.xml");
            _manifestFilePath = pathBuilder.ToString();

            return _manifestFilePath;
        }

        private class AndroidXmlDocument : XmlDocument
        {
            private string m_Path;
            protected XmlNamespaceManager nsMgr;
            public readonly string AndroidXmlNamespace = "http://schemas.android.com/apk/res/android";

            public AndroidXmlDocument(string path)
            {
                m_Path = path;
                using (var reader = new XmlTextReader(m_Path))
                {
                    reader.Read();
                    Load(reader);
                }

                nsMgr = new XmlNamespaceManager(NameTable);
                nsMgr.AddNamespace("android", AndroidXmlNamespace);
            }

            public string Save()
            {
                return SaveAs(m_Path);
            }

            public string SaveAs(string path)
            {
                using (var writer = new XmlTextWriter(path, new UTF8Encoding(false)))
                {
                    writer.Formatting = Formatting.Indented;
                    Save(writer);
                }

                return path;
            }
        }

        private class AndroidManifest : AndroidXmlDocument
        {
            private readonly XmlElement IntetnFilterElement;
            private readonly XmlElement ManifestElement;

            public AndroidManifest(string path) : base(path)
            {
                IntetnFilterElement = SelectSingleNode("/manifest/application/activity/intent-filter") as XmlElement;
                ManifestElement = SelectSingleNode("/manifest") as XmlElement;
            }

            private XmlAttribute CreateAndroidAttribute(string key, string value)
            {
                XmlAttribute attr = CreateAttribute("android", key, AndroidXmlNamespace);
                attr.Value = value;
                return attr;
            }

            internal void AddWaveCategory()
            {
                var md = IntetnFilterElement.AppendChild(CreateElement("category"));
                md.Attributes.Append(CreateAndroidAttribute("name", "com.htc.intent.category.VRAPP"));
            }

            internal void AddOpenXRPermission()
            {
                var md = ManifestElement.AppendChild(CreateElement("uses-permission"));
                md.Attributes.Append(CreateAndroidAttribute("name", "org.khronos.openxr.permission.OPENXR"));

                md = ManifestElement.AppendChild(CreateElement("uses-permission"));
                md.Attributes.Append(CreateAndroidAttribute("name", "org.khronos.openxr.permission.OPENXR_SYSTEM"));

                var md2 = IntetnFilterElement.AppendChild(CreateElement("category"));
                md2.Attributes.Append(CreateAndroidAttribute("name", "org.khronos.openxr.intent.category.IMMERSIVE_HMD"));
            }

			internal void AddOpenXRFeatures()
			{
				bool enableViveWristTracker = false;
				var settings = OpenXRSettings.GetSettingsForBuildTargetGroup(BuildTargetGroup.Android);
				if (null == settings)
					return;

				foreach (var feature in settings.GetFeatures<OpenXRInteractionFeature>())
				{
					if (feature is ViveWristTracker)
					{
						enableViveWristTracker = feature.enabled;
						break;
					}
				}

				//Debug.Log("enableViveWristTracker " + enableViveWristTracker);
				if (enableViveWristTracker)
				{
					{
						var newUsesFeature = CreateElement("uses-feature");
						newUsesFeature.Attributes.Append(CreateAndroidAttribute("name", "wave.feature.handtracking"));
						newUsesFeature.Attributes.Append(CreateAndroidAttribute("required", "true"));
						ManifestElement.AppendChild(newUsesFeature);
					}
					{
						var newUsesFeature = CreateElement("uses-feature");
						newUsesFeature.Attributes.Append(CreateAndroidAttribute("name", "wave.feature.tracker"));
						newUsesFeature.Attributes.Append(CreateAndroidAttribute("required", "true"));
						ManifestElement.AppendChild(newUsesFeature);
					}
				}
			}
		}
    }
}

