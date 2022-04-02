using DarkBinds.Systems.Worlds;

namespace DarkBinds.Systems.Blocks;

[Block( "Water" )]
public class Water : BaseBlock
{
	public override bool IsSolid()
	{
		return true;
	}
	public override bool IsTranslucent()
	{
		return true;
	}
	public override bool HasCollisions()
	{
		return false;
	}

	protected override void AddWall( int TileSize, Vector3 right, Vector3 left, Vector2 Blends )
	{
		var topleft = left + Vector3.Up * (World.TileHeight - 16);
		var topright = right + Vector3.Up * (World.TileHeight - 16);

		var uv0 = new Vector2( 0, 0 );
		var uv1 = new Vector2( 1, 0 );
		var uv2 = new Vector2( 0, 1 );
		var uv3 = new Vector2( 1, 1 );

		Vector3 v0 = topleft;
		Vector3 v1 = topright;
		Vector3 v2 = left;
		Vector3 v3 = right;


		ComputeTriangleNormalAndTangent( out var outnormal, out var outtangent, v0, v1, v2, uv0, uv1, uv2 );
		Vector3 Axis;
		//Get Closest Axis x forward y right z up
		if ( Math.Abs( outnormal.x ) > Math.Abs( outnormal.y ) && Math.Abs( outnormal.x ) > Math.Abs( outnormal.z ) )
			Axis = Vector3.Right;
		else if ( Math.Abs( outnormal.y ) > Math.Abs( outnormal.z ) )
			Axis = Vector3.Forward;
		else
			Axis = Vector3.Up;

		Vector4 uvcoords = GetUVCoords( Axis );
		MapVertex[] arr = new MapVertex[]{
					new MapVertex(v0,outnormal, outtangent,uv0,Blends,uvcoords),
					new MapVertex(v1,outnormal, outtangent,uv1,Blends,uvcoords),
					new MapVertex(v2,outnormal, outtangent,uv2,Blends,uvcoords),
					new MapVertex(v3,outnormal, outtangent,uv3,Blends,uvcoords),
				};
		vertices.AddRange( arr );
		int i = vertices.Count;
		indices.AddRange( new List<int>(){
				i - 4,i - 3,i - 1,
				i - 4,i - 1,i - 2
		} );
	}

	protected override void AddCeiiling( int HalfTileSize, Vector3 TileWorldPosition )
	{
		var v0 = TileWorldPosition + (Vector3.Forward + Vector3.Left) * HalfTileSize;
		var v1 = TileWorldPosition + (Vector3.Forward + Vector3.Right) * HalfTileSize;
		var v2 = TileWorldPosition + (Vector3.Backward + Vector3.Left) * HalfTileSize;
		var v3 = TileWorldPosition + (Vector3.Right + Vector3.Backward) * HalfTileSize;

		var uv0 = new Vector2( 0, 0 );
		var uv1 = new Vector2( 1, 0 );
		var uv2 = new Vector2( 0, 0.5f );
		var uv3 = new Vector2( 1, 0.5f );

		Vector2 VertBlends = new Vector2( 0, 0 );
		Vector4 Texcord0 = GetUVCoords( Vector3.Up );

		MapVertex[] arr = new MapVertex[]{
					new MapVertex(v0   + Vector3.Up * (World.TileHeight-16),Vector3.Up, Vector3.Right, uv0,VertBlends,Texcord0),
					new MapVertex(v1   + Vector3.Up * (World.TileHeight-16),Vector3.Up, Vector3.Right, uv1,VertBlends,Texcord0),
					new MapVertex(v2   + Vector3.Up * (World.TileHeight-16),Vector3.Up, Vector3.Right, uv2,VertBlends,Texcord0),
					new MapVertex(v3   + Vector3.Up * (World.TileHeight-16),Vector3.Up, Vector3.Right, uv3,VertBlends,Texcord0)
				};
		vertices.AddRange( arr );
		int i = vertices.Count;
		indices.AddRange( new List<int>(){
					i - 1,i - 3,i - 4,
					i - 2,i - 1,i - 4
			} );
	}
}
