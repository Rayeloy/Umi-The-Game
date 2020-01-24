using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


namespace AwesomeFolders
{
	public class FolderInspector : EditorWindow
	{
		private static EditorWindow currentWindow;

		private static bool selectedByContext;
		private static SelectionStatus selectionStatus;

		private static AssetImporter folderImporter;
		private static StyleGrid customStyles;
		private static string userData;

		private static NewFolderStyleInspector nfsi;
		private Vector2 currentScrollPos;

		// EditorStyles
		private static bool isStyleInit;
		public static GUIStyle titleBarStyle;
		public static GUIStyle titleBarButtonStyle;
		public static GUIStyle titleBarTextFieldStyle;
		public static GUIStyle titleBarLabelStyle;
		public static GUIStyle titleBarLabelCenteredStyle;
		public static GUIStyle toolBarStyle;
		public static GUIStyle toolBarButtonStyle;
		public static GUIStyle toolBarLabelStyle;
		public static GUIStyle toolBarPopupStyle;
		public static GUIStyle styleListBackgroundStyle;

		[InitializeOnLoadMethod]
		static void OnReload()
		{
			OnObjectSelected();

			Selection.selectionChanged += OnObjectSelected;
		}

		protected static void OnObjectSelected()
		{
			selectedByContext = false;
			TryOpenSelection();

			if (currentWindow != null)
			{
				currentWindow.Repaint();
			}
		}

		protected static void TryOpenSelection()
		{
			DefaultAsset[] folders = Selection.GetFiltered<DefaultAsset>(SelectionMode.Assets);
			if(folders.Length == 0)
			{
				selectionStatus = SelectionStatus.NONE;
				return;
			}

			if (folders.Length > 1)
			{
				selectionStatus = SelectionStatus.TOO_MANY;
				return;
			}

			string path = AssetDatabase.GetAssetPath(folders[0]);
			folderImporter = AssetImporter.GetAtPath(path);
			userData = folderImporter.userData;

			if (AssetDatabase.IsValidFolder(path))
			{
				selectionStatus = SelectionStatus.VALID;
			}
			else
			{
				selectionStatus = SelectionStatus.NONE;
			}

			ResourceUtil.Refresh();
			customStyles = new StyleGrid(ResourceUtil.CustomStylesPath, 64.0F + 16.0F, 8.0F);

			FolderInspector[] openedWindows = Resources.FindObjectsOfTypeAll<FolderInspector>();
			if (openedWindows.Length > 0)
			{
				currentWindow = openedWindows[0];
			}

			if (CanOpenWindow())
			{
				OpenWindow();
			}
		}

		private static bool CanOpenWindow()
		{
			return PreferencesUI.folderIconEnabled && (selectedByContext || PreferencesUI.simpleClickEnabled);
		}

		public void OnGUI()
		{
			ResourceUtil.Refresh();
			InitEditorStyles();

			// Is sub window activated ?
			if (nfsi != null)
			{
				currentScrollPos = EditorGUILayout.BeginScrollView(currentScrollPos);
				nfsi.OnInspectorGUI(this);

				EditorGUILayout.EndScrollView();
				return;
			}

			if(!TitleBarGUI())
			{
				return;
			}

			// Custom style foldout
			currentScrollPos = EditorGUILayout.BeginScrollView(currentScrollPos);

			EditorGUILayout.BeginHorizontal(toolBarStyle);
			EditorGUILayout.LabelField("Project styles:", toolBarLabelStyle);
			GUILayout.FlexibleSpace();

			if (GUILayout.Button(GUIHelper.ContentFromEditor("New", "Toolbar Plus"), toolBarButtonStyle))
			{
				nfsi = new NewFolderStyleInspector(null, false);
			}
			EditorGUILayout.EndHorizontal();

			GUILayout.BeginVertical(styleListBackgroundStyle);
			GUILayout.Space(6);
			ButtonGrid.ButtonAction styleAction = customStyles.DrawGrid();
			if (styleAction != null)
			{
				if (styleAction.IsLeftClick)
				{
					ApplyStyle((StyleGrid.StyleElement)styleAction.ClickedElement);
				}
				else
				{
					GenericMenu menu = new GenericMenu();
					menu.AddItem(new GUIContent("Create variant"), false, CreateVariant, (StyleGrid.StyleElement)styleAction.ClickedElement);
					menu.AddItem(new GUIContent("Edit"), false, EditStyle, (StyleGrid.StyleElement)styleAction.ClickedElement);
					menu.AddItem(new GUIContent("Delete"), false, AskDeleteStyle, (StyleGrid.StyleElement)styleAction.ClickedElement);
					menu.ShowAsContext();
				}
			}
			GUILayout.Space(2);
			GUILayout.EndVertical();

			EditorGUILayout.BeginHorizontal(toolBarStyle);
			GUILayout.FlexibleSpace();

			/*if (GUILayout.Button("Download Style packs", toolBarButtonStyle))
			{
				//nfsi = new NewFolderStyleInspector();
			}*/
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.EndScrollView();
		}

		private bool TitleBarGUI()
		{
			GUILayout.BeginHorizontal(titleBarStyle);

			// Not a folder or not openned with context menu
			GUIContent warningContent = EditorGUIUtility.IconContent("console.warnicon.sml");
			if (selectionStatus == SelectionStatus.NONE)
			{
				warningContent.text = " No folder selected";
				EditorGUILayout.LabelField(warningContent);
				GUILayout.EndHorizontal();
				return false;
			}

			if (selectionStatus == SelectionStatus.TOO_MANY)
			{
				warningContent.text = " Please select only one folder at a time.";
				EditorGUILayout.LabelField(warningContent);
				GUILayout.EndHorizontal();
				return false;
			}

			string folderName = " " + Selection.GetFiltered<DefaultAsset>(SelectionMode.Assets)[0].name;
			GUIContent folderGuiContent = GUIHelper.ContentFromAssets(folderName, ResourceUtil.TexturesPath + "/folder_icon_16.png");

			// Get folder actual texture
			if (userData.Length > 0)
			{
				string[] guids = userData.Split(';');
				if (guids.Length == 2)
				{
					Texture folderTexture = GUIHelper.LoadTexture(AssetDatabase.GUIDToAssetPath(guids[0]));
					if(folderTexture != null)
					{
						folderGuiContent.image = folderTexture;
					}
				}
			}

			GUILayout.Label(folderGuiContent);

			if (GUILayout.Button("Reset folder style", titleBarButtonStyle, GUILayout.Width(120.0F), GUILayout.ExpandWidth(false)))
			{
				ApplyStyle(null);
			}

			GUILayout.EndHorizontal();

			return true;
		}

		private static void InitEditorStyles()
		{
			if (!isStyleInit)
			{
				isStyleInit = true;
				titleBarStyle = new GUIStyle("Toolbar")
				{
					fixedHeight = 24
				};

				titleBarButtonStyle = new GUIStyle("ToolbarButton")
				{
					fixedHeight = 23
				};
				
				titleBarTextFieldStyle = new GUIStyle("TextFieldDropDownText")
				{
					fixedHeight = 19
				};

				titleBarLabelStyle = new GUIStyle("Label")
				{
					margin = new RectOffset(0, 0, 4, 0)
				};

				titleBarLabelCenteredStyle = new GUIStyle("Label")
				{
					margin = new RectOffset(0, 0, 4, 0),
					alignment = TextAnchor.MiddleCenter,
					fontStyle = FontStyle.Bold
				};

				toolBarStyle = new GUIStyle("Toolbar");
				toolBarButtonStyle = new GUIStyle("ToolbarButton");
				toolBarPopupStyle = new GUIStyle("ToolbarPopup");
				toolBarLabelStyle = new GUIStyle("Label")
				{
					margin = new RectOffset(0, 0, 0, 2),
					contentOffset = new Vector2(0, -2)					
				};

				styleListBackgroundStyle = new GUIStyle("CurveEditorBackground")
				{
					stretchHeight = false
				};
			}
		}

		public void ApplyStyle(StyleGrid.StyleElement style)
		{
			folderImporter.userData = style == null ? "" : AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(style.LowResTex)) + ";" + AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(style.HighResTex));
			folderImporter.SaveAndReimport();
			TryOpenSelection();
			CustomProjectView.RepaintProjectViews();
		}

		public void CreateVariant(object style)
		{
			nfsi = new NewFolderStyleInspector((StyleGrid.StyleElement)style, true);
		}

		public void EditStyle(object style)
		{
			nfsi = new NewFolderStyleInspector((StyleGrid.StyleElement)style, false);
		}

		public void AskDeleteStyle(object style)
		{
			StyleGrid.StyleElement theStyle = (StyleGrid.StyleElement)style;
			if (EditorUtility.DisplayDialog("Delete Style?", "Are you sure you want to delete style \"" + theStyle.Name + "\"?", "Yes", "No"))
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

		private static void OpenWindow()
		{
			currentWindow = GetWindow<FolderInspector>();

			GUIContent windowTitle = new GUIContent("Folder Icon");
			windowTitle.image = AssetDatabase.LoadAssetAtPath<Texture>(ResourceUtil.TexturesPath + "/windowIcon.png");
			currentWindow.titleContent = windowTitle;
			currentWindow.Show();

			currentWindow.Focus();
		}

		[MenuItem("Assets/Folder Icon...", false, 10000)]
		private static void OnMenuItemClicked()
		{
			selectedByContext = true;
			nfsi = null;

			TryOpenSelection();

			if (currentWindow != null)
			{
				currentWindow.Repaint();
			}
		}

		[MenuItem("Assets/Folder Icon...", true, 10000)]
		public static bool ValidateIsFolder()
		{
			if (Selection.activeObject != null && Selection.activeObject is DefaultAsset)
			{
				string path = AssetDatabase.GetAssetPath(Selection.activeObject);

				if (AssetDatabase.IsValidFolder(path))
				{
					return true;
				}
			}
			return false;
		}

		private enum SelectionStatus
		{
			NONE,
			VALID,
			TOO_MANY
		}
	}
}
