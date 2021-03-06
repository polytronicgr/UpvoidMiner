#version 140

#pragma ACGLimport <Common/Camera.csh>

in vec3 aPosition;
in vec3 aNormal;
in vec3 aTangent;
in vec2 aTexCoord;

out vec3 vNormal;
out vec3 vTangent;
out vec2 vTexCoord;
out vec4 vColor;
out float vLife;
out vec3 vWorldPos;
out vec3 vEyePos;
out vec4 vScreenPos;

uniform mat4 uModelMatrix;

uniform int uStrideSize;
uniform samplerBuffer uBuffer0;
uniform samplerBuffer uBuffer1;
uniform float uInterpolationFactor;

uniform float uConstrainRotationToY = 0;

const int offsets[] = int[]
(
   0, // position vec3
   3, // velocity vec3
   6, // size float
   7, // aCurrentLifetime float
   8, // aMaxLifetime float
   9, // tangent
   12 // angle float
);

vec2 linearInterpolationFactors = vec2(1.0 - uInterpolationFactor, uInterpolationFactor);

int getParticleOffset()
{
   return gl_InstanceID * uStrideSize;
}

float interpolateFloat(int offset)
{
   int currentOffset = getParticleOffset() + offset;

   float float0 = texelFetch(uBuffer0, currentOffset).x;
   float float1 = texelFetch(uBuffer1, currentOffset).x;

   return linearInterpolationFactors.x * float0 +
          linearInterpolationFactors.y * float1;
}

vec2 interpolateVec2(int offset)
{
    float floatX = interpolateFloat(offset);
    float floatY = interpolateFloat(offset+1);

    return vec2(floatX, floatY);
}

vec3 interpolateVec3(int offset)
{
    float floatX = interpolateFloat(offset);
    float floatY = interpolateFloat(offset+1);
    float floatZ = interpolateFloat(offset+2);

    return vec3(floatX, floatY, floatZ);
}

vec4 interpolateVec4(int offset)
{
    float floatX = interpolateFloat(offset);
    float floatY = interpolateFloat(offset+1);
    float floatZ = interpolateFloat(offset+2);
    float floatW = interpolateFloat(offset+3);

    return vec4(floatX, floatY, floatZ, floatW);
}

void main()
{
   vec3 iPosition = interpolateVec3(offsets[0]);
   //vec3 iVelocity = interpolateVec3(offsets[1]);
   float iSize = interpolateFloat(offsets[2]);
   float iCurLife = interpolateFloat(offsets[3]);
   float iMaxLife = interpolateFloat(offsets[4]);
   vec3 iTangent = interpolateVec3(offsets[5]);
   float iAngle = interpolateFloat(offsets[6]);

   // Compute relative particle life, i.e. \in 0..1
   float iRelLife = iCurLife/iMaxLife;
   iSize *= smoothstep(1, 0.85, iRelLife);

   vLife = iRelLife;

   // Pass attributes
   vTexCoord = aTexCoord;
   vColor = vec4(1);


   if(uConstrainRotationToY > 0.5)
   {
         // vec3(cos(100*iAngle),0, sin(100*iAngle)); //
        vec3 biTangent = normalize((uInverseViewMatrix * vec4(0,0,-1,0)).xyz);
        vec3 normal = vec3(0,1,0);
        vec3 tangent = normalize(cross(biTangent, normal));
        biTangent = normalize(cross(normal, tangent));

        mat3 localModel = mat3(tangent, normal, biTangent);

        vec3 objectPos = iPosition + localModel * (iSize * aPosition);

        // world space position:
        vWorldPos = (uModelMatrix * (vec4(objectPos, 1))).xyz;

        //vWorldPos = (uModelMatrix * (vec4(iSize * aPosition + iPosition, 1))).xyz;

        vEyePos = (uViewMatrix * vec4(vWorldPos,1)).xyz;
   }
   else
   {
       vec2 tangent = vec2(
                cos(iAngle),
                sin(iAngle)
                );
       vec2 normal = vec2(
                -tangent.y,
                tangent.x
                );

       // let the quad face the camera
       vNormal = vec3(uInverseViewMatrix * vec4(0, 0, -1, 0));
       vTangent = vec3(1,0,0);

       // world space position:
       vec4 worldPos = uModelMatrix * vec4(iPosition, 1.0);
       vWorldPos = worldPos.xyz;
       vEyePos = (uViewMatrix * worldPos).xyz;
       vEyePos.xy += tangent * iSize * aPosition.x
             + normal * iSize * aPosition.y;
   }



   // projected vertex position used for the interpolation
   vec4 screenPos = uProjectionMatrix * vec4(vEyePos,1);
   vScreenPos = screenPos;//screenPos.xyz / screenPos.w;


   gl_Position  = screenPos;
}
