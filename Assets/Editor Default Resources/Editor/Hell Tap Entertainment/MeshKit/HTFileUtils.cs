////////////////////////////////////////////////////////////////////////////////////////////////
//
//  HTEFileUtils.cs
//
//	Makes some file / system related helper functions statically available.
//
//	© 2015 Melli Georgiou.
//	Hell Tap Entertainment LTD
//
////////////////////////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;
using System.IO;
using HellTap.MeshKit;

// Use HellTap Namespace
namespace HellTap.MeshKit {

	// Class
	static class FileUtils{

		////////////////////////////////////////////////////////////////////////////////////////////////
		//	MAKE STRING FILESYSTEM SAFE
		//	Makes A String safe to saving to a file - can also replace bad characters with a chosen char
		////////////////////////////////////////////////////////////////////////////////////////////////

		public static string MakeFileSystemSafe( this string s, char replaceWith  ){ // to replace with blank, enter null as variable
			
			// Get the invalid characters	
			char[] invalidChars = Path.GetInvalidFileNameChars();

			// Create a new string based on those invalid chars, and add the three important ones - / and \ and :
			string invalidCharacters = new string( invalidChars );
			invalidCharacters += "/\\:";

			// Create an ArrayList to build up the new string
			ArrayList newStringArray = new ArrayList();
			newStringArray.Clear();

			// Loop through the characters in the string and check to see if they conflict with any of the invalid chars.
			foreach( char c in s ){
				bool replaced = false;
				foreach( char unsafeChar in invalidCharacters ){
					// If the character is an unsafe one ...
					if( c == unsafeChar ){
						// If our replaceWith variable is not empty, add the replaced char into the array
						newStringArray.Add(replaceWith);
						replaced = true;
						break;	// Break this loop and continue to the next character in the core string.
					}
				}
				// If this char was fine, add it to the array
				if(!replaced){ newStringArray.Add(c); }
			}

			// Recreate a new return string based on the characters in the newStringArray
			string returnString = "";
			foreach( char part in newStringArray ){
				returnString += part;
			}
			// Debug.Log("MAKE FILESYSTEM SAFE RETURN STRING: "+returnString);

			return returnString;
		}

		////////////////////////////////////////////////////////////////////////////////////////////////
		//	MAKE STRING FILESYSTEM SAFE
		//	Makes A String safe to saving to a file
		////////////////////////////////////////////////////////////////////////////////////////////////

		public static string MakeFileSystemSafe( this string s  ){
				
			// Get the invalid characters	
			char[] invalidChars = Path.GetInvalidFileNameChars();

			// Create a new string based on those invalid chars, and add the two important ones - / and \
			string invalidCharacters = new string( invalidChars );
			invalidCharacters += "/\\";

			// Create an ArrayList to build up the new string
			ArrayList newStringArray = new ArrayList();
			newStringArray.Clear();

			// Loop through the characters in the string and check to see if they conflict with any of the invalid chars.
			foreach( char c in s ){
				bool replaced = false;
				foreach( char unsafeChar in invalidCharacters ){
					// If the character is an unsafe one we skip it, effectively deleting it...
					if( c == unsafeChar ){
						break;	// Break this loop and continue to the next character in the core string.
					}
				}
				// If this char was fine, add it to the array
				if(!replaced){ newStringArray.Add(c); }
			}

			// Recreate a new return string based on the characters in the newStringArray
			string returnString = "";
			foreach( char part in newStringArray ){
				returnString += part;
			}
			// Debug.Log("MAKE FILESYSTEM SAFE RETURN STRING: "+returnString);

			return returnString;
		}
	}
}
