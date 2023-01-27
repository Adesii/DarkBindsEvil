HEADER
{
	DevShader = true;
	CompileTargets = ( IS_SM_50 && ( PC || VULKAN ) );
	Version = 1;
}

//-------------------------------------------------------------------------------------------------------------------------------------------------------------
MODES
{
	Default();
	VrForward();
}

//-------------------------------------------------------------------------------------------------------------------------------------------------------------
FEATURES
{
	#include "ui/features.hlsl"
}

//-------------------------------------------------------------------------------------------------------------------------------------------------------------
COMMON
{
	#include "ui/common.hlsl"
}
  
//-------------------------------------------------------------------------------------------------------------------------------------------------------------
VS
{
	#include "ui/vertex.hlsl"  
}

//-------------------------------------------------------------------------------------------------------------------------------------------------------------
PS
{
	#include "ui/pixel.hlsl"

	CreateTexture2D( g_tColor )< Attribute( "Texture" );SrgbRead( true );Filter(NEAREST);AddressU( WRAP );AddressV( WRAP );>;
	CreateTexture2D( g_tFrameBuffer ) < Attribute("FrameBufferCopyTexture"); SrgbRead( true ); Filter( NEAREST ); OutputFormat( DXT5 );AddressU(CLAMP);AddressV(CLAMP); >;


	CreateTexture2D( g_tQuantizeLut ) <Attribute( "Quantization" );Filter( NEAREST ); OutputFormat( DXT5 ); SrgbRead( true ); >;


	
	DynamicCombo( D_IS_QUANTIZED, 0..1, Sys( ALL ) );
	DynamicCombo( D_USE_FRAMEBUFFER, 0..1, Sys( ALL ) );

	float g_vGridSize < Attribute( "ScaleFactor" ); Default(32);>;
	float2 RealCameraPosition < Attribute( "Position" ); Default2(0,0);>;
	float2 SnappedCameraPosition < Attribute( "SnappedPosition" ); Default2(0,0);>;

	//DynamicCombo( D_IS_PIXELPERFECT, 0..1, Sys( ALL ) );
	RenderState( SrgbWriteEnable0, true );
	RenderState( ColorWriteEnable0, RGBA );
	RenderState( FillMode, SOLID );
	RenderState( CullMode, NONE );
	RenderState( DepthWriteEnable, false );

	PS_OUTPUT MainPs( PS_INPUT i )
	{
		PS_OUTPUT o;
		UI_CommonProcessing_Pre( i );
		float2 CameraPosition = RealCameraPosition;
		float2 SnappedPosition = SnappedCameraPosition;
		#if D_USE_FRAMEBUFFER

			float2 TextureSize =  TextureDimensions2D( g_tFrameBuffer, 0 );
			float2 vTexCoord = i.vTexCoord;
			vTexCoord = floor( vTexCoord * g_vGridSize) / g_vGridSize;
			float4 vImage = Tex2DLevel( g_tFrameBuffer,vTexCoord.xy , 0.0f );

		#else

			float2 vTexCoord = i.vTexCoord;
			float2 TextureSize =  TextureDimensions2D( g_tColor, 0 );
			float4 vImage = Tex2D( g_tColor,vTexCoord.xy );
		#endif

		#if D_IS_QUANTIZED == 1
			float greyscale = max(vImage.r, max(vImage.g, vImage.b));

			float quantized = Tex2DLevel( g_tQuantizeLut,float2(saturate(1-greyscale),0.2f) , 0.0f ).r;
			vImage *= quantized;


		#endif

		float3 vColor = vImage.rgb;//saturate(vQuantize.rgb).r;

		o.vColor.rgb = vColor.rgb* i.vColor.rgb;
		o.vColor.a = vImage.a * i.vColor.a;
		//o.vColor.rgb = 1;
		//o.vColor.a = 1;
		return UI_CommonProcessing_Post(i,o);
	}
}
