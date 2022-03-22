using SpriteKit.Asset;
using SpriteKit.Entities;
using static SpriteKit.Entities.ModelSprite;

namespace SpriteKit.SceneObjects;

public class SpriteSceneObject : SceneCustomObject
{
	private SpriteAsset _spriteAsset => _parent?.SpriteAsset;
	private ModelSprite _parent;
	public static Material SpriteMaterial = Material.Create( "SpriteMaterial", "spritekit_sprite" );
	private VertexBuffer VertexBuffer;
	private SpriteArea currentone;
	private SpriteArea _spriteArea
	{
		get
		{
			if ( currentone == null )
			{
				currentone = _spriteAsset.SpriteAreas[0];
			}
			if ( currentone != _parent.ActiveSpriteArea )
			{
				currentone = _parent.ActiveSpriteArea;
				Init();
			}
			return _parent.ActiveSpriteArea;
		}
	}

	public FacingDirection Facing = FacingDirection.Right;
	private CameraMode CameraMode => Local.Pawn.Components.Get<CameraMode>();

	private Rect SpriteViewport;

	BBox box;

	public Color TintColor = Color.Transparent;
	public float TintAmount = 0;
	public SpriteSceneObject( SceneWorld SceneWorld, ModelSprite EntityParent ) : base( SceneWorld )
	{
		_parent = EntityParent;
		Init();


	}

	public void Init()
	{
		VertexBuffer = new();
		VertexBuffer.Init( true );

		SpriteViewport = _spriteArea.Area;
		SpriteViewport.width /= _spriteArea.XSubdivisions;
		SpriteViewport.height /= _spriteArea.YSubdivisions;
		Update();
		var spriteOffset = _spriteArea.SpriteOrigin * SpriteViewport.Size;
		spriteOffset.y = SpriteViewport.height;
		var quad = SpriteViewport.WithoutPosition + -spriteOffset;

		var pos = quad.Position * ((_parent.Scale / 2) + 0.5f);
		var size = quad.Size * ((_parent.Scale / 2) + 0.5f);

		var posy = -(_spriteArea.SpriteOrigin.y * SpriteViewport.Size.y);

		Vector3 v1 = new( pos.x, posy, pos.y );
		Vector3 v2 = new( pos.x + size.x, posy, pos.y );
		Vector3 v3 = new( pos.x + size.x, posy, pos.y + size.y );
		Vector3 v4 = new( pos.x, posy, pos.y + size.y );

		VertexBuffer.AddQuad( v1, v2, v3, v4 );
	}

	public override void RenderSceneObject()
	{
		if ( !_parent.IsValid() )
		{
			Delete();
			return;
		}
		if ( !_parent.EnableDrawing ) return;


		Render.Attributes.Set( "SpriteSheet", currentone.SpriteSheetTexture );
		var view = SpriteViewport;
		view.Position = SpriteViewport.Position + new Vector2( (FramexProgress) * SpriteViewport.Size.x, (FrameyProgress) * SpriteViewport.Size.y );
		//view.Size /= _spriteAsset.SpriteSheetTexture.Size;
		Render.Attributes.Set( "StartUV", view.Position / currentone.SpriteSheetTexture.Size );
		Render.Attributes.Set( "EndUV", (view.Position + view.Size) / currentone.SpriteSheetTexture.Size );
		Render.Attributes.Set( "Facing", Facing == FacingDirection.Right ? 1f : -1f );
		Render.Attributes.Set( "TintColor", (Vector3)TintColor );
		Render.Attributes.Set( "TintAmount", TintAmount );

		Render.Attributes.SetCombo( "D_HAS_NORMALS", currentone.SpriteSheetNormalTexture != null );
		Render.Attributes.Set( "SpriteSheetNormalMap", currentone.SpriteSheetNormalTexture );


		VertexBuffer.Draw( SpriteMaterial );


		//DebugView();
		base.RenderSceneObject();
	}
	static Rotation RotOffset => Rotation.From( 180, 90, 0 );

	internal void Update()
	{
		if ( _parent == null || !_parent.IsValid() )
		{
			Delete();
			return;
		}
		if ( _parent == null || _spriteArea == null )
			return;
		Transform = _parent.Transform;
		var spriteOffset = _spriteArea.SpriteOrigin * SpriteViewport.Size;
		//spriteOffset *= _parent.Scale;

		switch ( _parent.Tracking )
		{
			case ModelSprite.TrackingMode.Billboard:
				Rotation = Rotation.LookAt( CameraMode.Position - Local.Pawn.Position, Vector3.Up );
				break;
			case ModelSprite.TrackingMode.WorldSpace:
				break;
			case ModelSprite.TrackingMode.VerticalBillboard:
				Rotation = Rotation.LookAt( CameraMode.Position.WithZ( 0 ) - Local.Pawn.Position.WithZ( 0 ), Vector3.Up );
				break;
			case ModelSprite.TrackingMode.RotatingBillboard:
				Rotation = Rotation.LookAt( CameraMode.Position - Position, Vector3.Up );
				break;
		}
		Rotation *= RotOffset;
		Vector3 offsetPos = Rotation * new Vector3( spriteOffset.x, spriteOffset.y, spriteOffset.y );

		box = new BBox( new Vector3( -SpriteViewport.Size.x, -SpriteViewport.Size.x, 0 ), new Vector3( SpriteViewport.Size.x, SpriteViewport.Size.x, SpriteViewport.Size.y ) );
		box += offsetPos;
		box *= _parent.Scale;
		Bounds = box + (Position);

		UpdateAnimation();
	}
	public int Frame;

	private float FramexProgress;
	private float FrameyProgress;
	private RealTimeSince LastFrameSwitch;

	private void UpdateAnimation()
	{
		if ( _spriteArea.XSubdivisions == 1 && _spriteArea.YSubdivisions == 1 )
		{
			return;
		}
		if ( LastFrameSwitch > 1f / _spriteArea.FrameRate )
		{
			Frame++;
			LastFrameSwitch = 0;
			//SpriteViewport.Position = SpriteViewport.Position.WithX( SpriteViewport.width * (Frame % _spriteArea.XSubdivisions) ).WithY( SpriteViewport.height * (Frame / _spriteArea.YSubdivisions) );
			if ( _spriteArea.YSubdivisions == 1 )
			{
				FramexProgress = Frame;
				if ( FramexProgress == _spriteArea.XSubdivisions )
				{
					_parent?.AnimationFinished();
					Frame = 0;
					FrameyProgress = 0;
					FramexProgress = 0;
				}
			}
			else
			{
				FramexProgress = Frame % _spriteArea.XSubdivisions;
				if ( FramexProgress == 0 )
				{
					FrameyProgress++;
				}
				if ( FrameyProgress == _spriteArea.YSubdivisions )
				{
					_parent?.AnimationFinished();
					Frame = 0;
					FrameyProgress = 0;
					FramexProgress = 0;
				}

			}

		}
	}

	internal void DebugView()
	{
		var spriteOffset = _spriteArea.SpriteOrigin * SpriteViewport.Size;
		Vector3 offsetPos = Rotation * new Vector3( spriteOffset.x, 0, -spriteOffset.y );
		offsetPos *= _parent.Scale;
		var bb = box + (Position - offsetPos);
		DebugOverlay.Box( bb.Mins, bb.Maxs, Color.Red, 0, false );
		DebugOverlay.Sphere( bb.Center, 10f, Color.Red );
		DebugOverlay.Axis( bb.Center, Transform.Rotation, 10f );
	}

}
