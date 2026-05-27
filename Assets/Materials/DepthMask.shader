Shader "Custom/DepthMask" {
    SubShader {
        Tags {"Queue" = "Geometry-1" } // Draw just before regular objects
        ColorMask 0                    // Don't draw any colors (Invisible)
        ZWrite On                      // DO write to the depth buffer (Acts solid)
        
        Pass {}
    }
}