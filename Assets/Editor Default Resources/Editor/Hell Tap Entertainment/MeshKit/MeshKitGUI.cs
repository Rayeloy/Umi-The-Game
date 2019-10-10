////////////////////////////////////////////////////////////////////////////////////////////////
//
//  MeshKitGUI.cs
//
//	Combines Meshes of all children objects at runtime
//
//	© 2015 - 2019 Melli Georgiou.
//	Hell Tap Entertainment LTD
//
////////////////////////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;
using System.Collections;
using HellTap.MeshKit;

// Use HellTap Namespace
namespace HellTap.MeshKit {

	// Class
	public class MeshKitGUI : EditorWindow {

		public static bool verbose = false;			// VERBOSE FLAG (All editor scripts look at this!)
		public static bool useSmallIcons = true;	// GUI option to show small icons so everything fits in a single row

		////////////////////////////////////////////////////////////////////////////////////////////////
		//	VARIABLES
		////////////////////////////////////////////////////////////////////////////////////////////////

		// This Window
		public static EditorWindow window;
		public static string versionString = "2.0.5";
		Event m_Event = null;

		// Icon Textures ( We load their textures in OnEnable() )
		public static Texture2D hellTapIcon, meshKitIcon, meshkitText, goIcon, splitIcon, invertIcon, doubleSidedIcon, rebuildIcon, combineIcon, decimateIcon, lodIcon, cleanIcon, warningIcon, upButton, downButton, deleteIcon, defaultIcon, gearIcon; 

		// Internal Icons
		public static Texture2D meshFilterIcon, skinnedMeshRendererIcon = null;

		// Window Helpers / Settings
		public static bool showOptions = false;			// Should we show advanced options to the user?
		bool updateProgressBars = false;				// Are we updating Progress bars [turns repaints on!]
		int selGridInt = 0;								// Which mode are we using?
		bool disableMainButton = false;					// Should the core MeshKit Button be disabled?
		bool showBottomSeperator = true;				// Should the bottom Seperator Line be shown?

		// GameObject Info
		int children = 0;
		int meshfilters = 0;
		int skinnedMeshRenderers = 0;

		// Selection Options
		bool optionThisGameObjectOnly = false;			// Only apply selected tool to the current GameObject [Not child objects]
		bool optionUseMeshFilters = true;				// Only use Mesh Filters
		bool optionUseSkinnedMeshRenderers = false;		// Only use Skinned Mesh Renderers

		// Rebuild Options
		bool optionStripNormals = false;
		bool optionStripTangents = false;
		bool optionStripColors = false;
		bool optionStripLightmapUVs = false;
		bool optionStripUV3 = false;
		bool optionStripUV4 = false;
		bool optionStripUV5 = false;
		bool optionStripUV6 = false;
		bool optionStripUV7 = false;
		bool optionStripUV8 = false;
		bool optionRebuildNormals = false;
		float optionRebuildNormalsAngle = 60.0f;
		bool optionRebuildTangents = false;
		bool optionRebuildLightmapUVs = false;

		// Combine Options
		int optionMaxVertices = 65534;					//	This should be the same as MeshCombiner.maxVertices;
		bool optionRestoreBindposes = false;			//	Attempts to restore skinned mesh renderer bindposes before combining

		// Decimator Options
		float optionDecimatorQuality = 0.8f;
		bool optionDecimatorRecalculateNormals = false;
		bool optionPreserveBorders = false;
		bool optionPreserveSeams = false;
		bool optionPreserveFoldovers = false;

		// Auto-LOD Options / Helpers
		MeshKitAutoLOD selectedAutoLOD = null;
		int howManyMeshKitAutoLODComponentsInChildren = 0;
		int howManyLODGroupComponentsInChildren = 0;
		bool shouldCreateNewMeshKitAutoLOD = false;
		bool LODSystemExistsInParent = false;			// This is true if an LODGroup or MeshKitAutoLOD component exists.

		// Editor Window
		public Vector2 scrollPosition = Vector2.zero;

		// Run Tool At End Helper (this helps avoid GUI layout error in Unity 2017.x)
		static RunToolAtEnd runToolAtEnd = RunToolAtEnd.No;
		public enum RunToolAtEnd{ No, Seperator, Invert, DoubleSided, Rebuild, Combine, Decimate, AutoLOD, Clean }

		// * Experimental features
		private static bool useExperimentalFeatures = false;	// Use experimental MeshKit features (unsupported)
		


		////////////////////////////////////////////////////////////////////////////////////////////////
		//	ON ENABLE
		//	Load the Texture Icons
		////////////////////////////////////////////////////////////////////////////////////////////////

		void OnEnable(){

			// Setup GUI Icons
			hellTapIcon = EditorGUIUtility.Load("Hell Tap Entertainment/MeshKit/Shared/HellTapEditor.png") as Texture2D; 
			meshKitIcon = EditorGUIUtility.Load("Hell Tap Entertainment/Meshkit/Icons/meshkit.png" ) as Texture2D; 	 	
			meshkitText = EditorGUIUtility.Load("Hell Tap Entertainment/Meshkit/Icons/meshkit_text.png") as Texture2D; 
			goIcon = EditorGUIUtility.Load("Hell Tap Entertainment/Meshkit/Icons/gameObjectIcon.png") as Texture2D; 

			splitIcon = EditorGUIUtility.Load("Hell Tap Entertainment/Meshkit/Icons/split.png") as Texture2D; 
			invertIcon = EditorGUIUtility.Load("Hell Tap Entertainment/Meshkit/Icons/invert.png") as Texture2D; 
			doubleSidedIcon = EditorGUIUtility.Load("Hell Tap Entertainment/Meshkit/Icons/doubleSided.png") as Texture2D;
			rebuildIcon = EditorGUIUtility.Load("Hell Tap Entertainment/Meshkit/Icons/rebuild.png") as Texture2D;
			combineIcon = EditorGUIUtility.Load("Hell Tap Entertainment/Meshkit/Icons/combine.png") as Texture2D; 
			decimateIcon = EditorGUIUtility.Load("Hell Tap Entertainment/Meshkit/Icons/decimate.png") as Texture2D; 
			lodIcon = EditorGUIUtility.Load("Hell Tap Entertainment/Meshkit/Icons/autoLOD.png") as Texture2D; 
			cleanIcon = EditorGUIUtility.Load("Hell Tap Entertainment/Meshkit/Icons/clean.png") as Texture2D;

			warningIcon = EditorGUIUtility.Load("Hell Tap Entertainment/Meshkit/Icons/warningIcon.png") as Texture2D;
			upButton = EditorGUIUtility.Load("Hell Tap Entertainment/Meshkit/Icons/upButton.png") as Texture2D; 
			downButton = EditorGUIUtility.Load("Hell Tap Entertainment/Meshkit/Icons/downButton.png") as Texture2D;
			deleteIcon = EditorGUIUtility.Load("Hell Tap Entertainment/Meshkit/Icons/deleteIcon.png") as Texture2D;
			defaultIcon = EditorGUIUtility.Load("Hell Tap Entertainment/Meshkit/Icons/defaultIcon.png") as Texture2D;
			gearIcon = EditorGUIUtility.Load("Hell Tap Entertainment/Meshkit/Icons/gearIcon.png") as Texture2D;

			// Internal Icons
			meshFilterIcon = EditorGUIUtility.IconContent("MeshFilter Icon").image as Texture2D;
			skinnedMeshRendererIcon = EditorGUIUtility.IconContent("SkinnedMeshRenderer Icon").image as Texture2D;

			// Set Verbose Mode depending on the value saved in Editor Preferences
			MeshKitGUI.verbose = EditorPrefs.GetBool ("MeshKitVerboseMode", false);
			MeshKitGUI.useSmallIcons = EditorPrefs.GetBool ("MeshKitUseSmallIcons", true);

			// Subscribe to Undo's and when Hierarchy Changes
			Undo.undoRedoPerformed += OnSelectionChange;
		}

		void OnDisable(){

			// Unsubscribe from Undo's
			Undo.undoRedoPerformed -= OnSelectionChange;
		}

		////////////////////////////////////////////////////////////////////////////////////////////////
		//	OPEN WINDOW
		////////////////////////////////////////////////////////////////////////////////////////////////

		[MenuItem ("Window/MeshKit")]
		public static void ShowWindow () {
			window = EditorWindow.GetWindow(typeof(MeshKitGUI));
		//    window.minSize = new Vector2(580f,128);
		//    window.title = " MeshKit";
			HTGUI.SetWindowTitle( window, hellTapIcon, " MeshKit");

		}

		////////////////////////////////////////////////////////////////////////////////////////////////
		//	ON SELECTION CHANGE
		////////////////////////////////////////////////////////////////////////////////////////////////

		void OnHierarchyChange(){ OnSelectionChange(); }

		// Did we choose another GameObject?
		void OnSelectionChange(){

			// Make sure the editor isn't playing
			if( !EditorApplication.isPlaying ){

				// If we have selected a valid gameobject, update its children and meshilter count to a local int.
				if( Selection.activeGameObject != null && Selection.activeGameObject.activeInHierarchy ){
					children = Selection.activeGameObject.GetComponentsInChildren(typeof(Transform)).Length ;
					meshfilters = Selection.activeGameObject.GetComponentsInChildren(typeof(MeshFilter)).Length ;
					skinnedMeshRenderers = Selection.activeGameObject.GetComponentsInChildren(typeof(SkinnedMeshRenderer)).Length ;

					// Auto-LOD helpers
					selectedAutoLOD = Selection.activeGameObject.GetComponent<MeshKitAutoLOD>();
					howManyMeshKitAutoLODComponentsInChildren = Selection.activeGameObject.GetComponentsInChildren(typeof(MeshKitAutoLOD)).Length ;
					howManyLODGroupComponentsInChildren = Selection.activeGameObject.GetComponentsInChildren(typeof(LODGroup)).Length ;
					LODSystemExistsInParent = CheckForLODSystemInParentObjects( Selection.activeGameObject );
				}

				// Repaint the window when the selection has changed.
				Repaint();

			}
		}

		////////////////////////////////////////////////////////////////////////////////////////////////
		//	CHECK FOR LOD SYSTEM IN PARENT OBJECTS
		////////////////////////////////////////////////////////////////////////////////////////////////

		bool CheckForLODSystemInParentObjects( GameObject go ){
			if( go != null ){

				// Start at the current Transform
				Transform currentTransform = go.transform;

				// Loop through the parent objects
				while( currentTransform.parent != null ){

					// Move to the next parent object in the Hierarchy
					currentTransform = currentTransform.parent;

					// See if the parent has an LODGroup or MeshKitAutoLOD component
					if( currentTransform.GetComponent<LODGroup>() != null ||
						currentTransform.GetComponent<MeshKitAutoLOD>() != null
					){
						return true;
					}
				}
			}

			// If we didn't find anything
			return false;
		}


		////////////////////////////////////////////////////////////////////////////////////////////////
		//	ON GUI
		//	The Inspector Controls
		////////////////////////////////////////////////////////////////////////////////////////////////

		// Bugfix for caching the window width and height
		static float _editorWindowWidth = 595;
		static float _editorWindowHeight = 574;

		void OnGUI () {

			// If the MeshKit Window should be open but isn't, open it
			// NOTE: This happens in the editor when a user deletes / creates a script. 
			// It removes the MeshKitGUI.window reference.
			if( MeshKitGUI.window == null ){ MeshKitGUI.ShowWindow(); }

			// Cache the width and height of the editor window
			_editorWindowWidth = MeshKitGUI.window.position.width;
			_editorWindowHeight = MeshKitGUI.window.position.height;

			// Update MeshKit Preferences
			MeshKitGUI.verbose = EditorPrefs.GetBool ("MeshKitVerboseMode", false);
			MeshKitGUI.useSmallIcons = EditorPrefs.GetBool ("MeshKitUseSmallIcons", true);

			// Make sure that by default, we're not running any tools at end
			runToolAtEnd = RunToolAtEnd.No;

			// Handle GUI Window scrolling
			m_Event = Event.current;
			if ( m_Event.type == EventType.ScrollWheel ){
				scrollPosition += (m_Event.delta * 2f);
				Repaint ();
			}

			// Setup Scrollview
			scrollPosition = EditorGUILayout.BeginScrollView( scrollPosition, GUILayout.MinWidth(580), GUILayout.Width (_editorWindowWidth), GUILayout.Height ( _editorWindowHeight ) );

				// Is this a backed up scene
				bool isThisSceneABackup = MeshAssets.IsThisABackedUpMeshKitScene();

				// ================================
				//	HEADER
				// ================================

				// Start Big Layout
				HTGUI.StartBigLayout();

					// Header Section
					GUILayout.Space(8);
					GUILayout.BeginHorizontal();

						// Setup the infoLabel style so we can use rich text
						GUIStyle infoLabelStyle = new GUIStyle (GUI.skin.label);
						infoLabelStyle.richText = true;

						// Column 1 - Select A tool
						GUILayout.BeginVertical( GUILayout.MaxHeight(32) );

							GUILayout.FlexibleSpace();
							GUILayout.Space(8);
						
							// Show info about selected GameObject
							GUILayout.BeginHorizontal();

								// Show the GameObject Icon and then the information of the object
								GUILayout.Label(goIcon, GUILayout.Width(32), GUILayout.Height(32) );
								GUILayout.BeginVertical();

									// Make sure the editor isn't playing
									if( EditorApplication.isPlaying ){
										GUILayout.Label("Meshkit is not available\nwhile Unity is in Play Mode.", "BoldLabel");

									// If this is a backed up scene
									} else if( isThisSceneABackup == true ){
										GUILayout.Label("Meshkit is not available\nin backed up Unity scenes.", "BoldLabel");
										GUILayout.Label("To work with this scene,\nduplicate it and move it out\nof the MeshKit/Backup Scene\nfolder.");

									// Make sure this Scene is saved first.
									} else if( HTScene.CurrentScene() == "" ){
										GUILayout.Label("You must save this Unity\nScene before MeshKit\nbecomes available.", "BoldLabel");

									// Make sure we have chosen an object and it is not in the Project pane.
									} else if(Selection.activeGameObject != null && Selection.activeGameObject.activeInHierarchy ){

										// Shorten the name if its too long...
										if( Selection.activeGameObject.name.Length > 20 ){
											GUILayout.Label("<b><size=12>"+ Selection.activeGameObject.name.Substring(0,19)+"...</size></b>", infoLabelStyle);
										} else {
											GUILayout.Label("<b><size=12>"+ Selection.activeGameObject.name+"</size></b>", infoLabelStyle);
										}
										GUILayout.Label("<size=10>Children: "+children.ToString()+"\nMeshFilters: "+meshfilters.ToString()+"\nSkinnedMeshRenderers: "+skinnedMeshRenderers.ToString()+"</size>", infoLabelStyle);

										

									// Show message saying this message needs to be a valid gameobject!	
									} else {
										GUILayout.Label("MeshKit requires an active\nGameObject selected in\nthe Hierarchy.", "BoldLabel");
									}

								// End Column	
								GUILayout.EndVertical();

							// End Horizontal	
							GUILayout.EndHorizontal();
							
							//GUILayout.FlexibleSpace();
						GUILayout.EndVertical();

						// Column 2 - Meshkit
						GUILayout.BeginVertical();
							
							// Setup horizontal flexible space and show the meshkit logo
							GUILayout.BeginHorizontal();
								GUILayout.FlexibleSpace();

								// Create a new column
								GUILayout.BeginVertical();
									GUILayout.Label(meshkitText);
									infoLabelStyle.alignment = TextAnchor.UpperRight;
									GUILayout.Label("<size=9>© 2018 Hell Tap Entertainment LTD\nversion: "+versionString+"</size>", infoLabelStyle);
								GUILayout.EndVertical();

								// Draw MeshKit Icon
								GUILayout.Label(meshKitIcon, GUILayout.MaxWidth(64), GUILayout.MaxHeight(64) );
							GUILayout.EndHorizontal();
							GUILayout.Space(4);

						GUILayout.EndVertical();

					GUILayout.EndHorizontal();
					GUILayout.Space(8);

					// ================================
					//	WHITE BOX
					// ================================

					// Make sure Editor isn't playing, this scene isnt a backup and we have chosen an object and it is not in the Project pane.
					if( !EditorApplication.isPlaying && !isThisSceneABackup && Selection.activeGameObject != null && Selection.activeGameObject.activeInHierarchy && HTScene.CurrentScene() != "" ){

					// Start White Box
					HTGUI.StartWhiteBox();

						// Select A Tool
						GUILayout.Label("Select A Tool", "boldLabel");
						GUILayout.Space(8);
						
						// Create a horizontal row
						GUILayout.BeginHorizontal();

							// Add Flexible Space
						//	GUILayout.Space(4);
							GUILayout.FlexibleSpace();

							// ================================
							//	SELECTION GRID - BUTTONS
							// ================================

							// Setup the buttons as GUIContents
							GUIContent[] selContent = { 
														new GUIContent("\nSeperate", splitIcon, "Seperates multiple submeshes in this GameObject and its children. This function extracts the submeshes and completely rebuilds them from scratch to be new independant Meshes, each containing only 1 material per object. Renderers that are disabled will be skipped."), 
														
														new GUIContent("\nInvert", invertIcon, "Inverts all Meshes in this GameObject and its children. Makes the mesh of the object appear inside-out. Great for showing the interior of a structure like a house or room."), 
														new GUIContent("\nTwo-Sided", doubleSidedIcon, "Creates Double Sided Meshes in this GameObject and its children. Similar to Invert but you will still be able to view the object from the original perspective. Please note that this increases the polycount of the object so be careful not to apply it multiple times to the same object."), 
														new GUIContent("\nRebuild", rebuildIcon, "Takes an existing mesh and can rebuild or strip specific features. This function skips any GameObjects that have submeshes or Renderers that are disabled."),
														new GUIContent("\nCombine", combineIcon, "Combines all Meshes in this GameObject and its children. This function skips any GameObjects that have submeshes or Renderers that are disabled."), 
														new GUIContent("\nDecimate", decimateIcon, "Reduces the polygons of all meshes in this GameObject and its children. If gaps appear in problematic meshes, try preserving borders, seams and UV crossovers."), 
														new GUIContent("\nAuto LOD", lodIcon, "Automatically creates LODs by creating decimated versions of all meshes in this GameObject and its children. By default, 3 LODs will be created. Custom settings may be applied by using the Advanced Settings."), 
														new GUIContent("\nClean Scene", cleanIcon, "Delete old Meshes that have been created in this scene but are no longer being used.")
													  };

							// Setup the GUIStyle
							GUIStyle selectionGridStyle = new GUIStyle (GUI.skin.button); 
							selectionGridStyle.fixedWidth = 64+8+8;		// 76 originally
							selectionGridStyle.fixedHeight = 80+16;
							selectionGridStyle.imagePosition = ImagePosition.ImageAbove;
							selectionGridStyle.padding = new RectOffset(8, 8, 8, 8);
							selectionGridStyle.fontSize = 9;
							//selectionGridStyle.fontStyle = FontStyle.Bold;

							// Modify GUIStyle to use small icons to fit in a single row
							if( useSmallIcons ){
								selectionGridStyle.fixedWidth = 48+8+8;		// 76 originally
								selectionGridStyle.fixedHeight = 76+16;
								//selectionGridStyle.imagePosition = ImagePosition.ImageOnly;
								selectionGridStyle.fontSize = 8;
							}

							// Show Selection Grid to choose a tool
							int oldSelectedGridInt = selGridInt;	// Cache the selection before we change it
							//selGridInt = GUILayout.SelectionGrid (selGridInt, selContent, 4, selectionGridStyle, GUILayout.MinWidth(300), GUILayout.MaxWidth(300), GUILayout.MaxHeight(64) );	

							selGridInt = GUILayout.SelectionGrid (selGridInt, selContent, useSmallIcons ? 8 : 4, selectionGridStyle, GUILayout.MinWidth(300), GUILayout.MaxWidth(300), GUILayout.MaxHeight(64) );	

							// If we've changed the tab, remove all focus
							if( oldSelectedGridInt != selGridInt ){ GUI.FocusControl(""); }

							// Add Flexible Space
							GUILayout.FlexibleSpace();
						//	GUILayout.Space(4);

						// End Horizontal row
						GUILayout.EndHorizontal();
						
						// Show the selected operation
						GUILayout.Space(8);
						GUILayout.Label(selContent[selGridInt].text.Replace("\n","") + " Meshes", "boldLabel", GUILayout.ExpandWidth(false) );
						GUILayout.BeginVertical( GUILayout.MaxWidth( _editorWindowWidth - 44 ) );
						//EditorGUILayout.HelpBox("\n"+selContent[selGridInt].tooltip+"\n", MessageType.Info );
						DoHelpBox( selContent[selGridInt].tooltip );
						GUILayout.EndVertical();
						GUI.backgroundColor = Color.white;

						// Show Options For Each Tool
						DoOptions();

						// Add Space and another sepLine
						if( showBottomSeperator ){
							GUILayout.Space(8);
							HTGUI.SepLine();
						}

						// RUN SELECTED TOOL
						GUILayout.BeginHorizontal();

							// Setup GUI for displaying Red Button
							GUIStyle finalButton = new GUIStyle(GUI.skin.button);
							finalButton.padding = new RectOffset(6, 6, 6, 6);
							finalButton.imagePosition = ImagePosition.ImageLeft;
							finalButton.fontStyle = FontStyle.Bold;

							if( selGridInt <= 5 ){

								// Default Button
								if( //GUILayout.Button( " Default Options ", 
									 GUILayout.Button(  new GUIContent( " Default Options ", defaultIcon ),
									finalButton, GUILayout.MinWidth(160), GUILayout.MinHeight(40), GUILayout.MaxHeight(40) ) ){
									ResetOptions();
								}

							}

							// Add Flexible Space
							GUILayout.FlexibleSpace();

							finalButton.fontStyle = FontStyle.Bold;

							// Disable main button if needed
							if(disableMainButton){ GUI.enabled = false; }


							// Update the Auto LOD button text
							selContent[6].text = shouldCreateNewMeshKitAutoLOD ? "\nSetup LOD" : "\nRecreate LOD";

							if( GUILayout.Button( new GUIContent( " "+selContent[selGridInt].text.Replace("\n","") + " ", selContent[selGridInt].image), finalButton, GUILayout.MinWidth(160), GUILayout.MinHeight(40), GUILayout.MaxHeight(40) ) ){

								// Update Verbose Mode depending on the value saved in Editor Preferences
								MeshKitGUI.verbose = EditorPrefs.GetBool ("MeshKitVerboseMode", false);

								// ================
								// SEPERATE TOOL
								// ================

								if( selGridInt == 0 && EditorUtility.DisplayDialog("Seperate Meshes", "Are you sure you want to seperate all submeshes on \""+Selection.activeGameObject.name+"\" and its child objects?", "Yes", "No") ){

									runToolAtEnd = RunToolAtEnd.Seperator; 
								} 

								// ================
								// INVERT TOOL
								// ================

								if( selGridInt == 1 && EditorUtility.DisplayDialog("Invert Meshes", "Are you sure you want to invert all meshes on \""+Selection.activeGameObject.name+"\" and its child objects?", "Yes", "No") ){

									runToolAtEnd = RunToolAtEnd.Invert; 
								} 

								// ==================
								// DOUBLE-SIDED TOOL
								// ==================

								if( selGridInt == 2 && EditorUtility.DisplayDialog("Double Sided Meshes", "Are you sure you want to convert all meshes on \""+Selection.activeGameObject.name+"\" and its child objects to be double-sided?", "Yes", "No") ){

									runToolAtEnd = RunToolAtEnd.DoubleSided;
								} 

								// ================
								// REBUILD TOOL
								// ================

								if( selGridInt == 3 && EditorUtility.DisplayDialog("Rebuild Meshes", "Are you sure you want to rebuild all meshes on \""+Selection.activeGameObject.name+"\" and its child objects?", "Yes", "No") ){

									runToolAtEnd = RunToolAtEnd.Rebuild;
								} 

								// ================
								// COMBINE TOOL
								// ================

								else if( selGridInt == 4 && EditorUtility.DisplayDialog("Combine Meshes", "Are you sure you want to combine all similar meshes on \""+Selection.activeGameObject.name+"\" and its child objects?", "Yes", "No") ){

									runToolAtEnd = RunToolAtEnd.Combine;
								}

								// ================
								// DECIMATE TOOL
								// ================

								else if( selGridInt == 5 && EditorUtility.DisplayDialog("Decimate Meshes", "Are you sure you want to decimate all meshes on \""+Selection.activeGameObject.name+"\" and its child objects?", "Yes", "No") ){

									runToolAtEnd = RunToolAtEnd.Decimate;
								}

								// ================
								// AUTO-LOD TOOL
								// ================

								else if( selGridInt == 6 ){

									if( shouldCreateNewMeshKitAutoLOD == true &&
										EditorUtility.DisplayDialog("Setup LOD System", "Are you sure you want to setup MeshKit's Automatic LOD system on the GameObject \""+Selection.activeGameObject.name+"\"?", "Yes", "No") 
										||
										shouldCreateNewMeshKitAutoLOD == false &&
										EditorUtility.DisplayDialog("Recreate LOD System", "Are you sure you want to replace the LODs on \""+Selection.activeGameObject.name+"\"?", "Yes", "No") 
									){

										runToolAtEnd = RunToolAtEnd.AutoLOD;

									}
								}

								// ==============
								// CLEAN TOOL
								// ==============

								else if( selGridInt == 7 && EditorUtility.DisplayDialog("Clean Meshes", "Are you sure you want to clean all MeshKit generated meshes for this scene?\n\nNOTE: The current Unity scene will be saved first.", "Yes", "No") ){

									runToolAtEnd = RunToolAtEnd.Clean;
								}
							}

							// Re-enable the GUI
							GUI.enabled = false;

						GUILayout.EndHorizontal();

					// End White Box
					HTGUI.EndWhiteBox();

				}

				// End Big Layout
				HTGUI.EndBigLayout( 24 );

			// End ScrollView
			GUILayout.Space(24);
			EditorGUILayout.EndScrollView();


			// ======================================================================================
			// 	RUN ALL THE TOOLS AT THE END SO UNITY 2017 DOESN'T THROW ERRORS ABOUT THE GUI LAYOUT
			// ======================================================================================

			// Run Any Tools AFTER the UI is finished (so we don't get errors in Unity 2017.x)
			if( GUI.changed == true && runToolAtEnd != RunToolAtEnd.No ){

				// ================
				//	SEPERATOR TOOL
				// ================

				if( runToolAtEnd == RunToolAtEnd.Seperator ){

					// Debug
					if(MeshKitGUI.verbose){ Debug.LogWarning( "MESHKIT GUI - Running Tool: " + runToolAtEnd.ToString() ); }

					// Make sure we don't run this twice
					runToolAtEnd = RunToolAtEnd.No;

					// Break Prefab Instance
					BreakPrefabInstance();

					// Start
					updateProgressBars = true;
					BatchMeshSeperator.PrepareObjects( Selection.activeGameObject, optionThisGameObjectOnly );
					updateProgressBars = false;
				}

				// ================
				//	INVERT TOOL
				// ================

				else if( runToolAtEnd == RunToolAtEnd.Invert ){

					// User hasn't selected anything
					if( optionUseMeshFilters == false && optionUseSkinnedMeshRenderers == false ){
						EditorUtility.DisplayDialog("Mesh Kit", "You have not selected to process Mesh Filters or Skinned Mesh Renderers. No meshes will be processed.", "Okay");
						
					} else {

						// Debug
						if(MeshKitGUI.verbose){ Debug.LogWarning( "MESHKIT GUI - Running Tool: " + runToolAtEnd.ToString() ); }

						// Make sure we don't run this twice
						runToolAtEnd = RunToolAtEnd.No;

						// Always Break Prefab Connections if we are performing MeshKit Operations ...
						BreakPrefabInstance();

						// Start
						updateProgressBars = true;
						MeshRebuilder.BatchInvertMesh( Selection.activeGameObject, optionThisGameObjectOnly, optionUseMeshFilters, optionUseSkinnedMeshRenderers );
						updateProgressBars = false;
					}
				}

				// ===================
				//	DOUBLE-SIDED TOOL
				// ===================

				else if( runToolAtEnd == RunToolAtEnd.DoubleSided ){

					// User hasn't selected anything
					if( optionUseMeshFilters == false && optionUseSkinnedMeshRenderers == false ){
						EditorUtility.DisplayDialog("Mesh Kit", "You have not selected to process Mesh Filters or Skinned Mesh Renderers. No meshes will be processed.", "Okay");
						
					} else {

						// Debug
						if(MeshKitGUI.verbose){ Debug.LogWarning( "MESHKIT GUI - Running Tool: " + runToolAtEnd.ToString() ); }

						// Make sure we don't run this twice
						runToolAtEnd = RunToolAtEnd.No;

						// Always Break Prefab Connections if we are performing MeshKit Operations ...
						BreakPrefabInstance();

						// Start
						updateProgressBars = true;
						MeshRebuilder.BatchDoubleSidedMesh( Selection.activeGameObject, optionThisGameObjectOnly, optionUseMeshFilters, optionUseSkinnedMeshRenderers );
						updateProgressBars = false;

					}
				}

				// ===================
				//	REBUILD TOOL
				// ===================

				else if( runToolAtEnd == RunToolAtEnd.Rebuild ){

					// User hasn't selected anything
					if( optionUseMeshFilters == false && optionUseSkinnedMeshRenderers == false ){
						EditorUtility.DisplayDialog("Mesh Kit", "You have not selected to process Mesh Filters or Skinned Mesh Renderers. No meshes will be processed.", "Okay");
						
					} else {

						// Debug
						if(MeshKitGUI.verbose){ Debug.LogWarning( "MESHKIT GUI - Running Tool: " + runToolAtEnd.ToString() ); }

						// Make sure we don't run this twice
						runToolAtEnd = RunToolAtEnd.No;

						// Always Break Prefab Connections if we are performing MeshKit Operations ...
						BreakPrefabInstance();

						// Start
						updateProgressBars = true;
						MeshRebuilder.BatchRebuildMesh( Selection.activeGameObject, optionThisGameObjectOnly, optionUseMeshFilters, optionUseSkinnedMeshRenderers, optionStripNormals, optionStripTangents, optionStripColors, optionStripLightmapUVs, optionStripUV3, optionStripUV4, optionStripUV5, optionStripUV6, optionStripUV7, optionStripUV8, optionRebuildNormals, optionRebuildTangents, optionRebuildLightmapUVs, optionRebuildNormalsAngle );
						updateProgressBars = false;

					}
				}

				// ===================
				//	COMBINE TOOL
				// ===================

				else if( runToolAtEnd == RunToolAtEnd.Combine ){

					// Debug
					if(MeshKitGUI.verbose){ Debug.LogWarning( "MESHKIT GUI - Running Tool: " + runToolAtEnd.ToString() ); }

					// Make sure we don't run this twice
					runToolAtEnd = RunToolAtEnd.No;

					// Always Break Prefab Connections if we are performing MeshKit Operations ...
					BreakPrefabInstance();

					// Start // useExperimentalFeatures
					updateProgressBars = true;
					MeshCombiner.StartCombine( Selection.activeGameObject.transform, false, -1, "", true, false, false, false, false, optionMaxVertices, useExperimentalFeatures ? optionUseMeshFilters : true, useExperimentalFeatures ? optionUseSkinnedMeshRenderers : false, optionRestoreBindposes );
					updateProgressBars = false;
				}

				// ===================
				//	DECIMATE TOOL
				// ===================

				else if( runToolAtEnd == RunToolAtEnd.Decimate ){

					// User hasn't selected anything
					if( optionUseMeshFilters == false && optionUseSkinnedMeshRenderers == false ){
						EditorUtility.DisplayDialog("Mesh Kit", "You have not selected to process Mesh Filters or Skinned Mesh Renderers. No meshes will be processed.", "Okay");
						
					} else {

						// Debug
						if(MeshKitGUI.verbose){ Debug.LogWarning( "MESHKIT GUI - Running Tool: " + runToolAtEnd.ToString() ); }

						// Make sure we don't run this twice
						runToolAtEnd = RunToolAtEnd.No;

						// Always Break Prefab Connections if we are performing MeshKit Operations ...
						BreakPrefabInstance();

						// Start Mesh Decimation
						updateProgressBars = true;
						MeshDecimation.StartDecimation( Selection.activeGameObject, !optionThisGameObjectOnly, optionUseMeshFilters, optionUseSkinnedMeshRenderers, true, optionDecimatorQuality,
								optionDecimatorRecalculateNormals, optionPreserveBorders, optionPreserveSeams, optionPreserveFoldovers
							);
						updateProgressBars = false;

					}

				}

				// ===================
				//	AUTO LOD TOOL
				// ===================

				else if( runToolAtEnd == RunToolAtEnd.AutoLOD ){

					// Debug
					if(MeshKitGUI.verbose){ Debug.LogWarning( "MESHKIT GUI - Running Tool: " + runToolAtEnd.ToString() ); }

					// Make sure we don't run this twice
					runToolAtEnd = RunToolAtEnd.No;

					// Always Break Prefab Connections if we are performing MeshKit Operations ...
					BreakPrefabInstance();

					// Start Mesh Decimation
					updateProgressBars = true;
					
					// If we should create a new MeshKitAutoLOD, do it...
					if( shouldCreateNewMeshKitAutoLOD == true ){

						// Create the MeshKit LOD system ( and true = create default LODs right away )
						MeshLOD.CreateNewMeshKitAutoLOD( Selection.activeGameObject, true );

						// Update the selection
						OnSelectionChange();

					// Otherwise, edit the existing setup ...	
					} else if ( selectedAutoLOD != null ){

						MeshLOD.StartGenerateLOD( selectedAutoLOD );

					}
						
					updateProgressBars = false;

				}

				// ===================
				//	CLEAN TOOL
				// ===================

				else if( runToolAtEnd == RunToolAtEnd.Clean ){

					// Debug
					if(MeshKitGUI.verbose){ Debug.LogWarning( "MESHKIT GUI - Running Tool: " + runToolAtEnd.ToString() ); }

					// Make sure we don't run this twice
					runToolAtEnd = RunToolAtEnd.No;

					// Check if the scene is saved first ...
					if( HTScene.SaveScene() ){
						updateProgressBars = true;
						MeshAssets.AreMeshAssetsBeingUsedInTheScene();
						updateProgressBars = false;
					} else {
						Debug.Log("MESHKIT: Could not perform clean because the scene was not saved.");
					}
				}
				
			}
		}

		 ////////////////////////////////////////////////////////////////////////////////////////////////
		//	RESET OPTIONS
		//	Resets the options back to the defaults
		////////////////////////////////////////////////////////////////////////////////////////////////

		void ResetOptions(){

			// Reset Options
			optionThisGameObjectOnly = false;
			optionUseMeshFilters = true;
			optionUseSkinnedMeshRenderers = false;
			optionStripNormals = false;
			optionStripTangents = false;
			optionRebuildNormals = false;
			optionRebuildNormalsAngle = 60.0f;
			optionRebuildTangents = false;
			optionStripColors = false;
			optionRebuildLightmapUVs = false;
			optionStripLightmapUVs = false;
			optionStripUV3 = false;
			optionStripUV4 = false;
			optionStripUV5 = false;
			optionStripUV6 = false;
			optionStripUV7 = false;
			optionStripUV8 = false;
			optionMaxVertices = MeshCombiner.maxVertices;
			optionRestoreBindposes = false;
			optionDecimatorQuality = 0.8f;
			optionDecimatorRecalculateNormals = false;
			optionPreserveBorders = false;
			optionPreserveSeams = false;
			optionPreserveFoldovers = false;
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		//	DO HELP BOX
		//	Quick way to make a help box with the correct color tints
		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		
		void DoHelpBox( string text, MessageType messageType = MessageType.Info, bool wide = true, bool addTopSpace = false, bool addBottomSpace = false ){
			
			// Add space
			if(addTopSpace){ EditorGUILayout.Space(); }

			// If this is the light skin, give the help box a yellow background
			if( EditorGUIUtility.isProSkin == false ){ GUI.backgroundColor = new Color(1f,1f,0f,0.333f); }

			// Show Help Box
			EditorGUILayout.HelpBox( "\n" + text + "\n", messageType, wide );

			// Restore Background Color
			if( EditorGUIUtility.isProSkin == false ){  GUI.backgroundColor = Color.white; }

			// Add space
			if(addBottomSpace){ EditorGUILayout.Space(); }
		}

		////////////////////////////////////////////////////////////////////////////////////////////////
		//	DO OPTIONS
		//	Shows the options for each tool
		////////////////////////////////////////////////////////////////////////////////////////////////

		void DoOptions(){

			// turn off disable button by default
			disableMainButton = false;
			showBottomSeperator = true;

			// Add Options for everything except "Clean Scene"
			if( selGridInt == 0 || selGridInt == 1 || selGridInt == 2 || selGridInt == 3 || selGridInt == 4 || selGridInt == 5 || selGridInt == 6 ){

				// Show Label
				if( selGridInt != 6 ){
					GUILayout.Label("Selection Options", "boldLabel");
					GUILayout.Space(8);
				}

				// Only Apply to this GameObject Only (for Seperate, Invert, Double Sided and Rebuild )
				if( selGridInt == 0 || selGridInt == 1 || selGridInt == 2 || selGridInt == 3 || selGridInt == 4 || selGridInt == 5 ){
					optionThisGameObjectOnly = HTGUI.ToggleField(goIcon, "Do Not Select Child Objects", optionThisGameObjectOnly, 220,220,24 );

					// Don't allow users to choose between mesh filters and skinned mesh renderers with Seperate and Combine
					if( selGridInt != 0 && selGridInt != 4 ||
						selGridInt == 4 && useExperimentalFeatures == true // <- EXPERIMENTAL COMBINE FEATURES!
					){

						optionUseMeshFilters = HTGUI.ToggleField( /*meshFilterIcon*/ goIcon, "Select Mesh Filters", optionUseMeshFilters, 220,220,24 );
						optionUseSkinnedMeshRenderers = HTGUI.ToggleField( /*skinnedMeshRendererIcon*/ goIcon, "Select Skinned Mesh Renderers", 	optionUseSkinnedMeshRenderers, 220,220,24 );
					}

				}

				// Rebuild Options
				if( selGridInt == 3 ){

					// Show Label
					GUILayout.Space(8);
					GUILayout.Label("Strip Options", "boldLabel");
					GUILayout.Space(8);

					// Strip Normals
					optionStripNormals = HTGUI.ToggleField(goIcon, "Remove Normals", optionStripNormals, 220,220,24 );
					if( optionStripNormals ){
						GUI.enabled = false;
						optionStripTangents = true;
					}	
					
					// Strip Tangents
					optionStripTangents = HTGUI.ToggleField(goIcon, "Remove Tangents", optionStripTangents, 220,220,24 );
					GUI.enabled = true;

					// Strip Colors
					optionStripColors = HTGUI.ToggleField(goIcon, "Remove Colors", optionStripColors, 220,220,24 );

					// Strip UV2 (Lightmap)
					optionStripLightmapUVs = HTGUI.ToggleField(goIcon, "Remove UV Set 2 (Lightmap)", optionStripLightmapUVs, 220,220,24 );

			// These features work on Unity 5 and up ...				
			#if !UNITY_3_5 && !UNITY_4_0_1 && !UNITY_4_1 && !UNITY_4_2 && !UNITY_4_3 && !UNITY_4_3 && !UNITY_4_4 && !UNITY_4_5 && !UNITY_4_6 && !UNITY_4_7	
				
					optionStripUV3 = HTGUI.ToggleField(goIcon, "Remove UV Set 3", optionStripUV3, 220,220,24 );
					optionStripUV4 = HTGUI.ToggleField(goIcon, "Remove UV Set 4", optionStripUV4, 220,220,24 );
			
			#endif	

			// These features work in Unity 2018.2 and up
			#if UNITY_2018_2_OR_NEWER

					optionStripUV5 = HTGUI.ToggleField(goIcon, "Remove UV Set 5", optionStripUV5, 220,220,24 );
					optionStripUV6 = HTGUI.ToggleField(goIcon, "Remove UV Set 6", optionStripUV6, 220,220,24 );
					optionStripUV7 = HTGUI.ToggleField(goIcon, "Remove UV Set 7", optionStripUV7, 220,220,24 );
					optionStripUV8 = HTGUI.ToggleField(goIcon, "Remove UV Set 8", optionStripUV8, 220,220,24 );

			#endif	


					// Show Label
					GUILayout.Space(8);
					GUILayout.Label("Rebuild Options", "boldLabel");
					GUILayout.Space(8);

					// Rebuild Normals
					optionRebuildNormals = HTGUI.ToggleField(goIcon, "Rebuild Normals", optionRebuildNormals, 220,220,24 );

					// If we're rebuilding normals, show the new Angle Threshold slider ( -1 -> 179 ), < -1 = fast normals
					if( optionRebuildNormals ){
						optionRebuildNormalsAngle = HTGUI.SliderField(null, "  Angle Threshold", optionRebuildNormalsAngle, -1, 179, 220,220,24 );

						GUILayout.BeginHorizontal();
							GUILayout.Label("", GUILayout.MaxWidth(20), GUILayout.MaxHeight(20) );
							//GUILayout.Label("Test", GUILayout.MinWidth(160), GUILayout.MaxWidth(160), GUILayout.MaxHeight(20));
							//EditorGUILayout.HelpBox("NOTE: Use -1 for default Unity calculation or 45 for default smoothing.", MessageType.Info );
							DoHelpBox( "NOTE: Use -1 for default Unity calculation or 45 for default smoothing." );
						GUILayout.EndHorizontal();

						// Spacer
						GUILayout.Label("", GUILayout.MinHeight(16), GUILayout.MaxHeight(16) );
					}

					// If we're not rebuilding normals, we can't rebuild tangents
					if(!optionRebuildNormals){
						optionRebuildTangents = false;
						GUI.enabled = false;
					}

					// Rebuild Tangents
					optionRebuildTangents = HTGUI.ToggleField(goIcon, "Rebuild Tangents", optionRebuildTangents, 220,220,24 );

					// Reset GUI Enabled
					GUI.enabled = true;

					// Rebuild Lightmap UVs
					optionRebuildLightmapUVs = HTGUI.ToggleField(goIcon, "Recreate Lightmap UVs", optionRebuildLightmapUVs, 220,220,24 );
				}

				// Combine Options
				else if( selGridInt == 4 ){

					// Show Label
					GUILayout.Space(8);
					GUILayout.Label("Combine Options", "boldLabel");
					GUILayout.Space(8);

					optionMaxVertices = HTGUI.IntSliderField(goIcon, "Maximum Vertices Per Object", optionMaxVertices, 16000, MeshCombiner.maxVertices, 220,220,24 );

					if( optionUseSkinnedMeshRenderers && useExperimentalFeatures ){

						// Restore Bindposes
						optionRestoreBindposes = HTGUI.ToggleField(goIcon, "Attempt To Fix Bindposes", optionRestoreBindposes, 220,220,24 );

						// If we enabled restore bindposes, show the user a warning
						if( optionRestoreBindposes ){

							GUILayout.BeginHorizontal();
							//	GUILayout.Label("", GUILayout.MaxWidth(20), GUILayout.MaxHeight(20) );
								//GUILayout.Label("Test", GUILayout.MinWidth(160), GUILayout.MaxWidth(160), GUILayout.MaxHeight(20));
								//EditorGUILayout.HelpBox("Attempting to fix bindposes should normally be disabled. You should only try this option on skinned meshes with problematic rigs.", MessageType.Info );
								DoHelpBox( "Attempting to fix bindposes should normally be disabled. You should only try this option on skinned meshes with problematic rigs." );
							GUILayout.EndHorizontal();

							// Spacer
							GUILayout.Label("", GUILayout.MinHeight(16), GUILayout.MaxHeight(16) );
						}
					}
				}

				// Decimator Options
				else if( selGridInt == 5 ){

					// Show Label
					GUILayout.Space(8);
					GUILayout.Label("Decimate Options", "boldLabel");
					GUILayout.Space(8);

					// Check boxes
					optionDecimatorRecalculateNormals = HTGUI.ToggleField(goIcon, "Recalculate Normals", optionDecimatorRecalculateNormals, 220,220,24 ); 
					optionPreserveBorders = HTGUI.ToggleField(goIcon, "Preserve Borders", optionPreserveBorders, 220,220,24 );
					optionPreserveSeams = HTGUI.ToggleField(goIcon, "Preserve Seams", optionPreserveSeams, 220,220,24 );
					optionPreserveFoldovers = HTGUI.ToggleField(goIcon, "Preserve UV Foldovers", optionPreserveFoldovers, 220,220,24 );

					// Quality Settings
					optionDecimatorQuality = HTGUI.SliderField( goIcon, "Mesh Decimator Quality", optionDecimatorQuality, 0f, 1f, 220,220,24 );
				}

				// LOD Options
				else if( selGridInt == 6 ){

					// This is complicated so is seperated out
					DoAutoLODOptions();

				}
			}
		}

		////////////////////////////////////////////////////////////////////////////////////////////////
		//	DO AUTO LOD OPTIONS
		//	Handles checks and shows options for Auto-LOD
		////////////////////////////////////////////////////////////////////////////////////////////////

		void DoAutoLODOptions(){

			// No MeshKitAutoLODs or LODGroup components in children or parents
			if( LODSystemExistsInParent == false && howManyMeshKitAutoLODComponentsInChildren == 0 && howManyLODGroupComponentsInChildren == 0 && ( meshfilters > 0 || skinnedMeshRenderers > 0 )
			){

				// We can't mix BOTH meshfilters and skinned mesh renderers into the same LOD
				if( meshfilters > 0 && skinnedMeshRenderers > 0 ){

					HTGUI.SectionTitle( warningIcon, "You cannot setup Automatic LOD on this GameObject because it contains both Mesh Filters and Skinned Mesh Renderers.", "", 32);

					// Dont allow user to trigger main button
					disableMainButton = true;
					shouldCreateNewMeshKitAutoLOD = false;
					
				// We can't mix multiple skinnedMeshRenderers into an LOD, we need to drill down into a single SkinnedMeshRenderer
				} else if( meshfilters == 0 && skinnedMeshRenderers > 1 ){

					HTGUI.SectionTitle( warningIcon, "You cannot setup Automatic LOD on this GameObject because it contains multiple Skinned Mesh Renderers. To create LODs on a Skinned Mesh Renderer, you should select it directly.", "", 32);

					// Dont allow user to trigger main button
					disableMainButton = true;
					shouldCreateNewMeshKitAutoLOD = false;

				// There is only 1 SkinnedMeshRenderer but its not on the selected GameObject
				} else if ( meshfilters == 0 && skinnedMeshRenderers == 1 && 
							Selection.activeGameObject.GetComponent<SkinnedMeshRenderer>() == null 
				){

					HTGUI.SectionTitle( warningIcon, "You cannot setup Automatic LOD on this GameObject because a child object contains a Skinned Mesh Renderer. To create LODs on a Skinned Mesh Renderer, you should select it directly.", "", 32);

					// Dont allow user to trigger main button
					disableMainButton = true;
					shouldCreateNewMeshKitAutoLOD = false;


				} else {

					// Show information under the options section ...
					HTGUI.WrappedTextLabel("Press the \"Setup LOD\" button to create automatic LODs on this GameObject.\n", true );

					// We should be creating a new MeshKit LOD component ...
					shouldCreateNewMeshKitAutoLOD = true;

				}

			// There are NO meshfilters or Skinned Mesh Renderers
			} else if( meshfilters == 0 && skinnedMeshRenderers == 0 ){

				HTGUI.SectionTitle( warningIcon, "You cannot setup Automatic LOD on this GameObject because it does not contain any Mesh Filters or Skinned Mesh Renderers.", "", 32);

				// Dont allow user to trigger main button
				disableMainButton = true;
				shouldCreateNewMeshKitAutoLOD = false;

			// MeshKit LOD setups found in parent objects
			} else if ( LODSystemExistsInParent == true ){

				HTGUI.SectionTitle( warningIcon, "You cannot setup Automatic LOD on this GameObject because LOD setups were found in a parent object.", "", 32);

				// Dont allow user to trigger main button
				disableMainButton = true;
				shouldCreateNewMeshKitAutoLOD = false;

			// MeshKit AutoLOD Setups found in children
			} else if( howManyMeshKitAutoLODComponentsInChildren >= 1 && selectedAutoLOD == null ){

				HTGUI.SectionTitle( warningIcon, "You cannot setup Automatic LOD on this GameObject because there are MeshKit LOD setups found in its children.", "", 32);

				// Dont allow user to trigger main button
				disableMainButton = true;
				shouldCreateNewMeshKitAutoLOD = false;


			// There are LODGroups already in the children, so its best not to mess them up
			} else if ( howManyLODGroupComponentsInChildren > howManyMeshKitAutoLODComponentsInChildren ){

				HTGUI.SectionTitle( warningIcon, "There appears to be existing LODGroup components within this GameObject. This setup should be removed manually before using MeshKit.", "", 32);

				// Dont allow user to trigger main button
				disableMainButton = true;
				shouldCreateNewMeshKitAutoLOD = false;


			// This gameobject is valid, show the options!
			} else if( howManyMeshKitAutoLODComponentsInChildren == 1 && selectedAutoLOD != null && LODSystemExistsInParent == false ){

				// Make sure we're not using the create new MeshKitAutoLOD flag
				shouldCreateNewMeshKitAutoLOD = false;

				// If the levels are null, we should Reset it ( which recreates the default levels )
				if( selectedAutoLOD.levels == null ){ selectedAutoLOD.Reset(); }
				else {

					// Show Label
					GUILayout.Label("Tool Options", "boldLabel");
					GUILayout.Space(8);

					// Use Advanced Mode
					selectedAutoLOD.advancedMode = HTGUI_UNDO.ToggleField( selectedAutoLOD, "Use Advanced Settings", gearIcon, "Use Advanced Settings", selectedAutoLOD.advancedMode ); 

					// Show Advanced Mode Settings
					if( selectedAutoLOD.advancedMode == true ){

						// Add Space
						GUILayout.Space(8);

						GUILayout.Label("Decimation Options", "BoldLabel", GUILayout.MaxHeight(24));

						// Preserve Borders
						selectedAutoLOD.preserveBorders = HTGUI_UNDO.ToggleField( selectedAutoLOD, "Preserve Borders", gearIcon, "Preserve Borders", selectedAutoLOD.preserveBorders ); 

						// Preserve Seams
						selectedAutoLOD.preserveSeams = HTGUI_UNDO.ToggleField( selectedAutoLOD, "Preserve Seams", gearIcon, "Preserve Seams", selectedAutoLOD.preserveSeams ); 

						// Preserve UV Foldovers
						selectedAutoLOD.preserveFoldovers = HTGUI_UNDO.ToggleField( selectedAutoLOD, "Preserve UV Foldovers", gearIcon, "Preserve UV Foldovers", selectedAutoLOD.preserveFoldovers ); 


						// Add Space
						GUILayout.Space(8);

						//EditorGUILayout.HelpBox("\n" + "NOTE: Configure this LOD Group by applying different settings to the primary mesh and its associated renderer. The LOD boxes at the top of the list are closest to the camera." + "\n", MessageType.Info );
						DoHelpBox( "NOTE: Configure this LOD Group by applying different settings to the primary mesh and its associated renderer. The LOD boxes at the top of the list are closest to the camera." );

						// Add Space
						GUILayout.Space(8);

						
						// Loop through the LOD Levels
						for( int i = 0; i < selectedAutoLOD.levels.Length; i++ ){

							// Start LOD Box
							HTGUI.StartWhiteBox();

								// =====================
								//	DO LOD BOX HEADER
								// =====================

								// Event Counter And Label 
								EditorGUILayout.BeginHorizontal();

									// Setup a larger bold font to use
									GUIStyle boldEventTitleGUIStyle = new GUIStyle("BoldLabel");
									boldEventTitleGUIStyle.fontSize = 12;
									boldEventTitleGUIStyle.richText = true;

									// Draw the icon and label using a GUIContent
									GUILayout.Label( new GUIContent(" Auto LOD - Level " + (i+1).ToString(), goIcon ), boldEventTitleGUIStyle, GUILayout.MaxHeight(24));

									// Space
									GUILayout.Label("", GUILayout.MaxWidth(5), GUILayout.MaxHeight(5) );

									// Only allow the prefab items to be moved when the game isn't running
									if( Application.isPlaying == false ){

										// Move LOD Up
										/*
										if( selectedAutoLOD.levels.Length > 0 && i != 0 &&
											GUILayout.Button( new GUIContent( System.String.Empty, upButton, "Move LOD Item Up"), GUILayout.MinWidth(24), GUILayout.MaxWidth(24) ) 
										){
											Undo.RecordObject ( selectedAutoLOD, "Move LOD Item Up" );
											Arrays.Shift( ref selectedAutoLOD.levels, i, true );
											GUIUtility.ExitGUI();
										}

										// Move LOD Down
										if( selectedAutoLOD.levels.Length > 0 && i !=  selectedAutoLOD.levels.Length-1 &&
											GUILayout.Button( new GUIContent( System.String.Empty, downButton, "Move LOD Item Down"), GUILayout.MinWidth(24), GUILayout.MaxWidth(24) ) 
										){
											Undo.RecordObject ( selectedAutoLOD, "Move LOD Item Down" );
											Arrays.Shift( ref selectedAutoLOD.levels, i, false );
											GUIUtility.ExitGUI();
										}
										*/

										// Destroy LOD Item (we must ensure at least 1 LOD item exists)
										if( selectedAutoLOD.levels.Length > 1 &&
											GUILayout.Button( new GUIContent( System.String.Empty, HTGUI.removeButton, "Remove LOD Item" ), GUILayout.MinWidth(24), GUILayout.MaxWidth(24) ) 
										){
											Undo.RecordObject ( selectedAutoLOD, "Remove LOD Item" );
											Arrays.RemoveItemAtIndex( ref selectedAutoLOD.levels, i );
											GUIUtility.ExitGUI();
										}

									}
									
								EditorGUILayout.EndHorizontal();

								// Seperator Line
								GUILayout.Space(8);
								HTGUI.SepLine();

								// =====================
								//	DO LOD BOX SETTINGS
								// =====================

								GUILayout.Label("LOD Distance", "BoldLabel", GUILayout.MaxHeight(24));

								// LOD Distance Slider
								selectedAutoLOD.levels[i].lodDistancePercentage = HTGUI_UNDO.SliderField( selectedAutoLOD, "Distance Percentage", goIcon, "Distance Percentage", selectedAutoLOD.levels[i].lodDistancePercentage, 0.01f, 100f ); 

								// Make sure the LOD Distance is never greater than the previous LOD
								if( i > 0 && selectedAutoLOD.levels[i].lodDistancePercentage > selectedAutoLOD.levels[i-1].lodDistancePercentage){
									selectedAutoLOD.levels[i].lodDistancePercentage = selectedAutoLOD.levels[i-1].lodDistancePercentage - 0.01f;
									if( selectedAutoLOD.levels[i].lodDistancePercentage < 0f ){ selectedAutoLOD.levels[i].lodDistancePercentage = 0f; }
									Repaint();
								}

								GUILayout.Label("Decimation", "BoldLabel", GUILayout.MaxHeight(24));

								// Quality Slider
								selectedAutoLOD.levels[i].quality = HTGUI_UNDO.SliderField( selectedAutoLOD, "Mesh Quality", goIcon, "Mesh Quality", selectedAutoLOD.levels[i].quality, 0.01f, 1f ); 

								//GUILayout.Label("Meshes", "BoldLabel", GUILayout.MaxHeight(24));

								// Combine Meshes
								selectedAutoLOD.levels[i].combineMeshes = false; // always use false for combine meshes!
								//selectedAutoLOD.levels[i].combineMeshes = HTGUI_UNDO.ToggleField( selectedAutoLOD, "Combine Meshes", goIcon, "Combine Meshes", selectedAutoLOD.levels[i].combineMeshes ); 

								GUILayout.Label("Renderers", "BoldLabel", GUILayout.MaxHeight(24));

								// Skin Quality
								selectedAutoLOD.levels[i].skinQuality = (SkinQuality)HTGUI_UNDO.EnumField( selectedAutoLOD, "Skin Quality", goIcon, "Skin Quality", selectedAutoLOD.levels[i].skinQuality ); 
								
								// Receive Shadows
								selectedAutoLOD.levels[i].receiveShadows = HTGUI_UNDO.ToggleField( selectedAutoLOD, "Receive Shadows", goIcon, "Receive Shadows", selectedAutoLOD.levels[i].receiveShadows ); 

								// Shadow Casting Mode
								selectedAutoLOD.levels[i].shadowCasting = (ShadowCastingMode)HTGUI_UNDO.EnumField( selectedAutoLOD, "Shadow Casting Mode", goIcon, "Shadow Casting Mode", selectedAutoLOD.levels[i].shadowCasting );

								// Motion Vector Generation Mode
								selectedAutoLOD.levels[i].motionVectors = (MotionVectorGenerationMode)HTGUI_UNDO.EnumField( selectedAutoLOD, "Motion Vector Generation", goIcon, "Motion Vector Generation", selectedAutoLOD.levels[i].motionVectors );

								// Skinned Motion Vectors
								selectedAutoLOD.levels[i].skinnedMotionVectors = HTGUI_UNDO.ToggleField( selectedAutoLOD, "Skinned Motion Vectors", goIcon, "Skinned Motion Vectors", selectedAutoLOD.levels[i].skinnedMotionVectors ); 

								// Light Probe Usage
								selectedAutoLOD.levels[i].lightProbeUsage = (LightProbeUsage)HTGUI_UNDO.EnumField( selectedAutoLOD, "Light Probe Usage", goIcon, "Light Probe Usage", selectedAutoLOD.levels[i].lightProbeUsage );

								// Reflection Probe Usage
								selectedAutoLOD.levels[i].reflectionProbeUsage = (ReflectionProbeUsage)HTGUI_UNDO.EnumField( selectedAutoLOD, "Reflection Probe Usage", goIcon, "Reflection Probe Usage", selectedAutoLOD.levels[i].reflectionProbeUsage );


							// End LOD Box
							HTGUI.EndWhiteBox();
						}

						// ======================
						//	ADD / REMOVE BUTTONS
						// ======================

						// Add / Remove LOD Items (Create Horizontal Row)
						if( !Application.isPlaying ){

							// Start Horizontal Row
							EditorGUILayout.BeginHorizontal();

								// Flexible Space
								GUILayout.FlexibleSpace();	
													
								// Remove Button (there must be at least 1 LOD)			
								if( selectedAutoLOD.levels.Length <= 1 ){ GUI.enabled = false; }			
								if( GUILayout.Button( new GUIContent( "", HTGUI.removeButton, "Remove Last LOD Item"), GUILayout.MaxWidth(32)) ) { 
									if( selectedAutoLOD.levels.Length > 0 ){	// <- We must always have at least 1 condition!
										Undo.RecordObject ( selectedAutoLOD, "Remove Last LOD Item");
										System.Array.Resize(ref selectedAutoLOD.levels, selectedAutoLOD.levels.Length - 1 );
										GUIUtility.ExitGUI();
									}
								}

								// Reset GUI Enabled
								GUI.enabled = true;
												
								// Add Button							
								if( GUILayout.Button( new GUIContent( "", HTGUI.addButton, "Add New LOD Item"), GUILayout.MaxWidth(32))) { 
									Undo.RecordObject ( selectedAutoLOD, "Add New LOD Item");
									System.Array.Resize(ref selectedAutoLOD.levels, selectedAutoLOD.levels.Length + 1 ); 
									selectedAutoLOD.levels[ selectedAutoLOD.levels.Length - 1 ] = new HellTap.MeshDecimator.Unity.LODSettings(0.5f);	// <- We need to set new LOD Item
									GUIUtility.ExitGUI();
								}

							// End Horizontal Row
							EditorGUILayout.EndHorizontal();

						}

						// ======================
						//	CULLING VALUE
						// ======================

						// Do Culling Distance
						GUILayout.Label("LOD Culling Distance", "BoldLabel", GUILayout.MaxHeight(24));

						// LOD Distance Slider
						selectedAutoLOD.cullingDistance = HTGUI_UNDO.SliderField( selectedAutoLOD, "Culling Distance", goIcon, "Culling Distance", selectedAutoLOD.cullingDistance, 0f, 100f ); 

						// Make sure the Culling LOD Distance is never greater than the last LOD
						if( selectedAutoLOD.levels.Length >= 1 &&
							selectedAutoLOD.cullingDistance >= selectedAutoLOD.levels[selectedAutoLOD.levels.Length-1].lodDistancePercentage 
						){
							selectedAutoLOD.cullingDistance = selectedAutoLOD.levels[selectedAutoLOD.levels.Length-1].lodDistancePercentage - 0.01f;
							if( selectedAutoLOD.cullingDistance < 0f ){ selectedAutoLOD.cullingDistance = 0f; }
							Repaint();
						}

						// =============================
						//	UPDATE LODGROUP PERCENTAGES
						// =============================

						// Update LODGroup Percentages
						LODGroup lodGroup = selectedAutoLOD.gameObject.GetComponent<LODGroup>();
						if (lodGroup != null ) {
						 
						 	// Cache the LODGroup so we can access it
							SerializedObject obj = new SerializedObject(lodGroup);						 
							SerializedProperty valArrProp = obj.FindProperty("m_LODs.Array");

							//Debug.Log("valArrProp.arraySize: " + valArrProp.arraySize);
							//Debug.Log("selectedAutoLOD.levels.Length: " + selectedAutoLOD.levels.Length);

							// If the LODGroup syncs correctly with the LODGroup component, use live updates on percentages.
							if( valArrProp.arraySize == selectedAutoLOD.levels.Length + 1 ){
								for (int i = 0; valArrProp.arraySize > i; i++) {

									// Cache the LOD Percentage (sHeight)
									SerializedProperty sHeight = obj.FindProperty("m_LODs.Array.data[" + i.ToString() + "].screenRelativeHeight");
							 
							 		// Update all LOD Percentages ( except for the last one, which is the cull value)
									if (i < valArrProp.arraySize-1 ) {
										if( sHeight.doubleValue != selectedAutoLOD.levels[i].lodDistancePercentage * 0.01f ){
											sHeight.doubleValue = selectedAutoLOD.levels[i].lodDistancePercentage * 0.01f;	
										}
									}

									// We should add the cull value here.
									if (i == valArrProp.arraySize-1 ) {
										if( sHeight.doubleValue != selectedAutoLOD.cullingDistance * 0.01f ){
											sHeight.doubleValue = selectedAutoLOD.cullingDistance * 0.01f;	
										}
									}

								}
								obj.ApplyModifiedProperties();
							}
						}
 

					}
				}

				// =====================
				//	FINAL OPTIONS
				// =====================

				//GUILayout.Space(8);
				//GUILayout.Label( "Auto LOD Actions", "BoldLabel");

				// Don't show the bottom seperator
				showBottomSeperator = false;
				GUILayout.Space(8);
				HTGUI.SepLine();

				// Setup GUI for displaying Red Button
				GUIStyle finalButton = new GUIStyle(GUI.skin.button);
				finalButton.padding = new RectOffset(6, 6, 6, 6);
				finalButton.imagePosition = ImagePosition.ImageLeft;
				finalButton.fontStyle = FontStyle.Bold;

				// RUN SELECTED TOOL
				GUILayout.BeginHorizontal();

					// Reset LOD
					if( GUILayout.Button( new GUIContent( " Reset LOD Setup ", defaultIcon ), finalButton, GUILayout.MinWidth(160), GUILayout.MinHeight(40), GUILayout.MaxHeight(40) ) &&
						EditorUtility.DisplayDialog("Reset LOD System", "Are you sure you want to reset the MeshKitAutoLOD Component on \""+Selection.activeGameObject.name+"\"?", "Yes", "No") 
					){

						Undo.RecordObject ( selectedAutoLOD, "Reset AutoLOD Component");
						selectedAutoLOD.Reset();

						// Update the selection
						//OnSelectionChange();
						GUIUtility.ExitGUI();
					}
				
					// Add Flexible Space
					GUILayout.FlexibleSpace();

					// Completely Remove Auto-LOD
					if( GUILayout.Button( new GUIContent( " Remove LOD ", deleteIcon ), finalButton, GUILayout.MinWidth(160), GUILayout.MinHeight(40), GUILayout.MaxHeight(40) ) &&
						EditorUtility.DisplayDialog("Remove LOD System", "Are you sure you want to remove the Auto LOD System on \""+Selection.activeGameObject.name+"\"?", "Yes", "No") 
					){

						MeshLOD.CompletelyRemoveLODSystemAndComponents( selectedAutoLOD, "Remove MeshKit AutoLOD System" );

						// Update the selection
						OnSelectionChange();
						GUIUtility.ExitGUI();
					}

				GUILayout.EndHorizontal();	

				// Final Space
				GUILayout.Space(8);

			}
		}


		////////////////////////////////////////////////////////////////////////////////////////////////
		//	BREAK PREFAB INSTANCE
		//	Makes sure any object we are applying an operation to is not connected to a prefab.
		////////////////////////////////////////////////////////////////////////////////////////////////

		void BreakPrefabInstance(){

			// New approach for Unity's new prefab system
			#if UNITY_2018_3_OR_NEWER

				/*
					NOTE:	If we don't disconect the prefab, MeshKit will immediately tell the user to move
							the mesh assets of the prefab to the "MeshKit Prefab Meshes" folder. As this may
							happen after every stage of an operation, This is likely to cause lots of ultimately
							unneeded meshes to be saved along with it, which is innefficient. 

							Because users cannot re-connect prefabs in 2018.3, we should instead recommend they unpack
							the prefab first, but ultimately allow them to work directly on the prefab inefficiently
							if they choose ( this is because nested prefabs cannot easily be re-connected ).
				*/

				// Get the nearest prefab root
				GameObject prefabRoot = PrefabUtility.GetNearestPrefabInstanceRoot( Selection.activeGameObject );

				// If a prefab root was found, break Prefab connections if the user tells us to.
				if( prefabRoot != null ){

					// Ask the user for permission
					if( EditorUtility.DisplayDialog("Unpack Prefab?", "MeshKit operations are faster and more efficient when they are applied to local scene objects rather than directly on to Prefabs.\n\nBefore proceeding, would you like to convert this prefab into a regular GameObject first? This is the recommended workflow as you can always convert it back into a prefab when you're finished.", "Yes", "No") 	
					){
						// Create a complete object undo so the user can go back to the original prefab
						Undo.RegisterCompleteObjectUndo( prefabRoot, "Unpack Prefab");

						// Completely unpack the prefab
						PrefabUtility.UnpackPrefabInstance( prefabRoot, PrefabUnpackMode.Completely,  InteractionMode.AutomatedAction );
					}
				}


			// Original Method
			#else

				// Find both the prefab root and the root gameobject with the same prefab as a parent - we'll break connections to both.
				GameObject prefabRoot = PrefabUtility.FindPrefabRoot( Selection.activeGameObject );
				GameObject gameObjectWithSameParentPrefab = PrefabUtility.FindRootGameObjectWithSameParentPrefab( Selection.activeGameObject );

				// Break Prefab Connections
				if( prefabRoot != null ){
					PrefabUtility.DisconnectPrefabInstance( prefabRoot );
				}
				if( gameObjectWithSameParentPrefab != null ){
					PrefabUtility.DisconnectPrefabInstance( gameObjectWithSameParentPrefab );
				}
			
			#endif
		}

		////////////////////////////////////////////////////////////////////////////////////////////////
		//	ON INSPECTOR UPDATE
		//	To Update Progress Bar
		////////////////////////////////////////////////////////////////////////////////////////////////

		void OnInspectorUpdate() { if(updateProgressBars){ Repaint(); } }	

		////////////////////////////////////////////////////////////////////////////////////////////////
		//	DEBUG LIST
		////////////////////////////////////////////////////////////////////////////////////////////////

		// DebugList
		public static Vector2 scrollPos = Vector2.zero;
		void DebugList(){
			
			if( BatchMeshSeperator.maxProgress == 0){
				EditorGUILayout.Space();
				HTGUI.SepLine();
				scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.MinHeight (200));
					if(BatchMeshSeperator.list != null && BatchMeshSeperator.list.Count > 0){
						foreach( BatchMeshes bm in BatchMeshSeperator.list ){
							if(bm!=null && bm.key!=null){
								string matNames = string.Empty;
								if( bm.key != null && bm.key.Length > 0 ){
									foreach( Material m in bm.key ){
										matNames += m.name + " | ";
									}
								}
								if(bm.originalMesh!=null){ GUILayout.Label(bm.originalMesh.name, "boldLabel"); }
								if(bm.gos!=null){ GUILayout.Label( "Total GameObjects using setup: " + bm.gos.Count );}
								if(bm.splitMeshes!=null){ GUILayout.Label( "Submeshes Ready: " + bm.splitMeshes.Length );}
								GUILayout.Label( "Materials:  | " + matNames );

								// Space and Seperation Line
								EditorGUILayout.Space();
								HTGUI.SepLine();
							}
						}
					}
				EditorGUILayout.EndScrollView();
			}
				
		}
	}
}