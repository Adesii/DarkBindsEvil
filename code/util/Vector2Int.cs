
using System;
using System.IO;
using Sandbox;

public struct Vector2Int : IEquatable<Vector2Int>
{
	internal System.Numerics.Vector2 vec;

	//
	// Summary:
	//     Returns a Vector with every component set to 1
	public static readonly Vector2Int One = new Vector2Int( 1, 1 );

	//
	// Summary:
	//     Returns a Vector with every component set to 0
	public static readonly Vector2Int Zero = new Vector2Int( 0, 0 );

	public static readonly Vector2Int Up = new Vector2Int( 0, 1 );

	public static readonly Vector2Int Down = new Vector2Int( 0, -1 );

	public static readonly Vector2Int Left = new Vector2Int( 1, 0 );

	public static readonly Vector2Int Right = new Vector2Int( -1f, 0 );

	public int x
	{
		readonly get
		{
			return vec.X.FloorToInt();
		}
		set
		{
			vec.X = value;
		}
	}

	public int y
	{
		readonly get
		{
			return vec.Y.FloorToInt();
		}
		set
		{
			vec.Y = value;
		}
	}

	//
	// Summary:
	//     Returns the magnitude of the vector
	public readonly float Length => vec.Length();

	//
	// Summary:
	//     This is faster than Length, so is better to use in certain circumstances
	public readonly float LengthSquared => vec.LengthSquared();

	//
	// Summary:
	//     returns true if the squared length is less than 1e-8 (which is really near zero)
	public readonly bool IsNearZeroLength => (double)LengthSquared <= 1E-08;

	//
	// Summary:
	//     Return the same vector but with a length of one
	public readonly Vector2Int Normal => System.Numerics.Vector2.Normalize( vec );

	public static Vector2Int Random => new Vector2Int( Game.Random.Float( -1f, 1f ), Game.Random.Float( -1f, 1f ) ).Normal * Game.Random.Float( 0f, 1f );

	public Vector2Int( int x )
		: this( x, x )
	{
	}

	public Vector2Int( int x, int y )
		: this( new System.Numerics.Vector2( x, y ) )
	{
	}
	public Vector2Int( float x, float y )
		: this( new System.Numerics.Vector2( x.FloorToInt(), y.FloorToInt() ) )
	{
	}

	public Vector2Int( Vector3 v )
		: this( new System.Numerics.Vector2( v.x, v.y ) )
	{
	}

	internal static float Distance( in Vector2Int a, in Vector2Int b )
	{
		return System.Numerics.Vector2.Distance( a.vec, b.vec );
	}

	public Vector2Int( System.Numerics.Vector2 v )
	{
		vec = v;
	}

	public static implicit operator Vector2Int( System.Numerics.Vector2 value )
	{
		return new Vector2Int( value );
	}

	public static implicit operator Vector2Int( Vector3 value )
	{
		return new Vector2Int( value );
	}


	public static implicit operator Vector4( Vector2Int value )
	{
		return new Vector4( value.x, value.y, 0f, 0f );
	}

	public static implicit operator Vector2Int( double value )
	{
		return new Vector2Int( (float)value, (float)value );
	}
	public static implicit operator Vector3( Vector2Int value )
	{
		return new Vector3( value.x, value.y, 0f );
	}

	public static Vector2Int operator +( Vector2Int c1, Vector2Int c2 )
	{
		return c1.vec + c2.vec;
	}
	public static Vector3 operator +( Vector3 c1, Vector2Int c2 )
	{
		return new Vector3( c1.x + c2.vec.X, c1.y + c2.vec.Y, c1.z );
	}

	public static Vector2Int operator -( Vector2Int c1, Vector2Int c2 )
	{
		return c1.vec - c2.vec;
	}

	public static Vector2Int operator -( Vector2Int c1 )
	{
		return System.Numerics.Vector2.Negate( c1.vec );
	}

	public static Vector2Int operator *( Vector2Int c1, float f )
	{
		return c1.vec * f;
	}

	public static Vector2Int operator *( float f, Vector2Int c1 )
	{
		return c1.vec * f;
	}

	public static Vector2Int operator *( Vector2Int c1, Vector2Int c2 )
	{
		return c1.vec * c2.vec;
	}
	public static Vector2Int operator %( Vector2Int c1, int c2 )
	{
		return new( c1.x % c2, c1.y % c2 );
	}

	public static Vector2Int operator /( Vector2Int c1, Vector2Int c2 )
	{
		return c1.vec / c2.vec;
	}

	public static Vector2Int operator /( Vector2Int c1, float c2 )
	{
		return c1.vec / c2;
	}

	//
	// Summary:
	//     TODO: Is this useful?
	public static Vector2Int FromRadian( float radian )
	{
		return new Vector2Int( MathF.Cos( radian ), MathF.Sin( radian ) );
	}

	public readonly float Distance( Vector2Int target )
	{
		return DistanceBetween( this, target );
	}

	public static float GetDistance( Vector2Int a, Vector2Int b )
	{
		return (b - a).Length;
	}

	public static float DistanceBetween( Vector2Int a, Vector2Int b )
	{
		return (b - a).Length;
	}

	public static double GetDot( Vector2Int a, Vector2Int b )
	{
		return a.x * b.x + a.y * b.y;
	}

	public override string ToString()
	{
		return $"{x:0.###},{y:0.###}";
	}

	//
	// Summary:
	//     Given a string, try to convert this into a vector4. The format is "x,y,z,w".
	public static Vector2Int Parse( string str )
	{
		str = str.Trim( '[', ']', ' ', '\n', '\r', '\t', '"' );
		string[] array = str.Split( new char[5] { ' ', ',', ';', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries );
		if ( array.Length != 2 )
		{
			return default( Vector2Int );
		}
		return new Vector2Int( array[0].ToInt(), array[1].ToInt() );
	}

	//
	// Summary:
	//     Snap To Grid along all 3 axes
	public readonly Vector2Int SnapToGrid( float gridSize, bool sx = true, bool sy = true )
	{
		return new Vector2Int( sx ? ((float)x).SnapToGrid( gridSize ) : x, sy ? ((float)y).SnapToGrid( gridSize ) : y );
	}

	//
	// Summary:
	//     Return this vector with x
	public readonly Vector2Int WithX( int x )
	{
		return new Vector2Int( x, y );
	}

	//
	// Summary:
	//     Return this vector with y
	public readonly Vector2Int WithY( int y )
	{
		return new Vector2Int( x, y );
	}

	//
	// Summary:
	//     Linearly interpolate from point a to point b
	public static Vector2Int Lerp( Vector2Int a, Vector2Int b, float t, bool clamp = true )
	{
		if ( clamp )
		{
			t = t.Clamp( 0f, 1f );
		}
		return System.Numerics.Vector2.Lerp( a.vec, b.vec, t );
	}

	public void Write( BinaryWriter writer )
	{
		writer.Write( x );
		writer.Write( y );
	}

	public static Vector2Int Read( BinaryReader reader )
	{
		return new Vector2Int( reader.ReadInt32(), reader.ReadInt32() );
	}

	public static bool operator ==( Vector2Int left, Vector2Int right )
	{
		return left.Equals( right );
	}

	public static bool operator !=( Vector2Int left, Vector2Int right )
	{
		return !(left == right);
	}

	public override bool Equals( object obj )
	{
		if ( obj is Vector2Int )
		{
			Vector2Int o = (Vector2Int)obj;
			return Equals( o );
		}
		return false;
	}

	public bool Equals( Vector2Int o )
	{
		return vec == o.vec;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine( vec );
	}
}



