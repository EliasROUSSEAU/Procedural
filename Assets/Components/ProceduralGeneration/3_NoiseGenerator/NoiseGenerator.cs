using Components.ProceduralGeneration;
using Cysharp.Threading.Tasks;
using Microsoft.Unity.VisualStudio.Editor;
using System.Threading;
using UnityEngine;
using VTools.Grid;
using VTools.ScriptableObjectDatabase;
using System;

[CreateAssetMenu(menuName = "Procedural Generation Method/Noise Generation")]
public class NoiseGenerator : ProceduralGenerationMethod
{
    [Header("Noise Settings")]
    public FastNoiseLite.NoiseType noiseType = FastNoiseLite.NoiseType.Perlin;
    [Range(0.01f, 0.1f)] public float frequency = 1f;
    [Range(0.5f, 1.5f)] public float amplitude = 1f;

    [Header("Fractal Settings")]
    public FastNoiseLite.FractalType fractalType = FastNoiseLite.FractalType.FBm;
    [Range(1, 5)] public int octaves = 3;
    [Range(1f, 3f)] public float lacunarity = 2f;
    [Range(0.5f, 1f)] public float persistence = 0.5f;

    [Header("Terrain Height Thresholds")]
    [Range(-1f, 1f)] public float waterHeight = 0.2f;
    [Range(-1f, 1f)] public float sandHeight = 0.3f;
    [Range(-1f, 1f)] public float grassHeight = 0.6f;
    [Range(-1f, 1f)] public float rockHeight = 1f;

    [Header("Tree  Settings")]
    public bool enableTrees = true;
    [Range(0.001f, 0.5f)] public float vegetationFrequency = 0.02f;
    [Range(-1f, 1f)] public float treeThreshold = 0.2f;
    [Range(0f, 1f)] public float treeDensity = 0.8f;
    [Range(0, 6)] public int treeMinDistance = 2;
    public string[] treeTemplateNames = new string[] { "Tree" };
    public bool verboseTreeDebug = true;

    [Header("Branches Settings")]
    public bool enableBranches = true;
    [Range(0.001f, 0.5f)] public float branchFrequency = 0.03f;
    [Range(-1f, 1f)] public float branchThreshold = 0.15f;
    [Range(0f, 1f)] public float branchDensity = 0.6f;
    [Range(0, 6)] public int branchMinDistance = 1;
    [Range(0, 6)] public int branchMaxWaterDistance = 2;
    public string[] branchTemplateNames = new string[] { "Branch" };
    public bool verboseBranchDebug = true;

    [Header("Gallais Settings")]
    public bool enableGallais = true;
    [Range(0.001f, 0.5f)] public float gallaiFrequency = 0.04f;
    [Range(-1f, 1f)] public float gallaiThreshold = 0.1f;
    [Range(0f, 1f)] public float gallaiDensity = 0.5f;
    [Range(0, 6)] public int gallaiMinDistance = 0;
    [Range(0, 6)] public int gallaiMaxDistance = 2;
    [Range(0, 8)] public int gallaiMinDistanceFromWater = 1;
    [Range(1, 8)] public int gallaiMaxDistanceFromWater = 4;
    public bool gallaiSpawnAnywhere = false;
    public string[] gallaiTemplateNames = new string[] { "Gallai" };
    public bool verboseGallaiDebug = true;

    public int seed = 1337;

    private int treesPlaced = 0;
    private int branchesPlaced = 0;
    private int gallaisPlaced = 0;

    private int grassCells = 0;
    private int treeNoisePassed = 0;
    private int treeDensityPassed = 0;
    private int treeOccupancyFailed = 0;

    protected override async UniTask ApplyGeneration(CancellationToken cancellationToken)
    {
        treesPlaced = 0;
        branchesPlaced = 0;
        gallaisPlaced = 0;
        grassCells = 0;
        treeNoisePassed = 0;
        treeDensityPassed = 0;
        treeOccupancyFailed = 0;

        var noise = new FastNoiseLite(seed);
        noise.SetNoiseType(noiseType);
        noise.SetFrequency(frequency);
        noise.SetFractalType(fractalType);
        noise.SetFractalOctaves(octaves);
        noise.SetFractalLacunarity(lacunarity);
        noise.SetFractalGain(persistence);

        var treeNoise = new FastNoiseLite(seed + 1);
        treeNoise.SetNoiseType(noiseType);
        treeNoise.SetFrequency(vegetationFrequency);
        treeNoise.SetFractalType(fractalType);
        treeNoise.SetFractalOctaves(Math.Max(1, octaves - 1));
        treeNoise.SetFractalLacunarity(lacunarity);
        treeNoise.SetFractalGain(persistence);

        var branchNoise = new FastNoiseLite(seed + 2);
        branchNoise.SetNoiseType(noiseType);
        branchNoise.SetFrequency(branchFrequency);
        branchNoise.SetFractalType(fractalType);
        branchNoise.SetFractalOctaves(Math.Max(1, octaves - 1));
        branchNoise.SetFractalLacunarity(lacunarity);
        branchNoise.SetFractalGain(persistence);

        var gallaiNoise = new FastNoiseLite(seed + 3);
        gallaiNoise.SetNoiseType(noiseType);
        gallaiNoise.SetFrequency(gallaiFrequency);
        gallaiNoise.SetFractalType(fractalType);
        gallaiNoise.SetFractalOctaves(Math.Max(1, octaves - 1));
        gallaiNoise.SetFractalLacunarity(lacunarity);
        gallaiNoise.SetFractalGain(persistence);

        TerrainType[,] terrainMap = new TerrainType[Grid.Width, Grid.Lenght];

        for (int x = 0; x < Grid.Width; x++)
        {
            for (int z = 0; z < Grid.Lenght; z++)
            {
                TerrainType type = GenerateTerrainTypeFromNoise((float)(noise.GetNoise(x * frequency, z * frequency) * amplitude));
                terrainMap[x, z] = type;
                PlaceTerrainObject(x, z, type);
                if (type == TerrainType.Grass) grassCells++;
            }
        }

        ComputeNoiseStats("TreeNoise", treeNoise, vegetationFrequency);
        ComputeNoiseStats("BranchNoise", branchNoise, branchFrequency);
        ComputeNoiseStats("GallaiNoise", gallaiNoise, gallaiFrequency);

        bool[,] occupied = new bool[Grid.Width, Grid.Lenght];
        bool[,] treeOccupancy = enableTrees ? new bool[Grid.Width, Grid.Lenght] : null;
        bool[,] branchOccupancy = enableBranches ? new bool[Grid.Width, Grid.Lenght] : null;
        bool[,] gallaiOccupancy = enableGallais ? new bool[Grid.Width, Grid.Lenght] : null;

        var prng = new System.Random(seed);

        for (int x = 0; x < Grid.Width; x++)
        {
            for (int z = 0; z < Grid.Lenght; z++)
            {
                TerrainType type = terrainMap[x, z];

                if (enableTrees)
                    TryPlaceTree(x, z, type, treeNoise, prng, treeOccupancy, occupied);

                if (enableBranches)
                    TryPlaceBranch(x, z, type, branchNoise, prng, branchOccupancy, occupied, terrainMap);

                if (enableGallais)
                    TryPlaceGallai(x, z, type, gallaiNoise, prng, gallaiOccupancy, occupied, terrainMap);
            }
        }

        Debug.Log($"[NoiseGenerator] Total arbres placés : {treesPlaced}");
        Debug.Log($"[NoiseGenerator] Total branches placées : {branchesPlaced}");
        Debug.Log($"[NoiseGenerator] Total gallais placés : {gallaisPlaced}");
        Debug.Log($"[NoiseGenerator] Grass cells : {grassCells} | treeNoisePassed : {treeNoisePassed} | treeDensityPassed : {treeDensityPassed} | treeOccupancyFailed : {treeOccupancyFailed}");

        if (treesPlaced == 0 && enableTrees)
            Debug.LogWarning("[NoiseGenerator] Aucun arbre placé — essaye de diminuer treeThreshold / augmenter vegetationFrequency ou treeDensity.");
        if (branchesPlaced == 0 && enableBranches)
            Debug.LogWarning("[NoiseGenerator] Aucune branch placée — essaye de diminuer branchThreshold / augmenter branchFrequency ou branchDensity.");
        if (gallaisPlaced == 0 && enableGallais)
            Debug.LogWarning("[NoiseGenerator] Aucun gallai placé — vérifie gallai thresholds/distance to water/density.");
    }

    private void ComputeNoiseStats(string name, FastNoiseLite noiseInstance, float freq)
    {
        float min = float.MaxValue;
        float max = float.MinValue;
        double sum = 0;
        long count = 0;
        int stepX = Math.Max(1, Grid.Width / 100);
        int stepZ = Math.Max(1, Grid.Lenght / 100);
        for (int x = 0; x < Grid.Width; x += stepX)
        {
            for (int z = 0; z < Grid.Lenght; z += stepZ)
            {
                float v = noiseInstance.GetNoise(x, z);
                if (v < min) min = v;
                if (v > max) max = v;
                sum += v;
                count++;
            }
        }
        float avg = count > 0 ? (float)(sum / count) : 0f;
        Debug.Log($"[NoiseStats] {name} samples={count} min={min:F3} max={max:F3} avg={avg:F3} (thresholds must be in [-1,1])");
    }

    private TerrainType GenerateTerrainTypeFromNoise(float _noiseValue)
    {
        if (_noiseValue < waterHeight)
            return TerrainType.Water;
        else if (_noiseValue < sandHeight)
            return TerrainType.Sand;
        else if (_noiseValue < grassHeight)
            return TerrainType.Grass;
        else
            return TerrainType.Rock;
    }

    private void PlaceTerrainObject(int x, int z, TerrainType type)
    {
        Cell cell;
        if (!Grid.TryGetCellByCoordinates(x, z, out cell))
            return;
        GridObjectTemplate template = type switch
        {
            TerrainType.Water => ScriptableObjectDatabase.GetScriptableObject<GridObjectTemplate>("Water"),
            TerrainType.Sand => ScriptableObjectDatabase.GetScriptableObject<GridObjectTemplate>("Sand"),
            TerrainType.Grass => ScriptableObjectDatabase.GetScriptableObject<GridObjectTemplate>("Grass"),
            TerrainType.Rock => ScriptableObjectDatabase.GetScriptableObject<GridObjectTemplate>("Rock"),
            _ => null
        };
        if (template != null)
        {
            GridGenerator.AddGridObjectToCell(cell, template, true);
        }
    }

    private void TryPlaceTree(int x, int z, TerrainType type, FastNoiseLite treeNoise, System.Random prng, bool[,] treeOccupancy, bool[,] occupied)
    {
        if (type != TerrainType.Grass) return;
        if (treeOccupancy == null) return;

        float n = treeNoise.GetNoise(x, z);
        if (n >= treeThreshold) treeNoisePassed++;
        else return;

        double roll = prng.NextDouble();
        if (roll <= treeDensity) treeDensityPassed++;
        else return;

        int minD = treeMinDistance;
        int startX = Math.Max(0, x - minD);
        int endX = Math.Min(Grid.Width - 1, x + minD);
        int startZ = Math.Max(0, z - minD);
        int endZ = Math.Min(Grid.Lenght - 1, z + minD);

        for (int ix = startX; ix <= endX; ix++)
        {
            for (int iz = startZ; iz <= endZ; iz++)
            {
                if (treeOccupancy[ix, iz]) return;
            }
        }

        GridObjectTemplate treeTemplate = null;
        if (treeTemplateNames != null && treeTemplateNames.Length > 0)
        {
            int idx = prng.Next(0, treeTemplateNames.Length);
            var name = treeTemplateNames[idx];
            treeTemplate = ScriptableObjectDatabase.GetScriptableObject<GridObjectTemplate>(name);
        }
        if (treeTemplate == null)
            treeTemplate = ScriptableObjectDatabase.GetScriptableObject<GridObjectTemplate>("Tree");

        object prefabVal = null;
        var prefabField = treeTemplate.GetType().GetField("prefab");
        if (prefabField != null) prefabVal = prefabField.GetValue(treeTemplate);
        var prefabProp = treeTemplate.GetType().GetProperty("Prefab");
        if (prefabVal == null && prefabProp != null) prefabVal = prefabProp.GetValue(treeTemplate, null);
        if (prefabVal == null) prefabVal = Resources.Load<GameObject>("FallbackTree");

        Cell cell;
        if (!Grid.TryGetCellByCoordinates(x, z, out cell)) return;
        bool spawned = false;
        try
        {
            VTools.Grid.GridObjectFactory.SpawnOnGridFrom(treeTemplate, cell, Grid, null, 0, null);
            spawned = true;
        }
        catch
        {
            try
            {
                GridGenerator.AddGridObjectToCell(cell, treeTemplate, false);
                spawned = true;
            }
            catch { }
        }

        if (spawned)
        {
            treeOccupancy[x, z] = true;
            occupied[x, z] = true;
            treesPlaced++;
        }
    }

    private void TryPlaceBranch(int x, int z, TerrainType type, FastNoiseLite branchNoise, System.Random prng, bool[,] branchOccupancy, bool[,] occupied, TerrainType[,] terrainMap)
    {
        if (branchOccupancy == null) return;

        if (type == TerrainType.Rock) return;

        float n = branchNoise.GetNoise(x, z);
        if (n < branchThreshold) return;

        double roll = prng.NextDouble();
        if (roll > branchDensity) return;

        if (occupied[x, z]) return;

        if (type == TerrainType.Water)
        {
            int nearestLand = NearestTerrainDistance(x, z, terrainMap, TerrainType.Grass, TerrainType.Sand, TerrainType.Rock);
            if (nearestLand > branchMaxWaterDistance) return;
        }

        int minD = branchMinDistance;
        int startX = Math.Max(0, x - minD);
        int endX = Math.Min(Grid.Width - 1, x + minD);
        int startZ = Math.Max(0, z - minD);
        int endZ = Math.Min(Grid.Lenght - 1, z + minD);

        for (int ix = startX; ix <= endX; ix++)
        {
            for (int iz = startZ; iz <= endZ; iz++)
            {
                if (branchOccupancy[ix, iz]) return;
            }
        }

        GridObjectTemplate branchTemplate = null;
        if (branchTemplateNames != null && branchTemplateNames.Length > 0)
        {
            int idx = prng.Next(0, branchTemplateNames.Length);
            var name = branchTemplateNames[idx];
            branchTemplate = ScriptableObjectDatabase.GetScriptableObject<GridObjectTemplate>(name);
        }
        if (branchTemplate == null) branchTemplate = ScriptableObjectDatabase.GetScriptableObject<GridObjectTemplate>("Branch");

        if (branchTemplate != null)
        {
            Cell cell;
            if (!Grid.TryGetCellByCoordinates(x, z, out cell)) return;
            try
            {
                VTools.Grid.GridObjectFactory.SpawnOnGridFrom(branchTemplate, cell, Grid, null, 0, null);
            }
            catch
            {
                GridGenerator.AddGridObjectToCell(cell, branchTemplate, false);
            }
            branchOccupancy[x, z] = true;
            occupied[x, z] = true;
            branchesPlaced++;
        }
    }


    private void TryPlaceGallai(int x, int z, TerrainType type, FastNoiseLite gallaiNoise, System.Random prng, bool[,] gallaiOccupancy, bool[,] occupied, TerrainType[,] terrainMap)
    {
        if (gallaiOccupancy == null) return;
        if (occupied[x, z]) return;

        float n = gallaiNoise.GetNoise(x, z);
        if (n < gallaiThreshold) return;

        double roll = prng.NextDouble();
        if (roll > gallaiDensity) return;

        if (!gallaiSpawnAnywhere)
        {
            if (type == TerrainType.Water) return;
            int distToWater = NearestTerrainDistance(x, z, terrainMap, TerrainType.Water);
            if (distToWater < gallaiMinDistanceFromWater || distToWater > gallaiMaxDistanceFromWater) return;
        }

        int minD = gallaiMinDistance;
        int startX = Math.Max(0, x - minD);
        int endX = Math.Min(Grid.Width - 1, x + minD);
        int startZ = Math.Max(0, z - minD);
        int endZ = Math.Min(Grid.Lenght - 1, z + minD);

        for (int ix = startX; ix <= endX; ix++)
        {
            for (int iz = startZ; iz <= endZ; iz++)
            {
                if (gallaiOccupancy[ix, iz]) return;
            }
        }

        GridObjectTemplate gallaiTemplate = null;
        if (gallaiTemplateNames != null && gallaiTemplateNames.Length > 0)
        {
            int idx = prng.Next(0, gallaiTemplateNames.Length);
            var name = gallaiTemplateNames[idx];
            gallaiTemplate = ScriptableObjectDatabase.GetScriptableObject<GridObjectTemplate>(name);
        }
        if (gallaiTemplate == null) gallaiTemplate = ScriptableObjectDatabase.GetScriptableObject<GridObjectTemplate>("Gallai");

        if (gallaiTemplate != null)
        {
            Cell cell;
            if (!Grid.TryGetCellByCoordinates(x, z, out cell)) return;
            try
            {
                VTools.Grid.GridObjectFactory.SpawnOnGridFrom(gallaiTemplate, cell, Grid, null, 0, null);
            }
            catch
            {
                GridGenerator.AddGridObjectToCell(cell, gallaiTemplate, false);
            }
            gallaiOccupancy[x, z] = true;
            occupied[x, z] = true;
            gallaisPlaced++;
        }
    }

    private int NearestTerrainDistance(int x, int z, TerrainType[,] terrainMap, params TerrainType[] targets)
    {
        int maxSearch = Math.Max(Grid.Width, Grid.Lenght);
        for (int d = 0; d <= maxSearch; d++)
        {
            int startX = Math.Max(0, x - d);
            int endX = Math.Min(Grid.Width - 1, x + d);
            int startZ = Math.Max(0, z - d);
            int endZ = Math.Min(Grid.Lenght - 1, z + d);
            int dd = d * d;
            for (int ix = startX; ix <= endX; ix++)
            {
                for (int iz = startZ; iz <= endZ; iz++)
                {
                    int dx = ix - x;
                    int dz = iz - z;
                    if (dx * dx + dz * dz > dd) continue;
                    foreach (var t in targets)
                    {
                        if (terrainMap[ix, iz] == t) return d;
                    }
                }
            }
        }
        return int.MaxValue;
    }

    private int NearestTerrainDistance(int x, int z, TerrainType[,] terrainMap, TerrainType singleTarget)
    {
        return NearestTerrainDistance(x, z, terrainMap, new TerrainType[] { singleTarget });
    }

    private enum TerrainType
    {
        Water,
        Sand,
        Grass,
        Rock
    }
}
