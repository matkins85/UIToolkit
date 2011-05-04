using UnityEngine;
using System.Collections.Generic;


public class UISprite : System.Object
{
    public UISpriteManager manager = null;      // Reference to the sprite manager in which this sprite resides
    public bool ___hidden = false;   // Indicates whether this sprite is currently hidden (has to be public because C# has no "friend" feature, just don't access directly from outside)

    public float width;  // Width and Height of the sprite in worldspace units. DO NOT SET THESE
    public float height; // THESE ARE PUBLIC TO AVOID THE GETTER OVERHEAD
	public bool gameObjectOriginInCenter = false;  // Set to true to get your origin in the center.  Useful for scaling/rotating
    protected GameObject client;        // Reference to the client GameObject
	protected UIUVRect _uvFrame;		// UV coordinates and size for the sprite
	
    protected Vector3[] meshVerts = new Vector3[4];        // Pointer to the array of vertices in the mesh
    protected Vector2[] UVs;              // Pointer to the array of UVs in the mesh
	protected Dictionary<string, UISpriteAnimation> spriteAnimations;
	
    public Transform clientTransform;         // Cached Transform of the client GameObject
    public Color _color = Color.white;       // The color to be used by all four vertices

    public int index;                     // Index of this sprite in its SpriteManager's list

    public Vector3 v1 = new Vector3();      // The sprite's vertices in local space
    public Vector3 v2 = new Vector3();
    public Vector3 v3 = new Vector3();
    public Vector3 v4 = new Vector3();

	// Indices of the associated vertices in the actual mesh (shortcut to get straight to the right vertices in the vertex array)
	// Also houses indices of UVs in the mesh and color values
	public UIVertexIndices vertexIndices;
	public Rect frame;
	
	
	public UISprite( Rect frame, int depth, UIUVRect uvFrame ):this( frame, depth, uvFrame, false )
	{
		
	}
	

    public UISprite( Rect frame, int depth, UIUVRect uvFrame, bool gameObjectOriginInCenter )
    {
		this.gameObjectOriginInCenter = gameObjectOriginInCenter;
		
		// Setup our GO
        client = new GameObject( "UIElement" );
        clientTransform = client.transform;
        clientTransform.parent = UI.instance.transform;
        client.layer = UI.instance.layer;
        client.transform.position = new Vector3( frame.x, -frame.y, 5 );
        client.AddComponent<MeshFilter>();
        client.AddComponent<MeshRenderer>();
        client.GetComponent<MeshRenderer>().material = UI.instance.material;
        
        var mesh = new Mesh();
        client.GetComponent<MeshFilter>().mesh = mesh;
        
        mesh.vertices = new Vector3[4];
        mesh.uv = new Vector2[4];
        mesh.triangles = new int[] { 0, 1, 3, 3, 1, 2 };
        
        mesh.RecalculateBounds();
		
		// Save these for later.  The manager will call initializeSize() when the UV's get setup
		width = frame.width;
		height = frame.height;
		this.frame = frame;
		
		_uvFrame = uvFrame;
    }
    
    
    public void setClient( GameObject clientGO )
    {
    	client = clientGO;
		client.transform.parent = UI.instance.transform; // Just for orginization in the hierarchy
		client.layer = UI.instance.layer; // Set the proper layer so we only render on the UI camera
		//client.transform.position = new Vector3( frame.x, -frame.y, depth ); // Depth will affect z-index

		// Cache the clientTransform
		clientTransform = client.transform;
    }


	public virtual UIUVRect uvFrame
	{
		get { return _uvFrame; }
		set
		{
			// Dont bother changing if the new value isn't different
			if( _uvFrame != value )
			{
				_uvFrame = value;
				updateUVs();
			}
		}
	}


    public bool hidden
    {
        get { return ___hidden; }
        set
        {
            // No need to do anything if we're already in this state:
            if( value == ___hidden )
                return;

            if( value )
                manager.hideSprite( this );
            else
                manager.showSprite( this );
        }
    }
	
	
	public void updateUVs()
	{
		var meshFilter = client.GetComponent<MeshFilter>();
		var mesh = meshFilter.mesh;
		var uvs = mesh.uv;

		uvs[0] = this.uvFrame.lowerLeftUV + Vector2.up * this.uvFrame.uvDimensions.y;  // Upper-left
		uvs[1] = this.uvFrame.lowerLeftUV;                              // Lower-left
		uvs[2] = this.uvFrame.lowerLeftUV + Vector2.right * this.uvFrame.uvDimensions.x;// Lower-right
		uvs[3] = this.uvFrame.lowerLeftUV + this.uvFrame.uvDimensions;     // Upper-right

		mesh.uv = uvs;
	}

	// This gets called by the manager just after the UV's get setup
	public void initializeSize()
	{
		setSize( width, height );
		manager.updateUV( this );
	}

	
    // Sets the physical dimensions of the sprite in the XY plane
    public void setSize( float width, float height )
    {
        this.width = width;
        this.height = height;
		
		if( gameObjectOriginInCenter )
		{
			// Some objects need to rotate so we set the origin at the center of the GO
			Vector3 offset = Vector3.zero;
			v1 = offset + new Vector3( -width / 2, height / 2, 0 );   // Upper-left
			v2 = offset + new Vector3( -width / 2, -height / 2, 0 );  // Lower-left
			v3 = offset + new Vector3( width / 2, -height / 2, 0 );   // Lower-right
			v4 = offset + new Vector3( width / 2, height / 2, 0 );    // Upper-right
		}
		else
		{
			// Make the origin the top-left corner of the GO
	        v1 = new Vector3( 0, 0, 0 );   // Upper-left
	        v2 = new Vector3( 0, -height, 0 );  // Lower-left
	        v3 = new Vector3( width, -height, 0 );   // Lower-right
	        v4 = new Vector3( width, 0, 0 );    // Upper-right
		}
		
		var meshFilter = client.GetComponent<MeshFilter>();
		var mesh = meshFilter.mesh;
		var verts = mesh.vertices;
		var uvs = mesh.uv;
		
		verts[0] = v1;
		verts[1] = v2;
		verts[2] = v3;
		verts[3] = v4;
		
		uvs[0] = this.uvFrame.lowerLeftUV + Vector2.up * this.uvFrame.uvDimensions.y;  // Upper-left
		uvs[1] = this.uvFrame.lowerLeftUV;                              // Lower-left
		uvs[2] = this.uvFrame.lowerLeftUV + Vector2.right * this.uvFrame.uvDimensions.x;// Lower-right
		uvs[3] = this.uvFrame.lowerLeftUV + this.uvFrame.uvDimensions;     // Upper-right
		
		//verts[0] = clientTransform.TransformPoint( v1 );
		//verts[1] = clientTransform.TransformPoint( v2 );
		//verts[2] = clientTransform.TransformPoint( v3 );
		//verts[3] = clientTransform.TransformPoint( v4 );
		
		/*
		Debug.Log( "--------------------" );
		Debug.Log(v1);
		Debug.Log(v2);
		Debug.Log(v3);
		Debug.Log(v4);
		
		foreach( var v in mesh.vertices )
			Debug.Log( v );
		*/
		
		mesh.vertices = verts;
		mesh.uv = uvs;
		
		//mesh.RecalculateNormals();
		
		//meshFilter.sharedMesh = mesh;
		mesh.RecalculateBounds();
		
        updateTransform();
    }
	

    // Sets the vertex and UV buffers
    public void setBuffers( Vector3[] v, Vector2[] uv )
    {
        meshVerts = v;
        UVs = uv;
    }
	

    // Applies the transform of the client GameObject and stores the results in the associated vertices of the overall mesh
    public virtual void updateTransform()
    {

		
        manager.updatePositions();
    }
	
	
	// sets the sprites to have its origin at it's center and repositions it so it doesn't move from
	// a non centered origin
	public virtual void centerize()
	{
		if( gameObjectOriginInCenter )
			return;
		
		// offset our sprite in the x and y direction to "fix" the change that occurs when we reset to center
		var pos = clientTransform.position;
		pos.x += width / 2;
		pos.y -= height / 2;
		clientTransform.position = pos;
		
		gameObjectOriginInCenter = true;
		setSize( width, height );
	}
	

    // Sets the specified color and automatically notifies the GUISpriteManager to update the colors
	public Color color
	{
		get { return _color; }
		set
		{
			_color = value;
			
			var meshFilter = client.GetComponent<MeshFilter>();
			var mesh = meshFilter.mesh;
			var colors = mesh.colors;
			
			// initialize the colors array if we dont have one
			if( colors.Length != 4 )
				colors = new Color[4];
			
			for( var i = 0; i < colors.Length; i++ )
				colors[i] = _color;
			
			mesh.colors = colors;
		}
	}
	
	
	#region Sprite Animation methods
	
	
	public UISpriteAnimation addSpriteAnimation( string name, float frameTime, params string[] filenames )
	{
		// create the spriteAnimations dictionary on demand
		if( spriteAnimations == null )
			spriteAnimations = new Dictionary<string, UISpriteAnimation>();
		
		// get the UIUVRects for the sprite frames
		var uvRects = new List<UIUVRect>( filenames.Length );
		
		foreach( var filename in filenames )
			uvRects.Add( UI.instance.uvRectForFilename( filename ) );
		
		var anim = new UISpriteAnimation( frameTime, uvRects );
		spriteAnimations[name] = anim;
		
		return anim;
	}
	
	
	public void playSpriteAnimation( string name, int loopCount )
	{
#if UNITY_EDITOR
		// sanity check while in editor
		if( !spriteAnimations.ContainsKey( name ) )
			throw new System.Exception( "can't find sprite animation with name:" + name );
#endif
	
		playSpriteAnimation( spriteAnimations[name], loopCount );
	}
	
	
	public void playSpriteAnimation( UISpriteAnimation anim, int loopCount )
	{
		UI.instance.StartCoroutine( anim.play( this, loopCount ) );
	}
	
	#endregion
	
}

