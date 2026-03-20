public static class DeterministicRandom
{
    private static uint HashString(string seed)
    {
        uint hash = 2166136261u; // FNV-1a ½ÃÀÛ°ª

        for (int i = 0; i < seed.Length; i++)
        {
            hash ^= seed[i];
            hash *= 16777619u;
        }

        return hash;
    }

    public static float RandomFromIndex(int index, string seed)
    {
        uint seedHash = HashString(seed);

        uint x = (uint)index + seedHash;

        // ÇØ½Ã ¹Í½Ì
        x ^= x >> 16;
        x *= 0x7feb352d;
        x ^= x >> 15;
        x *= 0x846ca68b;
        x ^= x >> 16;

        return (x & 0xFFFFFF) / (float)0xFFFFFF;
    }
}
