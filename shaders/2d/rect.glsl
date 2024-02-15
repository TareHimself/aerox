

//push constants block
layout( push_constant ) uniform constants
{
    vec4 clip;
	vec4 extent;
} pRect;

float mapRangeUnClamped(float value, float fromMin, float fromMax, float toMin, float toMax) {

    // Calculate the normalized position of the value in the input range
    float normalizedPosition = (value - fromMin) / (fromMax - fromMin);

    // Map the normalized position to the output range [toMin, toMax]
    return mix(toMin, toMax, normalizedPosition);
}

vec2 normalizePoint(vec4 viewport,vec2 point){
    return vec2(mapRangeUnClamped(point.x,0.0,viewport.z,-1.0,1.0),mapRangeUnClamped(point.y,0.0,viewport.w,-1.0,1.0));
}

bool shouldDiscard(vec4 viewport,vec4 clip,vec2 pixel){

    vec4 clip_ss = vec4(normalizePoint(viewport,clip.xy),normalizePoint(viewport,clip.xy + clip.zw));
    vec2 pixel_ss = normalizePoint(viewport,pixel);
    return pixel_ss.x > clip_ss.z || pixel_ss.x < clip_ss.x || pixel_ss.y < clip_ss.y || pixel_ss.y > clip_ss.w;
}