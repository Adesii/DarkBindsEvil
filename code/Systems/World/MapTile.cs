using System.IO;
using System.Runtime.Serialization;
using Sandbox;

namespace DarkBinds.Systems.World;

public partial class MapTile
{
	public static int TileSize = 64;
	private Vector2Int _pos;
	public Vector2Int Position
	{
		get => _pos;
		set
		{
			_pos = value;
			WorldPosition = (new Vector3( Position.x * TileSize, Position.y * TileSize, 0 ) - (World.WorldSize * MapChunk.ViewSize / 2)).WithZ( 0 );

		}
	}
	public Vector3 WorldPosition { get; set; }
	public int BlockIndex { get; set; }

	internal void Deserialize( ref BinaryReader reader )
	{
		int x = reader.ReadInt32();
		int y = reader.ReadInt32();
		Position = new Vector2Int( x, y );
		BlockIndex = reader.ReadInt32();
	}

	internal void Serialize( ref BinaryWriter writer )
	{
		writer.Write( Position.x );
		writer.Write( Position.y );
		writer.Write( BlockIndex );
	}

	internal void DebugView()
	{
		if ( BlockIndex == 0 )
			return;
		Debug.Sphere( WorldPosition, TileSize / 2, Color.Red );
	}
}
