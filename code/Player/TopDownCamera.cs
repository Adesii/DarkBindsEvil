namespace DarkBinds.Player;

public class TopDownCamera : CameraMode
{
	public float CameraHeight => 600.0f;
	protected override void OnActivate()
	{
		Camera.FieldOfView = 40f;
	}
	public override void Build()
	{
		Camera.Position = Entity.Position + Entity.Rotation * (Vector3.Up * CameraHeight - Vector3.Forward * CameraHeight / 3);
		Camera.Rotation = Rotation.LookAt( Entity.Position - Camera.Position );

	}

}
