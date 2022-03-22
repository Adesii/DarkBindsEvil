using System.Collections.Generic;
using DarkBinds.Systems.World;
using Sandbox.UI;

namespace DarkBinds.UI;

public class ChunkPreview : Image
{
	private Texture PreviewTexture { get; set; }
	private bool dirty = true;

	public World curworld => DarkBindsGame.Instance.World;
	List<byte> chunkdata = new List<byte>();
	Vector2Int PrevPosition = new Vector2Int( 0, 0 );
	Color32 PrevColor = Color.White;
	List<byte> prevchunkdata = new List<byte>();
	public override void Tick()
	{
		base.Tick();
		if ( curworld == null ) return;
		if ( !curworld.ChunksFullyReceived )
		{
			dirty = true;
		}
		if ( curworld.Tiles == null || curworld.Tiles.Count == 0 || curworld.Tiles[0].Tiles.Count == 0 || !curworld.ChunksFullyReceived ) return;
		if ( PreviewTexture == null )
		{
			PreviewTexture = Texture.Create( World.WorldSize * MapChunk.ChunkSize, World.WorldSize * MapChunk.ChunkSize ).WithDynamicUsage().Finish();
		}
		if ( dirty )
		{
			Log.Info( curworld.Tiles.Count );
			Log.Info( "Chunks received" );
			Log.Info( $"Preview size: {PreviewTexture.Width}x{PreviewTexture.Height}" );
			chunkdata = new();

			Log.Info( World.GetMapTile( new Vector2Int( 112, 112 ), true ) );

			for ( int x = 0; x < PreviewTexture.Width; x++ )
			{

				for ( int y = 0; y < PreviewTexture.Height; y++ )
				{
					var tile = World.GetMapTile( new Vector2Int( x, y ) );
					if ( tile == null )
					{
						chunkdata.Add( 0 );
						chunkdata.Add( 0 );
						chunkdata.Add( 0 );
						chunkdata.Add( byte.MaxValue );
					}
					else
					{
						chunkdata.Add( (byte)tile.BlockIndex );
						chunkdata.Add( (byte)tile.BlockIndex );
						chunkdata.Add( (byte)tile.BlockIndex );
						chunkdata.Add( byte.MaxValue );
					}
				}
			}
			PreviewTexture.Update( chunkdata.ToArray(), 0, 0, PreviewTexture.Width, PreviewTexture.Height );
			dirty = false;
		}
		var block = World.GetMapTile( Local.Pawn.Position );
		if ( block == null ) return;
		var PlayerPos = block.Position;
		if ( PlayerPos != PrevPosition )
		{
			prevchunkdata = chunkdata.ToList();
			PrevPosition = PlayerPos;
			for ( int x = -2; x < 2; x++ )
			{
				for ( int y = -2; y < 2; y++ )
				{
					int pixelfrompositionindex = ((PlayerPos.x + x) * PreviewTexture.Width + (PlayerPos.y + y)) * 4;
					if ( pixelfrompositionindex < 0 || pixelfrompositionindex > chunkdata.Count )
					{
						continue;
					}
					for ( int i = 0; i < 4; i++ )
					{
						prevchunkdata[pixelfrompositionindex + i] = i % 2 == 0 ? byte.MaxValue : byte.MinValue;

					}
				}
			}



			PreviewTexture.Update( prevchunkdata.ToArray(), 0, 0, PreviewTexture.Width, PreviewTexture.Height );
		}

		Texture = PreviewTexture;
		Style.Width = PreviewTexture.Width * 2;
		Style.Height = PreviewTexture.Height * 2;
	}

}
