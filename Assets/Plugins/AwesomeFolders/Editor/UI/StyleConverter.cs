using UnityEngine;
using System.Collections;
using UnityEditor;
using System.IO;

namespace AwesomeFolders
{
	public class StyleConverter
	{
		// Quick & (really) dirty converter
		public static void ConvertStyles()
		{
			StyleGrid sg = new StyleGrid(ResourceUtil.CustomStylesPath, 64.0F + 16.0F, 8.0F);
			IconGrid iconsUi = new IconGrid(ResourceUtil.StyleIconsPath, 36, 12);

			foreach (ButtonGrid.GridElement ge in sg.elementList)
			{
				StyleGrid.StyleElement style = (StyleGrid.StyleElement) ge;
				NewFolderStyleInspector.StyleProperties styleProperties = new NewFolderStyleInspector.StyleProperties();
				IconGrid.IconElement selectedIcon = iconsUi.GetIconForId(style.IconId);
				string styleName = style.Name;
				Color styleColor = style.MainColor;

				string lastJsonPath = null;
				string newPath = ResourceUtil.CustomStylesPath + "/" + ColorUtils.ColorToInt(styleColor) + ";" + GetIconId(selectedIcon) + ";" + styleName + "_prop.json";
				if (File.Exists(newPath))
				{
					StreamReader reader = new StreamReader(newPath);
					string json = reader.ReadToEnd();
					reader.Close();

					styleProperties = JsonUtility.FromJson<NewFolderStyleInspector.StyleProperties>(json);

					lastJsonPath = newPath;
				}

				Texture2D folderOriginalTexture64;
				Texture2D folderOriginalTexture16;
				if (PreferencesUI.settings.useNewUI)
				{
					folderOriginalTexture64 = (Texture2D)AssetDatabase.LoadMainAssetAtPath(ResourceUtil.TexturesPath + "/folder_icon_new.png");
					folderOriginalTexture16 = (Texture2D)AssetDatabase.LoadMainAssetAtPath(ResourceUtil.TexturesPath + "/folder_icon_new_16.png");
				}
				else
				{
					folderOriginalTexture64 = (Texture2D)AssetDatabase.LoadMainAssetAtPath(ResourceUtil.TexturesPath + "/folder_icon_64.png");
					folderOriginalTexture16 = (Texture2D)AssetDatabase.LoadMainAssetAtPath(ResourceUtil.TexturesPath + "/folder_icon_16.png");
				}

				Texture2D folderTexture64;
				Texture2D folderTexture16;
				folderTexture64 = new Texture2D(folderOriginalTexture64.width, folderOriginalTexture64.height, folderOriginalTexture64.format, false);
				Graphics.CopyTexture(folderOriginalTexture64, 0, 0, folderTexture64, 0, 0);

				folderTexture16 = new Texture2D(folderOriginalTexture16.width, folderOriginalTexture16.height, folderOriginalTexture16.format, false);
				Graphics.CopyTexture(folderOriginalTexture16, 0, 0, folderTexture16, 0, 0);

				Texture2D folderIcon48 = null;
				Texture2D folderOriginalIcon48 = null;
				Texture2D folderIcon24 = null;
				Texture2D folderOriginalIcon24 = null;
				Texture2D folderIcon10 = null;
				Texture2D folderOriginalIcon10 = null;

				if (selectedIcon != null)
				{
					// Very High res icon
					folderIcon48 = new Texture2D(selectedIcon.VeryHighResTex.width, selectedIcon.VeryHighResTex.height, TextureFormat.RGBA32, true);
					Graphics.CopyTexture(selectedIcon.VeryHighResTex, 0, 0, folderIcon48, 0, 0);
					folderOriginalIcon48 = selectedIcon.VeryHighResTex;

					// High res icon
					folderIcon24 = new Texture2D(selectedIcon.HighResTex.width, selectedIcon.HighResTex.height, TextureFormat.RGBA32, true);
					Graphics.CopyTexture(selectedIcon.HighResTex, 0, 0, folderIcon24, 0, 0);
					folderOriginalIcon24 = selectedIcon.HighResTex;

					// Low res icon
					folderIcon10 = new Texture2D(selectedIcon.LowResTex.width, selectedIcon.LowResTex.height, TextureFormat.RGBA32, true);
					Graphics.CopyTexture(selectedIcon.LowResTex, 0, 0, folderIcon10, 0, 0);
					folderOriginalIcon10 = selectedIcon.LowResTex;
				}

				int xOffset = Mathf.Clamp(styleProperties.iconOffset.x, -32, 8);
				int yOffset = Mathf.Clamp(styleProperties.iconOffset.y, -26, 14);
				Vector2Int bigOffset = new Vector2Int(xOffset, yOffset);

				xOffset = Mathf.Clamp(styleProperties.iconOffset.x / 4, -6, 0);
				yOffset = Mathf.Clamp(styleProperties.iconOffset.y / 4, -6, 0);
				Vector2Int smallOffset = new Vector2Int(xOffset, yOffset);

				if (PreferencesUI.settings.useNewUI)
				{
					UpdateTextureColor(styleProperties, styleColor, folderOriginalTexture64, folderTexture64, -7, 2, 96, 80, 16, 24, 2.0F);
					UpdateTextureColor(styleProperties, styleColor, folderOriginalTexture16, folderTexture16, 1, 3, 15, 13, 2, 3, 0.25F);

					UpdateTextureColor(styleProperties, styleColor, folderOriginalIcon24, folderIcon24, 32 + bigOffset.x - 3, 14 - bigOffset.y + 1, 48, 40, 8, 16, 1.0F);
					UpdateTextureColor(styleProperties, styleColor, folderOriginalIcon10, folderIcon10, 6 + smallOffset.x + 1, -smallOffset.y + 3, 15, 13, 2, 3, 0.25F);
				}
				else
				{
					UpdateTextureColor(styleProperties, styleColor, folderOriginalTexture64, folderTexture64, 3, 7, 56, 50, 3, 10, 1.0F);
					UpdateTextureColor(styleProperties, styleColor, folderOriginalTexture16, folderTexture16, 2, 3, 15, 13, 0, 1, 0.25F);

					
					UpdateTextureColor(styleProperties, styleColor, folderOriginalIcon24, folderIcon24, 32 + bigOffset.x + 3, 14 - bigOffset.y + 6, 56, 50, 4, 10, 1.0F);
					UpdateTextureColor(styleProperties, styleColor, folderOriginalIcon10, folderIcon10, 6 + smallOffset.x + 2, -smallOffset.y + 3, 15, 13, 0, 1, 0.25F);
				}

				Texture2D finalHigh;
				if (PreferencesUI.settings.useNewUI)
				{
					UpdateTextureColor(styleProperties, styleColor, folderOriginalIcon48, folderIcon48, (32 + bigOffset.x - 3) * 2, (14 - bigOffset.y + 1) * 2, 96, 80, 16, 24, 2.0F);
					finalHigh = BakeFinalTexture(folderTexture64, folderIcon48, (32 + bigOffset.x) * 2, (14 - bigOffset.y) * 2);
				}
				else
				{
					finalHigh = BakeFinalTexture(folderTexture64, folderIcon24, 32 + bigOffset.x, 14 - bigOffset.y);
				}

				Texture2D finalLow = BakeFinalTexture(folderTexture16, folderIcon10, 6 + smallOffset.x, -smallOffset.y);
				EditStyle(styleName, styleColor, style, selectedIcon, finalHigh, true, "64");
				EditStyle(styleName, styleColor, style, selectedIcon, finalLow, false, "16");

				// Delete previous json info if existing
				if (lastJsonPath != null)
				{
					AssetDatabase.DeleteAsset(lastJsonPath);
				}

				if (!(styleProperties.iconOffset == Vector2Int.zero && styleProperties.colorType == NewFolderStyleInspector.ColorType.SolidColor))
				{
					int iconId = 0;
					if (selectedIcon != null)
					{
						iconId = selectedIcon.Id;
					}

					StreamWriter writer = new StreamWriter(lastJsonPath, false);
					writer.Write(JsonUtility.ToJson(styleProperties));
					writer.Close();
				}
			}

			AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
		}

		private static void EditStyle(string styleName, Color styleColor, StyleGrid.StyleElement editStyle, IconGrid.IconElement selectedIcon, Texture2D finalTexture, bool highRes, string suffix)
		{
			string newPath = ResourceUtil.CustomStylesPath + "/" + ColorUtils.ColorToInt(styleColor) + ";" + GetIconId(selectedIcon) + ";" + styleName + "_" + suffix + ".png";

			// Edit texture info
			byte[] textureBytes = finalTexture.EncodeToPNG();
			Texture2D targetTexture = highRes ? editStyle.HighResTex : editStyle.LowResTex;
			AssetDatabase.MoveAsset(AssetDatabase.GetAssetPath(targetTexture), newPath);
			File.WriteAllBytes(Application.dataPath + "/../" + newPath, textureBytes);
		}

		private static void UpdateTextureColor(NewFolderStyleInspector.StyleProperties styleProperties, Color styleColor, Texture2D original, Texture2D modified, int offsetX, int offsetY, int realWidth, int realHeight, int alphaOffsetX, int alphaOffsetY, float offsetMultiplier)
		{
			if (original == null || modified == null)
			{
				return;
			}

			Color[] pixels = original.GetPixels();
			Color[] newPixels = modified.GetPixels();

			int width = (int)Vector2.Distance(new Vector2(alphaOffsetX, alphaOffsetY), new Vector2(alphaOffsetX + realHeight, alphaOffsetY + realHeight));
			int center = width / 2;

			float angle = 0.0F;
			if (styleProperties.colorType == NewFolderStyleInspector.ColorType.AxialGradient)
			{
				angle = styleProperties.angle;
			}

			float xStep = Mathf.Cos(Mathf.Deg2Rad * angle);
			float yStep = Mathf.Sin(Mathf.Deg2Rad * angle);

			Vector2 start = new Vector2(-xStep * realWidth, -yStep * realHeight);
			Vector2 end = new Vector2(xStep * realWidth, yStep * realHeight);
			Vector2 dir = end - start;

			Vector2Int offset = Vector2Int.zero;
			if (styleProperties.colorType != NewFolderStyleInspector.ColorType.SolidColor)
			{
				offset = styleProperties.offset;
			}

			for (int i = 0; i < pixels.Length; i++)
			{
				Color col = styleColor;

				if (styleProperties.colorType != NewFolderStyleInspector.ColorType.SolidColor)
				{
					int x = i % original.width + offsetX;
					int y = i / original.width + offsetY;
					float gradientPercent;

					if (styleProperties.colorType == NewFolderStyleInspector.ColorType.RadialGradient)
					{
						x -= (int)(offset.x * offsetMultiplier);
						y -= (int)(offset.y * offsetMultiplier);
						int distance = (x - center) * (x - center) + (y - center) * (y - center);
						gradientPercent = Mathf.Sqrt(distance) / (float)center;
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

		private static int GetIconId(IconGrid.IconElement selectedIcon)
		{
			return selectedIcon != null ? selectedIcon.Id : 0;
		}

		/// <summary>
		/// Merge icon & background textures
		/// </summary>
		private static Texture2D BakeFinalTexture(Texture2D targetTexture, Texture2D targetIcon, int xOffset, int yOffset)
		{
			if (targetIcon == null)
			{
				return targetTexture;
			}

			Texture2D newTexture = new Texture2D(targetTexture.width, targetTexture.height, targetTexture.format, false);
			Graphics.CopyTexture(targetTexture, 0, 0, newTexture, 0, 0);
			Color[] pixels = newTexture.GetPixels();
			Color[] iconPixels = targetIcon.GetPixels();

			for (int x = 0; x < targetIcon.width; x++)
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
	}
}
