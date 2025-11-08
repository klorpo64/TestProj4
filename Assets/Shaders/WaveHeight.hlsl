#define WAVE_COUNT 50

uniform float _Amplitudes[WAVE_COUNT];
uniform float2 _WaveDirections[WAVE_COUNT];
uniform float _WaveMagnitudes[WAVE_COUNT];
uniform float _Frequencies[WAVE_COUNT];

void calculateHeight_float(in float3 position, in float time, 
                           out float3 outPosition, out float3 normal)
{
    float3 displacedPos = position;
    float3 tangent = float3(1, 0, 0);
    float3 binormal = float3(0, 0, 1);

    for (int i = 0; i < WAVE_COUNT; i++)
    {
        float2 dir = _WaveDirections[i]; //shorthand bc too much to write
        float phase = dot(dir, position.xz) *
            _WaveMagnitudes[i] - _Frequencies[i] * time;
        float steepness = _Amplitudes[i] * _WaveMagnitudes[i];

        //idk if this is actually optimizing things
        float sinPhase = sin(phase);
        float cosPhase = cos(phase);
        
        displacedPos.xz -= dir * _Amplitudes[i] * sinPhase;
        displacedPos.y  += _Amplitudes[i] * cosPhase;

        tangent.x -= dir.x * dir.x * steepness * cosPhase;
        tangent.y += dir.x * steepness * sinPhase;
        tangent.z -= dir.x * dir.y * steepness * cosPhase;

        binormal.x -= dir.x * dir.y * steepness * cosPhase;
        binormal.y += dir.y * steepness * sinPhase;
        binormal.z -= dir.y * dir.y * steepness * cosPhase;
    }

    outPosition = displacedPos;
    normal = normalize(cross(binormal, tangent));
}
