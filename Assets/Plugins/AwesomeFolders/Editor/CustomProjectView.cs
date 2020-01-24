using System.Collections;
using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;
using System.Reflection.Emit;
using UnityEditor.IMGUI.Controls;

namespace AwesomeFolders
{
	/// <summary>
	/// Class responsible for binding custom icon methods to project view events
	/// </summary>
	[InitializeOnLoad]
	public class CustomProjectView
	{
		// Bindings
		private static MethodInfo ActualImageDrawPosition;
		private static Type projectBrowserType;
		private static FieldInfo folderTreeField;
		private static FieldInfo assetTreeField;
		private static FieldInfo opennedBrowsersField;
		private static MethodInfo repaintBrowsers;


		private static Type objectListAreaType;
		private static EventInfo postAssetIconEvent;
		private static FieldInfo postAssetIconField;
		private static MethodInfo postAssetIconEventAdd;
		private static MethodInfo postAssetIconEventRemove;
		private static MethodInfo onPostIconDrawMethod;

		private static Type assetTreeView;
		private static EventInfo postAssetIconEvent2;
		private static FieldInfo postAssetIconField2;
		private static MethodInfo postAssetIconEventAdd2;
		private static MethodInfo postAssetIconEventRemove2;
		private static MethodInfo onPostIconDrawMethod2;

		private static GUIStyle iconBackgroundStyle;

		// Data
		private static object lastFolderTreeController;
		private static object lastAssetTreeController;

		/// <summary>
		/// Bind to update event & prepare bindings
		/// </summary>
		static CustomProjectView()
		{
			Init();
			EditorApplication.update += CheckInitialized;
		}

		/// <summary>
		/// Check UI is ready & does not need to be reinitialized
		/// </summary>
		static void CheckInitialized()
		{
			PreferencesUI.InitPreferences();
			PreferencesUI.AutoConvertAssets();

			if (!PreferencesUI.folderIconEnabled)
			{
				return;
			}

			// Get all openned project browser
			IList opennedBrowsers = (IList)opennedBrowsersField.GetValue(null);

			// For each openned project browser try to initialize custom GUI
			foreach (object projectView in opennedBrowsers)
			{
				object folderTreeController = folderTreeField.GetValue(projectView);
				object assetTreeController = assetTreeField.GetValue(projectView);

				// View changed
				if(folderTreeController != lastFolderTreeController || lastAssetTreeController != assetTreeController)
				{
					InitUI();
				}

				lastFolderTreeController = folderTreeController;
				lastAssetTreeController = assetTreeController;
			}
		}

		/// <summary>
		/// Use bindings to register into events & hierarchy
		/// </summary>
		public static void InitUI()
		{
			/// Register handle into event
			// Search for previously added event & reorder with collab
			AttachToEvent(postAssetIconField, postAssetIconEvent, postAssetIconEventAdd, postAssetIconEventRemove, onPostIconDrawMethod);
			AttachToEvent(postAssetIconField2, postAssetIconEvent2, postAssetIconEventAdd2, postAssetIconEventRemove2, onPostIconDrawMethod2);
		}

		public static void AttachToEvent(FieldInfo iconField, EventInfo iconEvent, MethodInfo addEvent, MethodInfo removeEvent, MethodInfo drawInfo)
		{
			MulticastDelegate eventDelegate = (MulticastDelegate)iconField.GetValue(null);
			Delegate oldSelfDelegate = null;
			Delegate[] oldDelegates = null;
			if (eventDelegate != null)
			{
				oldDelegates = new Delegate[eventDelegate.GetInvocationList().Length];
				int index = 0;
				foreach (Delegate currentHandler in eventDelegate.GetInvocationList())
				{
					if (currentHandler.Method == drawInfo)
					{
						oldSelfDelegate = currentHandler;
					}
					oldDelegates[index++] = currentHandler;
				}
			}

			// Remove previous event
			if (oldSelfDelegate != null)
			{
				removeEvent.Invoke(null, new System.Object[] { oldSelfDelegate });
			}

			// Remove all other events
			if (oldDelegates != null)
			{
				foreach (Delegate del in oldDelegates)
				{
					if (oldSelfDelegate != del)
					{
						removeEvent.Invoke(null, new System.Object[] { del });
					}
				}
			}

			// Register delegate to event
			Delegate newHandler = Delegate.CreateDelegate(iconEvent.EventHandlerType, null, drawInfo);
			addEvent.Invoke(null, new System.Object[] { newHandler });

			// Re-add other event AFTER this one
			if (oldDelegates != null)
			{
				foreach (Delegate del in oldDelegates)
				{
					if (oldSelfDelegate != del)
					{
						addEvent.Invoke(null, new System.Object[] { del });
					}
				}
			}
		}

		public static Texture GetIcon(TreeViewItem tvi, bool oneColumn)
		{
			if (tvi.depth > 0 || oneColumn)
			{
				string path = AssetDatabase.GetAssetPath(tvi.id);
				return GetIconForPath(path, false);
			}

			return null;
		}

		/// <summary>
		/// Called when project view grid displays icons
		/// </summary>
		public static void OnPostIconDraw(Rect iconRect, string guid, bool isListMode)
		{
			Texture2D folderIcon = GetIconForPath(AssetDatabase.GUIDToAssetPath(guid), !isListMode);
			if(folderIcon != null)
			{
				Rect newRect = (Rect) ActualImageDrawPosition.Invoke(null, new object[] { iconRect, folderIcon.width, folderIcon.height });

				// Draw rectangle to cover previous icons
				if (!isListMode)
				{
					if (iconBackgroundStyle == null)
					{
						iconBackgroundStyle = "ProjectBrowserIconAreaBg";
					}

					GUI.Label(iconRect, GUIContent.none, iconBackgroundStyle);
				}

				GUI.DrawTexture(newRect, folderIcon, ScaleMode.ScaleToFit);
			}
		}

		/// <summary>
		/// Called when project view tree displays icons
		/// </summary>
		public static void OnPostIconDrawTree(Rect iconRect, string guid)
		{
			Texture2D folderIcon = GetIconForPath(AssetDatabase.GUIDToAssetPath(guid), false);
			if (folderIcon != null)
			{
				Rect newRect = (Rect)ActualImageDrawPosition.Invoke(null, new object[] { iconRect, folderIcon.width, folderIcon.height });
				GUI.DrawTexture(newRect, folderIcon, ScaleMode.ScaleToFit);
			}
		}

		/// <summary>
		/// Returns an icon for an asset path
		/// </summary>
		/// <param name="path">Asset path to get icon from</param>
		/// <param name="highRes">Should returned texture be high res</param>
		/// <returns></returns>
		public static Texture2D GetIconForPath(string path, bool highRes)
		{
			if (AssetDatabase.IsValidFolder(path))
			{
				string userData = AssetImporter.GetAtPath(path).userData;
				string[] guids = userData.Split(';');
				if (guids.Length == 2)
				{
					Texture2D folderIcon = (Texture2D)AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(guids[highRes ? 1 : 0]), typeof(Texture2D));
					if(PreferencesUI.folderIconEnabled)
					{
						return folderIcon;
					}
					else
					{
						return null;
					}
				}
			}

			return null;
		}

		/// <summary>
		/// Repaint all project views
		/// </summary>
		public static void RepaintProjectViews()
		{
			IList opennedBrowsers = (IList)opennedBrowsersField.GetValue(null);

			// For each openned project browser repaint it
			foreach (object projectView in opennedBrowsers)
			{
				repaintBrowsers.Invoke(projectView, null);
			}
		}

		/// <summary>
		/// Init project view bindings
		/// </summary>
		private static void Init()
		{
			// Bind cached methods
			Type localGroupType = Type.GetType("UnityEditor.ObjectListArea+LocalGroup,UnityEditor.dll");
			ActualImageDrawPosition = localGroupType.GetMethod("ActualImageDrawPosition", BindingFlags.Static | BindingFlags.NonPublic);

			// Bind tree types
			projectBrowserType = Type.GetType("UnityEditor.ProjectBrowser,UnityEditor.dll");
			folderTreeField = projectBrowserType.GetField("m_FolderTree", BindingFlags.Instance | BindingFlags.NonPublic);
			assetTreeField = projectBrowserType.GetField("m_AssetTree", BindingFlags.Instance | BindingFlags.NonPublic);

			opennedBrowsersField = projectBrowserType.GetField("s_ProjectBrowsers", BindingFlags.Static | BindingFlags.NonPublic);
			repaintBrowsers = projectBrowserType.GetMethod("Repaint", BindingFlags.Instance | BindingFlags.Public);

			// Bind with Event
			objectListAreaType = Type.GetType("UnityEditor.ObjectListArea,UnityEditor.dll");
			onPostIconDrawMethod = typeof(CustomProjectView).GetMethod("OnPostIconDraw");
			postAssetIconEvent = objectListAreaType.GetEvent("postAssetIconDrawCallback", BindingFlags.Static | BindingFlags.NonPublic);
			postAssetIconField = objectListAreaType.GetField("postAssetIconDrawCallback", BindingFlags.Static | BindingFlags.NonPublic);

			postAssetIconEventAdd = postAssetIconEvent.GetAddMethod(true);
			postAssetIconEventRemove = postAssetIconEvent.GetRemoveMethod(true);

			assetTreeView = Type.GetType("UnityEditor.AssetsTreeViewGUI,UnityEditor.dll");
			onPostIconDrawMethod2 = typeof(CustomProjectView).GetMethod("OnPostIconDrawTree");
			postAssetIconEvent2 = assetTreeView.GetEvent("postAssetIconDrawCallback", BindingFlags.Static | BindingFlags.NonPublic);
			postAssetIconField2 = assetTreeView.GetField("postAssetIconDrawCallback", BindingFlags.Static | BindingFlags.NonPublic);

			postAssetIconEventAdd2 = postAssetIconEvent2.GetAddMethod(true);
			postAssetIconEventRemove2 = postAssetIconEvent2.GetRemoveMethod(true);
		}
	}
}
