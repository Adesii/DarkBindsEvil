using System.Runtime.InteropServices;

namespace DarkBinds.Systems.Worlds;


public struct MapVertex
{
	public Vector3 Position;
	public Vector3 Normal;
	public Vector2 TexCoord0;
	public Vector4 Tangent;
	public Vector2 fBlendAmount;
	public Vector4 vUVData;

	public static readonly VertexAttribute[] Layout = new VertexAttribute[6]
		{
			new VertexAttribute( VertexAttributeType.Position, VertexAttributeFormat.Float32, 3 ),
			new VertexAttribute( VertexAttributeType.Normal, VertexAttributeFormat.Float32, 3 ),
			new VertexAttribute( VertexAttributeType.TexCoord, VertexAttributeFormat.Float32, 2, 0 ),
			new VertexAttribute( VertexAttributeType.Tangent, VertexAttributeFormat.Float32, 4 ),
			new VertexAttribute( VertexAttributeType.TexCoord, VertexAttributeFormat.Float32,2,17),
			new VertexAttribute( VertexAttributeType.TexCoord, VertexAttributeFormat.Float32,4,18),
		};

	public MapVertex( Vector3 position, Vector3 normal, Vector4 tangent, Vector2 texcoord, Vector2 fBlendAmount, Vector4 TexCoordData )
	{
		this = default( MapVertex );
		Position = position;
		Normal = normal;
		Tangent = tangent;
		TexCoord0 = texcoord;
		this.fBlendAmount.x = fBlendAmount.x.Clamp( 0f, 1f );
		this.fBlendAmount.y = fBlendAmount.y.Clamp( 0f, 1f );
		this.vUVData = TexCoordData;
	}

	public MapVertex( Vector3 position, Vector3 normal, Vector3 tangent, Vector2 texcoord, Vector2 fBlendAmount, Vector4 TexCoordData )
	{
		this = default( MapVertex );
		Position = position;
		Normal = normal;
		Tangent = new Vector4( tangent.x, tangent.y, tangent.z, -1f );
		TexCoord0 = texcoord;
		this.fBlendAmount.x = fBlendAmount.x.Clamp( 0f, 1f );
		this.fBlendAmount.y = fBlendAmount.y.Clamp( 0f, 1f );
		this.vUVData = TexCoordData;
	}
}
