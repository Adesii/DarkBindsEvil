using DarkBinds.Systems.Worlds;

namespace DarkBinds.util;
[SkipHotload]
public class WorldMovementHelper
{
	private Vector3 _position;
	private Vector3 _directionVector;
	private float _radius;

	public WorldMovementHelper( Vector3 Position, float Radius = 10f )
	{
		_position = Position;
		_radius = Radius;
	}

	//TODO: Fix being able to move into the wall and therefore being able to move through inside Corners stuff like X0 where X is a Wall, so moving from 0 to 0
	//																												0X
	public Vector3 Move( Vector3 Velocity )
	{
		var directionvector = Velocity;
		var transformedPos = _position + (Velocity.Normal * _radius);
		var CurrentPosition = World.GetTilePosition( _position );
		var NextPosition = World.GetTilePosition( transformedPos );
		var NextTile = World.GetMapTile( NextPosition );
		var diff = CurrentPosition - NextPosition;

		DebugOverlay.Line( _position, transformedPos, Color.Red );
		NextTile?.DebugView( true );
		if ( NextTile != null && !NextTile.Block.IsSolid() )
		{
			if ( diff.x != 0 && diff.y != 0 && ((World.GetMapTile( CurrentPosition + Vector2Int.Right * (Velocity.x > 0 ? -1 : 1) )?.Block.IsSolid() ?? false) && (World.GetMapTile( CurrentPosition + Vector2Int.Up * (Velocity.y > 0 ? 1 : -1) )?.Block.IsSolid() ?? false)) )
			{
				directionvector = directionvector.WithX( 0 ).WithY( 0 );
			}
			_position += directionvector;
		}
		else if ( NextTile != null && NextTile.Block.IsSolid() )
		{

			if ( diff.x != 0 && diff.y == 0 )
			{
				directionvector = directionvector.WithX( 0 );
			}
			else if ( diff.y != 0 && diff.x == 0 )
			{
				directionvector = directionvector.WithY( 0 );
			}
			else if ( diff.x != 0 && diff.y != 0 )
			{
				var horizontaltile = World.GetMapTile( CurrentPosition + Vector2Int.Right * (Velocity.x > 0 ? -1 : 1) );
				horizontaltile?.DebugView( true, Color.Green );
				if ( horizontaltile?.Block.IsSolid() ?? false )
					directionvector = directionvector.WithX( 0 );
				var VerticalTile = World.GetMapTile( CurrentPosition + Vector2Int.Up * (Velocity.y > 0 ? 1 : -1) );
				VerticalTile?.DebugView( true, Color.Yellow );
				if ( VerticalTile?.Block.IsSolid() ?? false )
					directionvector = directionvector.WithY( 0 );
			}

			_position += directionvector;

		}
		return _position;
	}
}
