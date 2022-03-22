namespace DarkBinds.Player;

public class TopDownCamera : CameraMode
{
	public float CameraHeight => 800.0f;
	public override void Activated()
	{
		base.Activated();

		OrthoSize = 0.5f;
		Ortho = false;
		FieldOfView = 40f;
	}
	public override void Update()
	{
		Position = Entity.Position + Entity.Rotation * (Vector3.Up * CameraHeight - Vector3.Forward * CameraHeight / 4);
		Rotation = Rotation.LookAt( Entity.Position - Position );

	}
}
