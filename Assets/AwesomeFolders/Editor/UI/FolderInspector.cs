using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


namespace AwesomeFolders
{
	[CustomEditor(typeof(DefaultAsset))]
	public class FolderInspector : Editor
	{
		public static bool selectedByContext;
		public static Editor lastWindow;
		private bool isFolder;
		private AssetImporter folderImporter;

		private NewFolderStyleInspector nfsi;
		private StyleGrid customStyles;

		protected virtual void OnEnable()
		{
			string path = AssetDatabase.GetAssetPath(target);
			folderImporter = AssetImporter.GetAtPath(path);
			string userData = folderImporter.userData;

			if (AssetDatabase.IsValidFolder(path))
			{
				isFolder = true;
				lastWindow = this;
			}

			ResourceUtil.Refresh();
			customStyles = new StyleGrid(ResourceUtil.CustomStylesPath, 64.0F + 16.0F);
		}

		protected virtual void OnDisable()
		{
			selectedByContext = false;
		}

		protected override bool ShouldHideOpenButton()
		{
			return true;
		}

		public override void OnInspectorGUI()
		{
			// Not a folder or not openned with context menu
			if (!isFolder || !EditorPrefs.GetBool("ext_" + ResourceUtil.ExtensionName + "_enabled") || (!selectedByContext && !EditorPrefs.GetBool("ext_" + ResourceUtil.ExtensionName + "_simpleclick")))
			{
				base.OnInspectorGUI();
				return;
			}

			EditorGUI.EndDisabledGroup(); // Everything disabled fix

			// Is sub window activated ?
			if(nfsi != null)
			{
				nfsi.OnInspectorGUI(this);
				return;
			}

			// Custom style foldout

			EditorGUILayout.BeginVertical(EditorStyles.helpBox);
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Styles", EditorStyles.boldLabel);
			if (GUILayout.Button("New", EditorStyles.miniButton, GUILayout.ExpandWidth(true), GUILayout.Width(40)))
			{
				nfsi = new NewFolderStyleInspector();
				return;
			}
			EditorGUILayout.EndHorizontal();

			ButtonGrid.ButtonAction styleAction = customStyles.DrawGrid();
			if(styleAction != null)
			{
				if(styleAction.IsLeftClick)
				{
					ApplyStyle((StyleGrid.StyleElement) styleAction.ClickedElement);
				}
				else
				{
					GenericMenu menu = new GenericMenu();
					menu.AddItem(new GUIContent("Edit"), false, EditStyle, (StyleGrid.StyleElement) styleAction.ClickedElement);
					menu.AddItem(new GUIContent("Delete"), false, AskDeleteStyle, (StyleGrid.StyleElement)styleAction.ClickedElement);
					menu.ShowAsContext();
				}
			}

			EditorGUILayout.EndVertical();

			GUILayout.Space(10);

			EditorGUILayout.BeginHorizontal();

			GUILayout.FlexibleSpace();
			if (GUILayout.Button("Reset", GUILayout.Width(80.0F), GUILayout.ExpandWidth(true)))
			{
				ApplyStyle(null);
			}
		
		

			EditorGUILayout.EndHorizontal();
		}

		public void ApplyStyle(StyleGrid.StyleElement style)
		{
			folderImporter.userData = style == null ? "" : AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(style.LowResTex)) + ";" + AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(style.HighResTex));
			folderImporter.SaveAndReimport();
			CustomProjectView.RepaintProjectViews();
		}

		public void EditStyle(object style)
		{
			nfsi = new NewFolderStyleInspector((StyleGrid.StyleElement) style);
		}

		public void AskDeleteStyle(object style)
		{
			StyleGrid.StyleElement theStyle = (StyleGrid.StyleElement) style;
			if(EditorUtility.DisplayDialog("Delete Style?", "Are you sure you want to delete style \"" + theStyle.Name + "\"?", "Yes", "No"))
			{
				DeleteStyle(theStyle);
			}
		}

		public void DeleteStyle(StyleGrid.StyleElement style)
		{
			AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(style.HighResTex));
			AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(style.LowResTex));
			AssetDatabase.DeleteAsset(ResourceUtil.CustomStylesPath + "/" + ColorUtils.ColorToInt(style.MainColor) + ";" + style.IconId + ";" + style.Name + "_prop.json");

			customStyles.Init();
		}

		public void CloseSubWindow()
		{
			nfsi = null;
			GUI.FocusControl(null);
			customStyles.Init();
		}

		[MenuItem("Assets/Folder Icon...", false, 10000)]
		private static void OnMenuItemClicked()
		{
			selectedByContext = true;
			if(lastWindow != null)
			{
				lastWindow.Repaint();
			}
		}

		[MenuItem("Assets/Folder Icon...", true, 10000)]
		public static bool ValidateIsFolder()
		{
			if(Selection.activeObject != null && Selection.activeObject is DefaultAsset)
			{
				string path = AssetDatabase.GetAssetPath(Selection.activeObject);

				if (AssetDatabase.IsValidFolder(path))
				{
					return true;
				}
			}
			return false;
		}
	}
}
