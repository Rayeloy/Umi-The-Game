// by @torahhorse

using UnityEngine;
using System.Collections;

namespace MeshMakerNamespace
{
	public class LockMouse : MonoBehaviour
	{	
		void Start()
		{
			LockCursor(true);
		}
	
	    void Update()
	    {
	    	// lock when mouse is clicked
	    	if( Input.GetMouseButtonDown(0) && Time.timeScale > 0.0f )
	    	{
	    		LockCursor(true);
	    	}
	    
	    	// unlock when escape is hit
	        if  ( Input.GetKeyDown(KeyCode.Escape) )
	        {
				if (Cursor.lockState == CursorLockMode.Locked)
					LockCursor(false);
	        	//LockCursor(!Screen.lockCursor);
	        }
	    }
	    
	    public void LockCursor(bool lockCursor)
	    {
			if (lockCursor == true)
				Cursor.lockState = CursorLockMode.Locked;
	    	//Screen.lockCursor = lockCursor;
	    }
	}
}