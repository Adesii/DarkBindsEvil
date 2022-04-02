using DarkBinds.Systems.Worlds;

namespace DarkBinds.util;
[SkipHotload]
public class WorldMovementHelper
{
	private Vector3 _position;
	private Vector3 _directionVector;
	private Vector3 _radius;

	public WorldMovementHelper( Vector3 Position, Vector3 Size )
	{
		_position = Position;
		_radius = Size;
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
		if ( NextTile != null && !NextTile.Block.HasCollisions() && NextTile.FloorBlock.HasCollisions() )
		{
			if ( diff.x != 0 && diff.y != 0 )
			{
				var horizontaltile = World.GetMapTile( CurrentPosition + Vector2Int.Right * (Velocity.x > 0 ? -1 : 1) );
				horizontaltile?.DebugView( true, Color.Green );
				if ( horizontaltile?.Block.HasCollisions() ?? false )
					directionvector = directionvector.WithX( 0 );
				if ( !horizontaltile?.FloorBlock.HasCollisions() ?? false )
				{
					directionvector = directionvector.WithX( 0 );
				}
				var VerticalTile = World.GetMapTile( CurrentPosition + Vector2Int.Up * (Velocity.y > 0 ? 1 : -1) );
				VerticalTile?.DebugView( true, Color.Yellow );
				if ( VerticalTile?.Block.HasCollisions() ?? false && (VerticalTile?.FloorBlock.HasCollisions() ?? false) )
					directionvector = directionvector.WithY( 0 );
				if ( !VerticalTile?.FloorBlock.HasCollisions() ?? false )
				{
					directionvector = directionvector.WithY( 0 );
				}

			}

			_position += directionvector;
		}
		else if ( NextTile != null && (NextTile.Block.HasCollisions() || !NextTile.FloorBlock.HasCollisions()) )
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
				if ( horizontaltile?.Block.HasCollisions() ?? false )
					directionvector = directionvector.WithX( 0 );
				if ( !horizontaltile?.FloorBlock.HasCollisions() ?? false )
				{
					directionvector = directionvector.WithX( 0 );
				}
				var VerticalTile = World.GetMapTile( CurrentPosition + Vector2Int.Up * (Velocity.y > 0 ? 1 : -1) );
				VerticalTile?.DebugView( true, Color.Yellow );
				if ( VerticalTile?.Block.HasCollisions() ?? false && (VerticalTile?.FloorBlock.HasCollisions() ?? false) )
					directionvector = directionvector.WithY( 0 );
				if ( !VerticalTile?.FloorBlock.HasCollisions() ?? false )
				{
					directionvector = directionvector.WithY( 0 );
				}

			}

			_position += directionvector;

		}
		return _position;
	}
}
