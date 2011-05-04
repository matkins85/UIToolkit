/*
	This file can be used as a template for making custom UI controls.  Just rename the file and class and get coding!
*/
using UnityEngine;


public class UIControlTemplate : UITouchableSprite
{
	private UIUVRect _normalUVframe; // Holds a copy of the uvFrame that the button was initialized with
	public UIUVRect highlightedUVframe;

	
	
	#region Constructors/Destructor
	
	/*
	public static UIControlTemplate create( string filename, string highlightedFilename, int xPos, int yPos, int depth = 1 )
	{
		// create and return a new UIControlTemplate
	}
	*/

	public UIControlTemplate( Rect frame, int depth, UIUVRect uvFrame, UIUVRect highlightedUVframe ):base( frame, depth, uvFrame )
	{
		// Save a copy of our uvFrame here so that when highlighting turns off we have the original UVs
		_normalUVframe = uvFrame;
		
		// If a highlighted frame has not yet been set use the normalUVframe
		if( highlightedUVframe == UIUVRect.zero )
			highlightedUVframe = uvFrame;
		
		this.highlightedUVframe = highlightedUVframe;
	}

	#endregion;


	// Sets the uvFrame of the original UISprite and resets the _normalUVFrame for reference when highlighting
	public override UIUVRect uvFrame
	{
		get { return _uvFrame; }
		set
		{
			_uvFrame = value;
			_normalUVframe = value;
			updateUVs();
		}
	}

	
	public override bool highlighted
	{
		set
		{
			// Only set if it is different than our current value
			if( _highlighted != value )
			{			
				_highlighted = value;
				
				if ( value )
					base.uvFrame = highlightedUVframe;
				else
					base.uvFrame = _normalUVframe;
			}
		}
	}


	// Touch handlers
#if UNITY_EDITOR
	public override void onTouchBegan( UIFakeTouch touch, Vector2 touchPos )
#else
	public override void onTouchBegan( Touch touch, Vector2 touchPos )
#endif
	{
		highlighted = true;
	}


	#if UNITY_EDITOR
		public override void onTouchMoved( UIFakeTouch touch, Vector2 touchPos )
	#else
		public override void onTouchMoved( Touch touch, Vector2 touchPos )
	#endif
		{
			highlighted = true;
		}


#if UNITY_EDITOR
	public override void onTouchEnded( UIFakeTouch touch, Vector2 touchPos, bool touchWasInsideTouchFrame )
#else
	public override void onTouchEnded( Touch touch, Vector2 touchPos, bool touchWasInsideTouchFrame )
#endif
	{
		highlighted = false;
	}


}