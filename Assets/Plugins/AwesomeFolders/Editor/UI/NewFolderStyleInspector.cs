using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Reflection;
using System.Linq;

namespace AwesomeFolders
{
	/// <summary>
	/// Sub window shown when user create a new style or edit one
	/// </summary>
	public class NewFolderStyleInspector
	{
		enum ColorType
		{
			Color,
			Radial,
			Axial
		}

		[System.Serializable]
		class StyleProperties
		{
			[SerializeField]
			public ColorType colorType;

			[SerializeField]
			public Gradient gradient = new Gradient();

			[SerializeField]
			public Vector2Int offset;

			[SerializeField]
			public int angle;

			[SerializeField]
			public Vector2Int iconOffset;
		}

		// Style properties
		private string styleName = "New Style";
		private Color styleColor;
		private StyleProperties styleProperties;

		private bool creationSuccess = true;
		private StyleGrid.StyleElement editStyle;
		private IconGrid.IconElement selectedIcon;
		private string lastJsonPath;

		private bool initIconsTexture;	

		// Textures & Baked texture
		private Texture2D folderOriginalTexture64;
		private Texture2D folderTexture64;
		private Texture2D folderOriginalTexture16;
		private Texture2D folderTexture16;

		private Texture2D folderOriginalIcon24;
		private Texture2D folderIcon24;
		private Texture2D folderOriginalIcon10;
		private Texture2D folderIcon10;

		// UI
		private IconGrid iconsUi;
		private bool nameEdited;

		/// <summary>
		/// Prepare the style subwindow
		/// </summary>
		/// <param name="editStyle">If not null set in edit mode</param>
		public NewFolderStyleInspector(StyleGrid.StyleElement editStyle = null)
		{
			this.editStyle = editStyle;
			iconsUi = new IconGrid(ResourceUtil.StyleIconsPath, 36, 4);
			styleProperties = new StyleProperties();

			if (editStyle != null)
			{
				nameEdited = true;
				styleName = editStyle.Name;
				styleColor = editStyle.MainColor;

				selectedIcon = iconsUi.GetIconForId(editStyle.IconId);
				if(selectedIcon != null)
				{
					folderOriginalIcon24 = selectedIcon.HighResTex;
					folderOriginalIcon10 = selectedIcon.LowResTex;
					initIconsTexture = true;
				}

				ReadGradientInfo();
			}
			else
			{
				styleColor = Color.HSVToRGB(Random.value, 0.85F, 0.75F);
				styleName = "New Style";
			}
		}

		/// <summary>
		/// Define window layout
		/// </summary>
		/// <param name="fI"></param>
		public void OnInspectorGUI(FolderInspector fI)
		{
			InitTextures();

			//// Header
			EditorGUILayout.BeginVertical(EditorStyles.helpBox);
			EditorGUILayout.BeginHorizontal();

			string headerTitle = (editStyle != null ? "Edit" : "New") + " style: ";
			Rect labelRect = GUILayoutUtility.GetRect(new GUIContent(headerTitle), "label");
			GUI.Label(labelRect, headerTitle, EditorStyles.boldLabel);

			// Name field
			string newStyleName = EditorGUILayout.TextField(styleName);
			if(!styleName.Equals(newStyleName))
			{
				styleName = newStyleName;
				nameEdited = true;
			}

			EditorGUILayout.EndHorizontal();

			ColorGUI();


			// Icons
			IconGUI();

			GUILayout.Space(2);


			//// Preview
			// Real time change pixels
			UpdateTextureColor(folderOriginalTexture64, folderTexture64, 64, 3, 3 + 3, 56, 50, 4, 10, 1.0F);
			UpdateTextureColor(folderOriginalTexture16, folderTexture16, 16, 2, 3, 15, 13, 0, 1, 0.25F);

			Vector2Int offset = GetClampedBigIconOffset();
			UpdateTextureColor(folderOriginalIcon24, folderIcon24, 64, 32 + offset.x + 3, 14 - offset.y + 6, 56, 50, 4, 10, 1.0F);

			offset = GetClampedSmallIconOffset();
			UpdateTextureColor(folderOriginalIcon10, folderIcon10, 16, 6 + offset.x + 2, -offset.y + 3, 15, 13, 0, 1, 0.25F);

			PreviewGUI();
			EditorGUILayout.EndVertical();

			//// Button pannel
			ControlsGUI(fI);
		}

		private void ColorGUI()
		{
			EditorGUILayout.BeginVertical(EditorStyles.helpBox);
			EditorGUILayout.LabelField("Background", EditorStyles.boldLabel);

			styleProperties.colorType = (ColorType)EditorGUILayout.EnumPopup("Background type", styleProperties.colorType);

			if (styleProperties.colorType == ColorType.Color)
			{
				styleColor = EditorGUI.ColorField(EditorGUILayout.GetControlRect(), "Background color", styleColor);
				styleColor.a = 1.0F;
			}
			else
			{
				MethodInfo[] methods = typeof(EditorGUI).GetMethods(BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public);
				MethodInfo method = methods.FirstOrDefault(t => t.Name == "GradientField" && t.GetParameters().Length == 3 && t.GetParameters()[1].ParameterType == typeof(string));
				if (method == null)
				{
					methods = typeof(EditorGUILayout).GetMethods(BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public);
					method = methods.FirstOrDefault(t => t.Name == "GradientField" && t.GetParameters().Length == 3 && t.GetParameters()[0].ParameterType == typeof(string) && t.GetParameters()[1].ParameterType == typeof(Gradient) && t.GetParameters()[2].ParameterType == typeof(GUILayoutOption[]));
					styleProperties.gradient = (Gradient)method.Invoke(null, new object[] { "Gradient colors", styleProperties.gradient, null });
				}
				else
				{
					styleProperties.gradient = (Gradient)method.Invoke(null, new object[] { EditorGUILayout.GetControlRect(), "Gradient colors", styleProperties.gradient });
				}
				
				styleProperties.offset = DisplayOffsetField("Background Offset", 140, styleProperties.offset);

				if (styleProperties.colorType == ColorType.Axial)
				{
					styleProperties.angle = EditorGUILayout.IntSlider("Gradient angle", styleProperties.angle, 0, 359);
				}
			}

			EditorGUILayout.EndVertical();
		}

		private void IconGUI()
		{
			EditorGUILayout.BeginVertical(EditorStyles.helpBox);

			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Folder Icon", EditorStyles.boldLabel);
			if (GUILayout.Button("None", EditorStyles.miniButton, GUILayout.ExpandWidth(true), GUILayout.Width(40)))
			{
				selectedIcon = null;
				folderIcon24 = folderOriginalIcon24 = null;
				folderIcon10 = folderOriginalIcon10 = null;
				return;
			}
			EditorGUILayout.EndHorizontal();

			ButtonGrid.ButtonAction bA = iconsUi.DrawGrid();
			if (bA != null || initIconsTexture)
			{
				IconGrid.IconElement icon = null;
				if (initIconsTexture)
				{
					icon = selectedIcon;
				}
				else if (bA != null)
				{
					icon = (IconGrid.IconElement)bA.ClickedElement;
					
					if(!nameEdited)
					{
						styleName = bA.ClickedElement.GetElementName().Substring(0, Mathf.Min(bA.ClickedElement.GetElementName().Length, 14));
					}
				}

				selectedIcon = icon;

				// High res icon
				folderIcon24 = new Texture2D(icon.HighResTex.width, icon.HighResTex.height, TextureFormat.RGBA32, true);
				Graphics.CopyTexture(icon.HighResTex, 0, 0, folderIcon24, 0, 0);
				folderOriginalIcon24 = icon.HighResTex;

				// Low res icon
				folderIcon10 = new Texture2D(icon.LowResTex.width, icon.LowResTex.height, TextureFormat.RGBA32, true);
				Graphics.CopyTexture(icon.LowResTex, 0, 0, folderIcon10, 0, 0);
				folderOriginalIcon10 = icon.LowResTex;

				initIconsTexture = false;
			}

			if (selectedIcon != null)
			{
				GUILayout.Space(4);
				styleProperties.iconOffset = DisplayOffsetField("Icon Offset", 100, styleProperties.iconOffset);
			}

			EditorGUILayout.EndVertical();
		}

		/// <summary>
		/// Preview subpart of the window layout
		/// </summary>
		private void PreviewGUI()
		{
			EditorGUILayout.BeginVertical(EditorStyles.helpBox);
			EditorGUILayout.LabelField("Preview", EditorStyles.boldLabel);
			EditorGUILayout.BeginHorizontal();

			// Draw high res texture
			Rect originalRect = EditorGUILayout.GetControlRect();
			Rect rect = originalRect;
			rect.x = rect.width / 2.0F - 32F;
			rect.width = 64;
			rect.height = 64;
			GUI.DrawTexture(rect, folderTexture64, ScaleMode.ScaleToFit, true);
			// Draw high res icon
			if (folderIcon24 != null)
			{
				Rect oldRect = rect;
				rect.width = folderIcon24.width;
				rect.height = folderIcon24.height;
				rect.x += 44 - rect.width * 0.5F + Mathf.Clamp(styleProperties.iconOffset.x, -32, 8);
				rect.y += 38 - rect.height * 0.5F + Mathf.Clamp(styleProperties.iconOffset.y, -26, 14);
				GUI.DrawTexture(rect, folderIcon24, ScaleMode.ScaleToFit, true);

				rect = oldRect;
			}

			// Low res texture
			rect.width = 16;
			rect.height = 16;
			rect.x += 72;
			rect.y += 24;
			GUI.DrawTexture(rect, folderTexture16, ScaleMode.ScaleToFit, true);

			// Low res icon
			if (folderIcon10 != null)
			{
				rect.width = folderIcon10.width;
				rect.height = folderIcon10.height;
				rect.x += 6 + Mathf.Clamp(styleProperties.iconOffset.x / 4, -6, 0);
				rect.y += 6 + Mathf.Clamp(styleProperties.iconOffset.y / 4, -6, 0);
				GUI.DrawTexture(rect, folderIcon10, ScaleMode.ScaleToFit, true);
			}

			EditorGUILayout.EndHorizontal();

			// End Space
			GUILayout.Space(38);
			GUIStyle centeredLabel = new GUIStyle(EditorStyles.label);
			centeredLabel.alignment = TextAnchor.UpperCenter;
			EditorGUILayout.LabelField(styleName, centeredLabel, GUILayout.ExpandWidth(true));
			EditorGUILayout.EndVertical();
		}


		/// <summary>
		/// Controls subpart of the window layout
		/// </summary>
		private void ControlsGUI(FolderInspector fI)
		{
			EditorGUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			if (GUILayout.Button("Back", GUILayout.Width(80.0F)))
			{
				fI.CloseSubWindow();
			}

			bool validName = styleName.Length > 2 && styleName.Length < 15 && styleName.IndexOfAny(Path.GetInvalidFileNameChars()) == -1;
			GUI.enabled = validName;
			if (GUILayout.Button(editStyle != null ? "Save" : "Create", GUILayout.Width(80.0F)))
			{
				Vector2Int highResOffset = GetClampedBigIconOffset();
				Vector2Int lowResOffset = GetClampedSmallIconOffset();

				if (editStyle != null)
				{
					EditStyle(BakeFinalTexture(folderTexture64, folderIcon24, 32 + highResOffset.x, 14 - highResOffset.y), true, "64");
					EditStyle(BakeFinalTexture(folderTexture16, folderIcon10, 6 + lowResOffset.x, -lowResOffset.y), false, "16");

					// Delete previous json info if existing
					if (lastJsonPath != null)
					{
						AssetDatabase.DeleteAsset(lastJsonPath);
					}

					WriteGradientInfo();

					AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
					fI.CloseSubWindow();
				}
				else
				{
					bool success = SaveStyle(BakeFinalTexture(folderTexture64, folderIcon24, 32 + highResOffset.x, 14 - highResOffset.y), "64");
					success = success && SaveStyle(BakeFinalTexture(folderTexture16, folderIcon10, 6 + lowResOffset.x, -lowResOffset.y), "16");
					WriteGradientInfo();

					if (success)
					{
						AssetDatabase.Refresh();
						fI.CloseSubWindow();
					}
					else
					{
						creationSuccess = false;
					}
				}
			}
			GUI.enabled = true;

			EditorGUILayout.EndHorizontal();

			if (!creationSuccess)
			{
				EditorGUILayout.HelpBox("The same style already exists !", MessageType.Error);
			}

			if (!validName)
			{
				EditorGUILayout.HelpBox("Style name is not a valid name !", MessageType.Error);
			}
		}

		/// <summary>
		/// Create required textures based on default ones
		/// </summary>
		private void InitTextures()
		{
			// Initialize textures
			if (folderTexture64 == null)
			{
				folderOriginalTexture64 = (Texture2D)AssetDatabase.LoadMainAssetAtPath(ResourceUtil.TexturesPath + "/folder_icon_64.png");
				folderTexture64 = new Texture2D(folderOriginalTexture64.width, folderOriginalTexture64.height, folderOriginalTexture64.format, false);
				Graphics.CopyTexture(folderOriginalTexture64, 0, 0, folderTexture64, 0, 0);

				folderOriginalTexture16 = (Texture2D)AssetDatabase.LoadMainAssetAtPath(ResourceUtil.TexturesPath + "/folder_icon_16.png");
				folderTexture16 = new Texture2D(folderOriginalTexture16.width, folderOriginalTexture16.height, folderOriginalTexture16.format, false);
				Graphics.CopyTexture(folderOriginalTexture16, 0, 0, folderTexture16, 0, 0);
			}
		}

		/// <summary>
		/// Merge icon & background textures
		/// </summary>
		private Texture2D BakeFinalTexture(Texture2D targetTexture, Texture2D targetIcon, int xOffset, int yOffset)
		{
			if(targetIcon == null)
			{
				return targetTexture;
			}

			Texture2D newTexture = new Texture2D(targetTexture.width, targetTexture.height, targetTexture.format, false);
			Graphics.CopyTexture(targetTexture, 0, 0, newTexture, 0, 0);
			Color[] pixels = newTexture.GetPixels();
			Color[] iconPixels = targetIcon.GetPixels();

			for(int x = 0; x < targetIcon.width; x++)
			{
				int rX = x + xOffset;
				for (int y = 0; y < targetIcon.height; y++)
				{
					int rY = y + yOffset;
					int index = rX + rY * newTexture.width;
					int iconIndex = x + y * targetIcon.width;
					Color targetC = iconPixels[iconIndex];
					targetC.a = 1.0F;
					pixels[index] = Color.Lerp(pixels[index], targetC, iconPixels[iconIndex].a);
				}
			}

			newTexture.SetPixels(pixels);
			newTexture.Apply();

			return newTexture;
		}

		/// <summary>
		/// Save texture into asset database
		/// </summary>
		private bool SaveStyle(Texture2D finalTexture, string suffix)
		{
			string path = ResourceUtil.CustomStylesPath + "/" + ColorUtils.ColorToInt(styleColor) + ";" + GetIconId() + ";" + styleName + "_" + suffix + ".png";
		
			if(!File.Exists(Application.dataPath + "/../" + path))
			{
				byte[] textureBytes = finalTexture.EncodeToPNG();
				File.WriteAllBytes(Application.dataPath + "/../" + path, textureBytes);

				AssetDatabase.ImportAsset(path);

				TextureImporter tI = (TextureImporter)AssetImporter.GetAtPath(path);
				tI.isReadable = true;
				tI.textureType = TextureImporterType.GUI;
				tI.filterMode = FilterMode.Point;
				tI.textureCompression = TextureImporterCompression.Uncompressed;

				tI.SaveAndReimport();

				return true;
			}

			return false;
		}

		private void EditStyle(Texture2D finalTexture, bool highRes, string suffix)
		{
			string newPath = ResourceUtil.CustomStylesPath + "/" + ColorUtils.ColorToInt(styleColor) + ";" + GetIconId() + ";" + styleName + "_" + suffix + ".png";

			// Edit texture info
			byte[] textureBytes = finalTexture.EncodeToPNG();
			Texture2D targetTexture = highRes ? editStyle.HighResTex : editStyle.LowResTex;
			AssetDatabase.MoveAsset(AssetDatabase.GetAssetPath(targetTexture), newPath);
			File.WriteAllBytes(Application.dataPath + "/../" + newPath, textureBytes);
		}

		private void ReadGradientInfo()
		{
			string newPath = ResourceUtil.CustomStylesPath + "/" + ColorUtils.ColorToInt(styleColor) + ";" + GetIconId() + ";" + styleName + "_prop.json";
			if(!File.Exists(newPath))
			{
				return;
			}

			lastJsonPath = newPath;

			StreamReader reader = new StreamReader(newPath);
			string json = reader.ReadToEnd();
			reader.Close();

			styleProperties = JsonUtility.FromJson<StyleProperties>(json);
		}

		private void WriteGradientInfo()
		{
			if(styleProperties.iconOffset == Vector2Int.zero && styleProperties.colorType == ColorType.Color)
			{
				return;
			}

			int iconId = 0;
			if (selectedIcon != null)
			{
				iconId = selectedIcon.Id;
			}

			string newPath = ResourceUtil.CustomStylesPath + "/" + ColorUtils.ColorToInt(styleColor) + ";" + iconId + ";" + styleName + "_prop.json";
			StreamWriter writer = new StreamWriter(newPath, false);
			writer.Write(JsonUtility.ToJson(styleProperties));
			writer.Close();
		}

		private void UpdateTextureColor(Texture2D original, Texture2D modified, int parentSize, int offsetX, int offsetY, int realWidth, int realHeight, int alphaOffsetX, int alphaOffsetY, float offsetMultiplier)
		{
			if (original == null || modified == null)
			{
				return;
			}

			Color[] pixels = original.GetPixels();
			Color[] newPixels = modified.GetPixels();

			int width = (int) Vector2.Distance(new Vector2(alphaOffsetX, alphaOffsetY), new Vector2(alphaOffsetX + realHeight, alphaOffsetY + realHeight));
			int center = width / 2;
			int centerSquared = center * center;

			float angle = 0.0F;
			if(styleProperties.colorType == ColorType.Axial)
			{
				angle = styleProperties.angle;
			}

			float xStep = Mathf.Cos(Mathf.Deg2Rad * angle);
			float yStep = Mathf.Sin(Mathf.Deg2Rad * angle);

			Vector2 start = new Vector2(-xStep * realWidth, -yStep * realHeight);
			Vector2 end = new Vector2(xStep * realWidth, yStep * realHeight);
			Vector2 dir = end - start;

			Vector2Int offset = Vector2Int.zero;
			if (styleProperties.colorType != ColorType.Color)
			{
				offset = styleProperties.offset;
			}

			for (int i = 0; i < pixels.Length; i++)
			{
				Color col = styleColor;

				if (styleProperties.colorType != ColorType.Color)
				{
					int x = i % original.width + offsetX;
					int y = i / original.width + offsetY;
					float gradientPercent;

					if (styleProperties.colorType == ColorType.Radial)
					{
						x -= (int) (offset.x * offsetMultiplier);
						y -= (int) (offset.y * offsetMultiplier);
						int distance = (x - center) * (x - center) + (y - center) * (y - center);
						gradientPercent = distance / (float)centerSquared;
					}
					else // Gradient interface
					{
						x -= (int)(offset.x * offsetMultiplier);
						y -= (int)(offset.y * offsetMultiplier);

						Vector2 pix = new Vector2(x - center, y - center) * 2 - start;
						float dot = Vector2.Dot(pix, dir);
						dot *= 1 / dir.sqrMagnitude;
						gradientPercent = dot;
					}
					col = styleProperties.gradient.Evaluate(Mathf.PingPong(gradientPercent, 1.0F));
				}

				newPixels[i].r = ColorUtils.OverlayBlend(pixels[i].r, col.r);
				newPixels[i].g = ColorUtils.OverlayBlend(pixels[i].g, col.g);
				newPixels[i].b = ColorUtils.OverlayBlend(pixels[i].b, col.b);
			}

			modified.SetPixels(newPixels);
			modified.Apply();
		}

		private Vector2Int DisplayOffsetField(string label, int width, Vector2Int offset)
		{
			Vector2Int o = new Vector2Int();

			Rect labelRect = GUILayoutUtility.GetRect(new GUIContent(label), "label");
			float totalWidth = labelRect.width;
			labelRect.width = width;
			GUI.Label(labelRect, label);

			GUI.Label(new Rect(new Vector2(labelRect.x + totalWidth - 100, labelRect.y), new Vector2(12, labelRect.height)), "X");
			GUI.Label(new Rect(new Vector2(labelRect.x + totalWidth - 50, labelRect.y), new Vector2(12, labelRect.height)), "Y");

			o.x = EditorGUI.IntField(new Rect(new Vector2(labelRect.x + totalWidth - 87, labelRect.y), new Vector2(34, labelRect.height)), offset.x);
			o.y = EditorGUI.IntField(new Rect(new Vector2(labelRect.x + totalWidth - 37, labelRect.y), new Vector2(34, labelRect.height)), offset.y);

			return o;
		}

		private int GetIconId()
		{
			return selectedIcon != null ? selectedIcon.Id : 0;
		}

		private Vector2Int GetClampedBigIconOffset()
		{
			int xOffset = Mathf.Clamp(styleProperties.iconOffset.x, -32, 8);
			int yOffset = Mathf.Clamp(styleProperties.iconOffset.y, -26, 14);

			return new Vector2Int(xOffset, yOffset);
		}

		private Vector2Int GetClampedSmallIconOffset()
		{
			int xOffset = Mathf.Clamp(styleProperties.iconOffset.x / 4, -6, 0);
			int yOffset = Mathf.Clamp(styleProperties.iconOffset.y / 4, -6, 0);

			return new Vector2Int(xOffset, yOffset);
		}
	}
}
