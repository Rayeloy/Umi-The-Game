using UnityEngine;
using System.Linq;

namespace AwesomeFolders
{
	/// <summary>
	/// Display a button grid with icon textures on it
	/// </summary>
	public class IconGrid : ButtonGrid
	{
		/// <summary>
		/// Represent a grid element (button) which is an icon with a name, an Id and a Low & High resolution
		/// </summary>
		public class IconElement : GridElement
		{
			public string Name { get; set; }
			public int Id { get; set; }
			public Texture2D VeryHighResTex { get; set; }
			public Texture2D HighResTex { get; set; }
			public Texture2D LowResTex { get; set; }

			public IconElement(string name, int id)
			{
				Name = name;
				Id = id;
			}

			public override Texture2D GetElementTexture()
			{
				return HighResTex;
			}

			public override string GetElementName()
			{
				return Name;
			}
		}

		/// <summary>
		/// Create a new button grid
		/// </summary>
		/// <param name="path">Path to search for texture</param>
		/// <param name="size">Button size</param>
		/// <param name="offset">Offset between buttons</param>
		public IconGrid(string path, float size, float offset = 0) : base(path, size, offset) { }

		public override GridElement GetElementForTexture(Texture2D tex, string key, string texPath)
		{
			string iconName = "NoName";
			int hashIndex;
			if ((hashIndex = key.IndexOf("#")) != -1)
			{
				iconName = key.Substring(hashIndex + 1, key.Length - hashIndex - 1);
			}

			int id = 0;
			if (hashIndex != -1)
			{
				id = int.Parse(key.Substring(0, hashIndex));
			}

			if (tex.width == 48 || tex.width == 24 || tex.width == 10)
			{
				return new IconElement(iconName, id);
			}

			return null;
		}

		public override void OnElementFound(GridElement element, Texture2D tex)
		{
			if (tex.width == 48)
			{
				((IconElement)element).VeryHighResTex = tex;
			}
			else if (tex.width == 24)
			{
				((IconElement)element).HighResTex = tex;
			}
			else if (tex.width == 10)
			{
				((IconElement)element).LowResTex = tex;
			}
		}

		public override void OnAllElementFound(){}

		public IconElement GetIconForId(int id)
		{
			return (IconElement) elementList.Find(x => ((IconElement)x).Id == id);
		}
	}
}
