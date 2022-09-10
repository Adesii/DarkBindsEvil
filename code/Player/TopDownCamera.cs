namespace DarkBinds.Player;

public class TopDownCamera : CameraMode
{
	public float CameraHeight => 600;//20000f; //600.0f;
	public override void Activated()
	{
		base.Activated();
		FieldOfView = 40f;

	}
	public override void Update()
	{
		Position = Entity.Position + Entity.Rotation * (Vector3.Up * CameraHeight - Vector3.Forward * CameraHeight / 4);
		Rotation = Rotation.LookAt( Entity.Position - Position );
		//FieldOfView = 1;
		//Ortho = false;
		//OrthoSize = 1f;
		//ZNear = 15000f;
		//ZFar = 30000f;

	}
	public override void Build( ref CameraSetup camSetup )
	{
		base.Build( ref camSetup );

		FieldOfView = camSetup.FieldOfView;
		Ortho = camSetup.Ortho;
		OrthoSize = camSetup.OrthoSize;
		ZNear = camSetup.ZNear;
		ZFar = camSetup.ZFar;
		ViewModelZNear = camSetup.ViewModel.ZNear;
		ViewModelZFar = camSetup.ViewModel.ZFar;
	}

}
