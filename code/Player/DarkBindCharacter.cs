using SpriteKit.Entities;
using SpriteKit.SceneObjects;

namespace DarkBinds.Player;

public class DarkBindCharacter : ModelSprite
{
	public float Speed => 300f;
	/// <summary>
	/// Provides an easy way to switch our current cameramode component
	/// </summary>
	public CameraMode CameraMode
	{
		get => Components.Get<CameraMode>();
		set => Components.Add( value );
	}
	public override void Spawn()
	{
		//SpritePath = "/sprites/mainchar/darkbind.sprite";
		SpritePath = "/sprites/mainchar/testing.sprite";
		CameraMode = new TopDownCamera();
		base.Spawn();
		Rotation = Rotation.LookAt( Vector3.Backward, Vector3.Up );

		//Components.Create<SoulPillar>();
		//Components.Create<SoulEater>();
	}

	public override void Simulate( Client cl )
	{
		base.Simulate( cl );
		Vector3 Vel = new();
		if ( Input.Down( InputButton.Forward ) )
		{
			Vel += Vector3.Forward;
		}
		if ( Input.Down( InputButton.Back ) )
		{
			Vel += Vector3.Backward;
		}
		if ( Input.Down( InputButton.Left ) )
		{
			Vel += Vector3.Left;
		}
		if ( Input.Down( InputButton.Right ) )
		{
			Vel += Vector3.Right;
		}
		if ( Input.Down( InputButton.Use ) )
		{
			Rotation *= Rotation.FromAxis( Vector3.Up, 100f * Time.Delta );
		}
		if ( Input.Down( InputButton.Menu ) )
		{
			Rotation *= Rotation.FromAxis( Vector3.Up, -100f * Time.Delta );
		}
		float boost = 1;
		if ( Input.Down( InputButton.Run ) )
		{
			boost *= 4;
		}
		var direction = Vel.y < 0;
		Vel = Rotation * Vel;
		if ( Vel.Length.AlmostEqual( 0f ) )
		{
			SetAnimation( "idle" );
		}
		else
		{
			SetAnimation( "walk" );
			if ( Vel.y != 0 )
				SetFacingDirection( direction );
		}
		Position += Vel.Normal * Speed * boost * Time.Delta;
	}

}
