using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using System;

namespace AwesomeFolders
{
	/// <summary>
	/// Display a button grid with textures on it
	/// </summary>
	public abstract class ButtonGrid
	{
		protected string path;
		protected float size;
		protected float offset;
		public List<GridElement> elementList;

		/// <summary>
		/// Represent a grid element (button) with a texture and a name
		/// </summary>
		public abstract class GridElement
		{
			public abstract Texture2D GetElementTexture();
			public abstract string GetElementName();
		}

		/// <summary>
		/// Class returned on a button click
		/// Detect right or left click & send grid element
		/// </summary>
		public class ButtonAction
		{
			public GridElement ClickedElement { get; private set; }
			public bool IsLeftClick { get; private set; }

			public ButtonAction(GridElement element, bool isLeftClick)
			{
				ClickedElement = element;
				IsLeftClick = isLeftClick;
			}
		}

		/// <summary>
		/// Create a new button grid
		/// </summary>
		/// <param name="path">Path to search for texture</param>
		/// <param name="size">Button size</param>
		/// <param name="offset">Offset between buttons</param>
		public ButtonGrid(string path, float size, float offset = 0)
		{
			this.path = path;
			this.size = size;
			this.offset = offset;

			Init();
		}

		/// <summary>
		/// Load textures files
		/// </summary>
		public void Init()
		{
			// Load all styles
			string[] files = AssetDatabase.FindAssets("t:texture2D", new string[] { path });
			Dictionary<string, GridElement> elementDictionnary = new Dictionary<string, GridElement>();
			for (int i = 0; i < files.Length; i++)
			{
				string texPath = AssetDatabase.GUIDToAssetPath(files[i]);
				Texture2D tex = (Texture2D)AssetDatabase.LoadAssetAtPath(texPath, typeof(Texture2D));

				// Is File a texture ?
				if (tex != null)
				{
					string key = tex.name.Substring(0, tex.name.Length - 3);
					GridElement element = null;

					if (!elementDictionnary.ContainsKey(key))
					{
						element = GetElementForTexture(tex, key, texPath);

						if(element != null)
						{
							elementDictionnary.Add(key, element);
						}
					}

					if (elementDictionnary.ContainsKey(key))
					{
						OnElementFound(elementDictionnary[key], tex);
					}
				}
			}

			// Convert dictionary to list & sort it by date
			elementList = elementDictionnary.Values.ToList();
			OnAllElementFound();
		}

		/// <summary>
		/// Draw grid of button & handle clicks
		/// </summary>
		/// <returns>ButtonAction instance if clicked, null otherwise</returns>
		public ButtonAction DrawGrid()
		{
			EditorGUILayout.BeginVertical();

			int perRow = (int)((EditorGUIUtility.currentViewWidth - offset * 2) / (size + 2));
			int end = (int)Mathf.Ceil(elementList.Count / (float)perRow);
			float remainingSpace = EditorGUIUtility.currentViewWidth - offset * 2 - (perRow * (size + 2));
			float additionnalSpace = remainingSpace / perRow;

			Rect rect = EditorGUILayout.GetControlRect();

			float rectY = rect.y;
			for (int y = 0; y < end; y++)
			{
				for (int x = 0; x < perRow; x++)
				{
					int index = y * perRow + x;
					if (index >= elementList.Count)
					{
						break;
					}
					
					rect.x = x * (size + 2 + additionnalSpace) + offset;
					rect.y = y * (size + 2) + rectY;
					rect.width = size;
					rect.height = size;

					if (GUI.Button(rect, new GUIContent("", elementList[index].GetElementTexture(), elementList[index].GetElementName())))
					{
						if (Event.current.type == EventType.Used && Event.current.button == 0)
						{
							return new ButtonAction(elementList[index], true);
						}
						else if (Event.current.type == EventType.Used && Event.current.button == 1)
						{
							return new ButtonAction(elementList[index], false);
						}
					}

					OnButtonDrawn(elementList[index], rect);
				}
			}
			GUILayout.Space(end * (size + 2) - 16);
			EditorGUILayout.EndVertical();

			return null;
		}

		public abstract GridElement GetElementForTexture(Texture2D tex, string key, string texPath);

		public abstract void OnElementFound(GridElement element, Texture2D tex);

		public abstract void OnAllElementFound();

		public virtual void OnButtonDrawn(GridElement element, Rect rect) { }
	}
}
