using UnityEngine;

public static class NoiseGenerator  {

    public static float GetPerlinNoise (Vector2 position, float seed, float scale) 
    {
        float x = (position.x / 2 + seed) / scale;
        float z = (position.y / 2 + seed) / scale;
        return Mathf.PerlinNoise(x, z);
    }

    public static bool IsHereLode (Vector3 position, float offset, float scale, float frequency) 
    {
        // https://www.youtube.com/watch?v=Aga0TBJkchM on YouTube

        float x = (position.x + offset + 0.1f) * scale;
        float y = (position.y + offset + 0.1f) * scale;
        float z = (position.z + offset + 0.1f) * scale;

        float ab = Mathf.PerlinNoise(x, y);
        float bc = Mathf.PerlinNoise(y, z);
        float ac = Mathf.PerlinNoise(x, z);
        float ba = Mathf.PerlinNoise(y, x);
        float cb = Mathf.PerlinNoise(z, y);
        float ca = Mathf.PerlinNoise(z, x);

        return (ab + bc + ac + ba + cb + ca) / 6f <= frequency;
    }

}
