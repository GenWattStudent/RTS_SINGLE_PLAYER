string filename = "MyHeightMap";: This property represents the desired name for the generated height map texture file.

float perlinXScale; and float perlinYScale;: These properties control the scaling factors applied to the Perlin noise function in the X and Y directions, respectively. Adjusting these values will change the overall
"frequency" of the generated noise pattern.

int perlinOctaves;: This property determines the number of octaves used in the Perlin noise generation. Increasing the value will add more detail and complexity to the height map.

float perlinPersistance;: This property controls the persistence of the Perlin noise. It affects the amplitude of each subsequent octave in relation to the previous octave.
Higher values create more pronounced variations between different octaves.

float perlinHeightScale;: This property adjusts the overall height of the generated terrain. Increasing this value will result in a higher range of heights, while decreasing it will create flatter terrain.

int perlinOffsetX; and int perlinOffsetY;: These properties allow you to offset the Perlin noise function in the X and Y directions. Adjusting these values will shift the generated terrain horizontally and vertically.

bool alphaToggle = false;: This property represents a toggle that determines whether the alpha channel of the generated texture should be used. If enabled (true), the alpha channel will be set to the height values,
creating a grayscale image. If disabled (false), the alpha channel will be set to fully opaque.

bool seamlessToggle = false;: This property controls the generation of seamless terrain. If enabled (true), the Perlin noise function is evaluated at the corners of the texture and blended together,
ensuring a smooth transition across the borders of the height map.

bool mapToggle = false;: This property determines whether a grayscale map of the height values should be generated alongside the height map. If enabled (true),
the height values are mapped to a grayscale range of colors, allowing for a visual representation of the terrain.

float brightness = 0.5f; and float contrast = 0.5f;: These properties control the brightness and contrast adjustments applied to the generated height map.
Increasing the brightness value will make the terrain brighter, while increasing the contrast value will increase the difference between light and dark areas.


The script is a Unity editor window that generates and saves height map textures based on Perlin noise. It provides a user interface where various parameters can be adjusted to control
the characteristics of the generated terrain. The script allows customization of the Perlin noise scale, octaves, persistence, and height scale, enabling the creation of diverse and realistic landscapes.
Additionally, the script offers options to toggle the usage of the alpha channel, seamless terrain generation, and the generation of a grayscale map alongside the height map.
The brightness and contrast of the generated height map can also be adjusted. Once the desired settings are configured, the user can click the "Generate" button to create the height map and view it in the editor window.
The resulting height map can then be saved as a PNG file, with the option to overwrite existing files if desired.
In summary, this script provides a flexible and interactive tool for generating and saving height map textures, allowing for the creation of custom terrains within Unity.
You must have the folder "Assets/HeightMapGenerator/HeightMaps/" , otherwise the code doesn't work.