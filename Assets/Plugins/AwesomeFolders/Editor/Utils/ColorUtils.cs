using UnityEngine;

namespace AwesomeFolders
{
	/// <summary>
	/// Contains utils method to use with colors
	/// </summary>
	public class ColorUtils
	{
		/// <summary>
		/// Convert a color to int
		/// </summary>
		/// <param name="color">Color to convert</param>
		/// <returns>rgba value</returns>
		public static int ColorToInt(Color color)
		{
			int rgba = (int)(color.r * 255.0F);
			rgba = (rgba << 8) + (int)(color.g * 255.0F);
			rgba = (rgba << 8) + (int)(color.b * 255.0F);
			rgba = (rgba << 8) + (int)(color.a * 255.0F);

			return rgba;
		}

		/// <summary>
		/// Convert rgba value to Color
		/// </summary>
		/// <param name="rgb">rgba value to convert</param>
		/// <returns>A Color</returns>
		public static Color IntToColor(int rgba)
		{
			float red = ((rgba >> 24) & 0xFF) / 255.0F;
			float green = ((rgba >> 16) & 0xFF) / 255.0F;
			float blue = ((rgba >> 8) & 0xFF) / 255.0F;
			float alpha = (rgba & 0xFF) / 255.0F;

			return new Color(red, green, blue, alpha);
		}

		/// <summary>
		/// Overlay blend function to handle color change
		/// </summary>
		/// <param name="originalColor">Original color value</param>
		/// <param name="topColor">New value</param>
		/// <returns>Both color value combined</returns>
		public static float OverlayBlend(float originalColor, float topColor)
		{
			// Overlay blend function
			if (originalColor > 0.5F)
			{
				float value = (1.0F - originalColor) / 0.5F;
				float min = originalColor - (1.0F - originalColor);
				return topColor * value + min;
			}
			else
			{
				float value = originalColor / 0.5F;
				return topColor * value;
			}
		}
	}
}
