namespace DarkBinds.Player;

public class SpinningCube : SceneModel
{
	Rotation RandRot;
	Rotation StartRot;
	TimeSince RotStart;
	public SpinningCube( SceneWorld sceneWorld, Transform transform ) : base( sceneWorld, "models/maya_testcube_100.vmdl", transform )
	{
		Event.Register( this );
		RandRot = Rotation.Random;
		RotStart = 0;
		StartRot = Rotation;
	}
	[Event.Client.Frame]
	public void UpdatePosition()
	{
		Rotation = Rotation.Lerp( StartRot, RandRot, RotStart );
		if ( Rotation.Forward.Angle( RandRot.Forward ) < 10f )
		{
			RandRot = Rotation.Random;
			RotStart = 0;
			StartRot = Rotation;
		}
	}
}
