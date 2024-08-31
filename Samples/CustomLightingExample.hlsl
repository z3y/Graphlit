
// override the function called at the end before the color is returned
// create a new implementation or call the default one to modify it
#define CUSTOM_COLOR
COLOR_FUNC // args: surf, fragData, giInput, giOutput
{
    half4 color = COLOR_DEFAULT;
    
    return color;
}


// override the default light function
// called once per light
#define CUSTOM_LIGHT
LIGHT_FUNC // args: light, fragData, giInput, surf, giOutput
{
    giOutput.directDiffuse += light.color * light.NoL * light.attenuation;
}
