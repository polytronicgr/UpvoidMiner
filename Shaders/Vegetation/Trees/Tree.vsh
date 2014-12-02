#version 140

#pragma ACGLimport  <Common/Camera.csh>

uniform mat4 uModelMatrix;

uniform float uDiscardBias = 0.5;

in vec3 aPosition;
in vec3 aNormal;
in vec3 aTangent;
in vec2 aTexCoord;

out vec3 vNormal;
out vec3 vWorldPos;
out vec2 vTexCoord;
out float vDisc;

vec3 windOffset(float height, vec3 pos)
{
    float ws = cos(uRuntime + pos.x + pos.y + pos.z);
    return vec3(ws * cos(pos.x + pos.z), 0, ws * cos(pos.y + pos.z)) * max(0, height) * .1;
}

void main()
{
    // world space normal:
    vNormal = mat3(uModelMatrix) * aNormal;
    vTexCoord = aTexCoord;
    // world space position:
    vec4 worldPos = uModelMatrix * vec4(aPosition, 1.0);
    worldPos.xyz += windOffset(0.5*length(aPosition.xz)*clamp(aPosition.y-0.2, 0, 1), worldPos.xyz/6);
    vWorldPos = worldPos.xyz;

    // disc
    float disc = uDiscardBias;
    disc = distance(vWorldPos, uCameraPosition);
    disc = 0.901-clamp(disc/100,0,0.9);
    vDisc = disc;
    
    // projected vertex position used for the interpolation
    gl_Position  = uViewProjectionMatrix * worldPos;
}

