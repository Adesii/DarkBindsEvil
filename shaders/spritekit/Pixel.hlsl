#ifndef COMMON_PIXEL_H
#define COMMON_PIXEL_H


//Includes -----------------------------------------------------------------------------------------------------------------------------------------------
SamplerState TextureFiltering < Filter( NEAREST ); MaxAniso( 8 ); >;


#ifndef S_ALPHA_TEST
    #define S_ALPHA_TEST 0
#endif

#ifndef S_TRANSLUCENT
    #define S_TRANSLUCENT 0
#endif


#include "common/pixel.material.minimal.hlsl"

#include "sbox_pixel.fxc"

//-----------------------------------------------------------------------------
//
// Compose the final color with lighting from material parameters
//
//-----------------------------------------------------------------------------

PixelOutput FinalizePixelMaterial( PixelInput i, Material m )
{
    CombinerInput o = MaterialToCombinerInput( i, m );
    
    return FinalizePixel( o );
}


#endif // COMMON_PIXEL_H