using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using System.Linq;
using Unity.Jobs;
using Unity.Collections;
using UnityEngine.Rendering;
using System.Threading;
using System;

namespace Voxel {
    /// <summary>
    /// A cube in the voxel space.
    /// </summary>
    //public class Cube {
    //    /// <summary>
    //    /// A list of floats containing values at the eight corners of the cube.
    //    /// </summary>
    //    public float[] data = new float[8];

    //    public float3 origin;

    //    public float3[] corners = new float3[8];


    //    public Cube(float[] data, float3 origin) {
    //        this.data = data;
    //        this.origin = origin;
    //        PopulateCorners();
    //    }

    //    public Cube(float3 origin) {
    //        this.origin = origin;
    //        PopulateCorners();
    //    }
    //}
    /// <summary>
    /// A collection of cubes
    /// </summary>
    public class Chunk {
        /// <summary>
        /// Size of a leaf chunk
        /// </summary>
        public const int CHUNK_SIZE = 32;
        /// <summary>
        /// Coordinate of the center of the chunk
        /// </summary>
        public float3 center;
        /// <summary>
        /// Data points within the chunk. xyz is the coordinate of the point. w is the data value at that point.
        /// </summary>
        public float4[] data = null;
        public NativeArray<float4>? PPData = null;

        public Mesh mesh;
        public Mesh parentPartialMesh;
        public GameObject meshGO;
        public bool partialParent;

        public int size;
        public bool active;
        public bool startedDisplay;
        public bool hasMesh, calculated;




        /// <summary>
        /// Initialize a chunk with specified center
        /// </summary>
        /// <param name="center">Center location of the chunk.</param>
        public Chunk(float3 center, int size) {
            this.center = center;
            this.size = size;

        }

        #region Noise
        private float GenerateData(float3 location) {
            float noiseScale = 10f;
            location = location + new float3(0.5f, 0.5f, 0.5f);
            VoxelWorld.WorldType worldType = VoxelWorld.worldType;
            float d = 2;
            switch (worldType) {

                case VoxelWorld.WorldType.Terrain:
                    //if () { //mountains
                    d = location.y - OctavePerlinNoise(location.xz / noiseScale / 10, 5, .5f) * 100;
                    //}
                    if (location.y < 0) {
                        //d = location.y - OctavePerlinNoise(location.xz / noiseScale / 10, 5, .3f)*8;
                        d = location.y - OctavePerlinNoise(location.xz / noiseScale / 10, 5, .5f) * 10;
                    }

                    break;

                case VoxelWorld.WorldType.Maze:
                    float height = 10f;
                    if (OctavePerlinNoise(location.xz / noiseScale, 5, .5f) < 0 || location.y < 0 || location.y > height)
                        d = -1;
                    break;

                case VoxelWorld.WorldType.Caves:
                    d = OctavePerlinNoise(location / noiseScale, 8, .5f);
                    break;

                case VoxelWorld.WorldType.Sponge:
                    if (location.y < 0) {
                        d = OctavePerlinNoise(location / noiseScale, 8, .5f);
                    }
                    break;

                default:
                    if (location.y < 0) d = -1;
                    break;
            }

            return d * 10;
        }

        private float OctavePerlinNoise(float3 location, int octaves, float persistence) {
            float total = 0;
            float frequency = 1;
            float amplitude = 1;
            float maxValue = 0;  // Used for normalizing result to 0.0 - 1.0
            for (int i = 0; i < octaves; i++) {
                total += Perlin.Noise(location.x * frequency, location.y * frequency, location.z * frequency) * amplitude;

                maxValue += amplitude;

                amplitude *= persistence;
                frequency *= 2;
            }

            return total / maxValue;
        }

        private float OctavePerlinNoise(float2 location, int octaves, float persistence) {
            float total = 0;
            float frequency = 1;
            float amplitude = 1;
            float maxValue = 0;  // Used for normalizing result to 0.0 - 1.0
            for (int i = 0; i < octaves; i++) {
                total += Perlin.Noise(location.x * frequency, location.y * frequency) * amplitude;

                maxValue += amplitude;

                amplitude *= persistence;
                frequency *= 2;
            }

            return total / maxValue;
        }

        private float OctavePerlinNoise(float location, int octaves, float persistence) {
            float total = 0;
            float frequency = 1;
            float amplitude = 1;
            float maxValue = 0;  // Used for normalizing result to 0.0 - 1.0
            for (int i = 0; i < octaves; i++) {
                total += Perlin.Noise(location * frequency) * amplitude;

                maxValue += amplitude;

                amplitude *= persistence;
                frequency *= 2;
            }

            return total / maxValue;
        }

        #endregion

        //public bool HasMesh() {
        //    //bool flag = data[0].w > 0;
        //    return true;
        //    NativeArray<float4> d = data.Value;
        //    for (int i = 0; i < d.Length; i++) {
        //        if (!(d[i].w > 0 && d[0].w > 0 || (d[i].w <= 0 && d[0].w <= 0))) {
        //            return true;
        //        }
        //    }
        //    return false;
        //}

    }



    public class Octree {
        public Octree[] children;
        public Octree parent;
        public bool isLeaf;
        public Chunk chunk;
        public Bounds bounds;
        //public int size;
        //public float3 center;
        public int level;

        public bool noiseCoroutineStarted = false;
        public bool startedCalculate;
        public bool scanned = false;


        public Octree(int size, Vector3 center) {
            //Debug.Log("Create new chunk." + (Chunk.size) + " at " + center);
            //this.size = size;
            //this.center = center;
            this.chunk = new Chunk(center, size);
            bounds = new Bounds(center, Vector3.one * size);
            
            if (size == Chunk.CHUNK_SIZE) isLeaf = true;
            if (isLeaf) {
                level = 0;
            } else {
                level = -1;
                this.children = new Octree[8];
            }
        }

        public Chunk GetChunkAt(float3 location, int level) {
            return GetNodeAt(location, level).chunk;
        }

        public Octree GetNodeAt(float3 location, int level) {
            float3 offset = location - chunk.center;
            //bitmap for octant indexing
            int index = (offset.x > 0 ? 1 : 0) | ((offset.y > 0 ? 1 : 0)) << 1 | ((offset.z > 0 ? 1 : 0) << 2);
            bool insideNode = InsideNode(location);

            //if offset within nodeSize
            if (insideNode && this.level >= level) {
                //if node is of interest
                if (this.level == level) {
                    //if (chunk != null) {
                    //    //Debug.Log("EXISTING CHUNK " + chunk.center);
                    //    return chunk;
                    //} else {
                    //    this.chunk = new Chunk(center);
                        return this;
                    //}
                } else {  //else node is not specified level
                    if (children[index] == null) { //child node does not exist 
                        //subdivide
                        Subdivide();
                    }
                    return children[index].GetNodeAt(location, level);
                }
            } else {
                //make new root
                float3 newCenter = chunk.center;
                newCenter.x += (offset.x > 0 ? 1 : -1) * (chunk.size / 2);
                newCenter.y += (offset.y > 0 ? 1 : -1) * (chunk.size / 2);
                newCenter.z += (offset.z > 0 ? 1 : -1) * (chunk.size / 2);
                Octree newRoot = new Octree(chunk.size * 2, newCenter);
                int idx;
                idx = ((offset.x < 0 ? 1 : 0) << 0) | ((offset.y < 0 ? 1 : 0) << 1) | ((offset.z < 0 ? 1 : 0) << 2);
                newRoot.level = this.level + 1;
                this.parent = newRoot;
                newRoot.children[idx] = this;
                newRoot.Subdivide();
                //return this;
                newRoot.isLeaf = false;
                return newRoot.GetNodeAt(location, level);
            }
        }

        public bool InsideNode(float3 location) {
            float r = chunk.size / 2;
            float3 offset = location - chunk.center; //calculate offset
            if (offset.x > r || offset.x < -r) return false;
            if (offset.y > r || offset.y < -r) return false;
            if (offset.z > r || offset.z < -r) return false;
            return true;
        }

        public Octree GetRoot() {
            if (parent != null) return parent.GetRoot();
            else return this;
        }


        /// <summary>
        /// Subdivde a node into 8 octants
        /// </summary>
        public void Subdivide() {
            if (children == null) {
                children = new Octree[8];
            }
            float3 localCenter;
            for (int i = 0; i < 8; i++) {
                localCenter.x = ((i & (1 << 0)) >> 0 == 0 ? -1 : 1);
                localCenter.y = ((i & (1 << 1)) >> 1 == 0 ? -1 : 1);
                localCenter.z = ((i & (1 << 2)) >> 2 == 0 ? -1 : 1);
                if (children[i] == null) {
                    children[i] = new Octree(chunk.size / 2, localCenter * chunk.size / 4 + chunk.center);
                    children[i].level = this.level - 1;
                    children[i].parent = this;
                }

            }
        }

        public bool IntersectSphere(Vector3 center, float r) {
            float r2 = r * r;
            for (int i = 0; i < 3; i++) {
                if (center[i] < bounds.min[i]) r2 -= Squared(center[i] - bounds.min[i]);
                else
                if (center[i] > bounds.max[i]) r2 -= Squared(center[i] - bounds.max[i]);
            }
            //Debug.Log(minCorner + " " + maxCorner + " " + center + " " + r + " " + (r2>0));
            return r2 > 0;
        }


        public float Squared(float x) {
            return x * x;
        }
    }

    /// <summary>
    /// Performs marching cube algorithm to generate mesh from point cloud data.
    /// </summary>
    public class MarchingCube {
        public static ComputeShader cshader;
        public static float isolevel = 0f;
        


        static int CHUNK_SIZE = Chunk.CHUNK_SIZE;
        static bool IS_SMOOTH = false;

        int[][] bufferCounts = new int[THREAD_MAX][];
        static readonly int THREAD_MAX = Environment.ProcessorCount;

        static ComputeBuffer[] trigBuffs = new ComputeBuffer[THREAD_MAX];
        static ComputeBuffer[] trigBuffs_PP = new ComputeBuffer[THREAD_MAX];
        static ComputeBuffer[] countBuffs = new ComputeBuffer[THREAD_MAX];
        static ComputeBuffer[] noiseBuffs = new ComputeBuffer[THREAD_MAX];
        ComputeShader perlin_shader = (ComputeShader)Resources.Load("MC/Perlin");
        ComputeShader mc_shader = (ComputeShader)Resources.Load("MC/MarchingCube");
        public CraterConfig craterConfig;
        static ComputeBuffer cratersBuffer;
        int MCkernel, PNkernel;


        public MarchingCube() {
            ThreadPool.SetMaxThreads(THREAD_MAX, THREAD_MAX);
            MCkernel = mc_shader.FindKernel("MCMain");
            PNkernel = perlin_shader.FindKernel("PerlinMain");
            for (int i = 0; i < THREAD_MAX; i++) {
                bufferCounts[i] = new int[] { 0, 0, 0, 0 };
                countBuffs[i] = new ComputeBuffer(4, sizeof(int), ComputeBufferType.IndirectArguments);
                countBuffs[i].SetData(bufferCounts[i]);

                trigBuffs[i] = new ComputeBuffer(CHUNK_SIZE * CHUNK_SIZE * CHUNK_SIZE * 12 * 4, 72, ComputeBufferType.Append);
                trigBuffs_PP[i] = new ComputeBuffer(CHUNK_SIZE * CHUNK_SIZE * CHUNK_SIZE * 6, 72, ComputeBufferType.Append);
                noiseBuffs[i] = new ComputeBuffer((CHUNK_SIZE + 1) * (CHUNK_SIZE + 1) * (CHUNK_SIZE + 1), 16);

                craterConfig = new CraterConfig { verticalScale = 2f };
            }

            cratersBuffer = new ComputeBuffer(500, 24);
            List<Crater> craterData = new List<Crater>();
            UnityEngine.Random.InitState(1234);
            for (int j = 0; j < 500; j++) {
                float r = UnityEngine.Random.Range(0.2f, 1f);
                craterData.Add(new Crater {
                    position = UnityEngine.Random.insideUnitCircle *500,
                    radius = math.clamp(BiasFunction(r, 0.92f), 0.01f, 1f) * 1000,
                    verticalScale = math.pow(BiasFunction(r, 0.7f) + 0.7f, 3) * 5,
                    rimSharpness = math.clamp((1-r), 0.2f, 1f),
                    rimThickness = 0.1f
                    //math.pow(BiasFunction(r, 0.7f) + 0.7f, 3)
                });
            }
            mc_shader.SetBuffer(PNkernel, "craters", cratersBuffer);
            cratersBuffer.SetData(craterData);


        }

        public float BiasFunction(float x, float bias) {
            float k = math.pow(1 - bias, 3);
            return (x * k) / (x * k - x + 1);
        }


        public void March(Octree node) {
            if (node.noiseCoroutineStarted) return;
            node.noiseCoroutineStarted = true;
            //ThreadPool.QueueUserWorkItem(MarchChunk, node);
            MarchChunk(node);
        }

        public Mesh[] MarchChunk(Octree node) {
            Mesh[] result = new Mesh[2];

            //Octree node = (Octree)state;
            //Debug.Log("MARCH: " + node.center);
            //Debug.Log("MARCH1: " + node.center);

            int id = Thread.CurrentThread.ManagedThreadId % THREAD_MAX;

            //Debug.Log("MARCH: " + id);

            //ComputeBuffer noiseDataBuffer = noiseBuffs[id];
            ComputeBuffer trianglesBuffer = trigBuffs[id];
            ComputeBuffer PPTrianglesBuffer = trigBuffs_PP[id];
            ComputeBuffer counterBuffer = countBuffs[id];
            int[] bufferCount = bufferCounts[id];
            ////Debug.Log("MARCH2: " + node.center);





            //perlin_shader.SetBuffer(PNkernel, "output", noiseDataBuffer);
            ////Debug.Log("MARCH3: " + node.center);

            //perlin_shader.SetVector("minCorner", new float4(node.bounds.min, 0));
            //perlin_shader.SetInt("sampleSize", CHUNK_SIZE + 1);
            //perlin_shader.SetInt("scale", node.level);
            ////Debug.Log("MARCH4: " + node.center);
            //perlin_shader.SetBuffer(PNkernel, "craters", cratersBuffer);
            //perlin_shader.SetFloat("craterVerticalScale", craterConfig.verticalScale);
            //perlin_shader.SetFloat("craterRadius", craterConfig.radius);
            //perlin_shader.SetFloat("craterRimSharpness", craterConfig.rimSharpness);
            //perlin_shader.SetFloat("craterRimThickness", craterConfig.rimThickness);

            int groupSize = Chunk.CHUNK_SIZE / 8;
            ////Debug.Log("MARCH5: " + node.center);

            //perlin_shader.Dispatch(PNkernel, groupSize + 1, groupSize + 1, groupSize + 1);

            ////data = new float4[(size + 1) * (size + 1) * (size + 1)];
            ////perlinBuffer.GetData(data);
            ////this.hasMesh = HasMesh();
            ////AsyncGPUReadbackRequest PVBrequest = AsyncGPUReadback.Request(noiseDataBuffer);
            ////AsyncGPUReadbackRequest PPPVBrequest = AsyncGPUReadback.Request(PPNoiseDataBuffer);

            //StartCoroutine(GetNoiseDataCoroutine(PVBrequest, PPPVBrequest, node.chunk));
            Chunk chunk = node.chunk;
            //while (!PVBrequest.done || !PPPVBrequest.done) continue;
            //if (PVBrequest.hasError || PPPVBrequest.hasError) { yield break};
            //float4[] data = new float4[(CHUNK_SIZE + 1) * (CHUNK_SIZE + 1) * (CHUNK_SIZE + 1)];
            //noiseDataBuffer.GetData(data);
            //Debug.Log(data[0] + " " + data[data.Length - 1]);
            ////chunk.PPData = PPPVBrequest.GetData<float4>();
            //bool hasMesh = false;
            ////if (chunk.center.x == -128f && chunk.center.y == -128f && chunk.center.z == -128f) {
            //    float4[] d = data;
            //    for (int i = 0; i < d.Length; i++) {
            //        if (!(d[i].w > 0 && d[0].w > 0 || (d[i].w <= 0 && d[0].w <= 0))) {
            //            hasMesh = true;
            //            break;
            //        }
            //    }
            //    Debug.Log(hasMesh);
            ////}
            ////Debug.Log(result.Length + " " + chunk.data.Value.Length);
            //chunk.hasMesh = hasMesh;
            //chunk.data = data;
            //if (!hasMesh) yield break;
            //if (((Vector3)chunk.center + Vector3.one * chunk.size / 2).y < chunk.data.Value[0].y) Debug.LogError("Data out of NODE");
            //if (((Vector3)chunk.center + Vector3.one * chunk.size / 2).y < chunk.PPData.Value[0].y) Debug.LogError("PPData out of NODE");


            //Debug.Log("MARCH " + chunk.center);
            trianglesBuffer.SetCounterValue(0);
            PPTrianglesBuffer.SetCounterValue(0);
            mc_shader.SetVector("minCorner", new float4(node.bounds.min, 0));

           // mc_shader.SetBuffer(MCkernel, "dataBuffer", noiseDataBuffer);
            mc_shader.SetBuffer(MCkernel, "trianglesBuffer", trianglesBuffer);
            mc_shader.SetBuffer(MCkernel, "PPTrianglesBuffer", PPTrianglesBuffer);

            mc_shader.SetFloat("isolevel", MarchingCube.isolevel);
            mc_shader.SetInt("dataSize", CHUNK_SIZE + 1);
            mc_shader.SetInt("scale", node.level);
            //MCShader.SetInt("scale", node.level + 1);


            mc_shader.Dispatch(MCkernel, groupSize, groupSize, groupSize);

            ComputeBuffer.CopyCount(trianglesBuffer, counterBuffer, 0);
            counterBuffer.GetData(bufferCount);
            Triangle[] triangles = new Triangle[bufferCount[0]];
            trianglesBuffer.GetData(triangles);

            ComputeBuffer.CopyCount(PPTrianglesBuffer, counterBuffer, 0);
            counterBuffer.GetData(bufferCount);
            Triangle[] PPtriangles = new Triangle[bufferCount[0]];
            PPTrianglesBuffer.GetData(PPtriangles);

            result[0] = GenMesh(triangles);
            if (chunk.mesh != null && ((Vector3)chunk.center + Vector3.one * chunk.size / 2).y < chunk.mesh.vertices[0].y) Debug.LogError("Mesh out of NODE" + chunk.center + " " + node.level);
            result[1] = GenMesh(PPtriangles);

            if (result[1] == null && result[0] != null || result[0] == null && result[1] != null) {
                Debug.LogError("!!!!!!!!!!!!!!!!!!!");
            }

            if (result[0] != null && result[1] != null) chunk.hasMesh = true;
            chunk.calculated = true;
            //Debug.Log("MARCH: " + node.center);
            return result;
        }

        Mesh GenMesh(Triangle[] triangles) {

            //Debug.Log("GENMESH " + chunk.center);

            int numTris = triangles.Length;
            if (numTris == 0) return null;
            var meshTriangles = new int[numTris * 3];

            Vector3[] vertices, normals;


            if (IS_SMOOTH) {
                int counter = 0;
                Dictionary<Vector3, int> verticesMap = new Dictionary<Vector3, int>();
                for (int i = 0; i < numTris; i++) {
                    for (int j = 0; j < 3; j++) {
                        Vector3 v = triangles[i][j];
                        if (!verticesMap.ContainsKey(v)) {
                            verticesMap.Add(v, counter);
                            counter++;
                        }
                        meshTriangles[i * 3 + j] = verticesMap[v];
                    }
                }
                vertices = verticesMap.Keys.ToArray<Vector3>();
                normals = new Vector3[numTris * 3];

            } else {
                vertices = new Vector3[numTris * 3];
                normals = new Vector3[numTris * 3];

                for (int i = 0; i < numTris; i++) {

                    meshTriangles[i * 3 + 0] = i * 3 + 0;
                    vertices[i * 3 + 0] = triangles[i].vertexC;
                    normals[i * 3 + 0] = triangles[i].normalC;

                    meshTriangles[i * 3 + 1] = i * 3 + 1;
                    vertices[i * 3 + 1] = triangles[i].vertexB;
                    normals[i * 3 + 1] = triangles[i].normalB;

                    meshTriangles[i * 3 + 2] = i * 3 + 2;
                    vertices[i * 3 + 2] = triangles[i].vertexA;
                    normals[i * 3 + 2] = triangles[i].normalA;

                }
            }



            //Debug.Log(chunk.hasMesh + " " + triangles.Length + " "+ chunk.center);
            Mesh mesh = new Mesh();
            mesh.MarkDynamic();
            mesh.SetVertices(vertices);
            mesh.SetTriangles(meshTriangles, 0);
            mesh.SetNormals(normals);
            //mesh.RecalculateNormals();
            //chunk.meshGO = chunkGO;

            //Debug.Log("Added Chunk " + chunk.center);
            return mesh;

        }

        void ReleaseBuffers() {
            for (int i = 0; i < THREAD_MAX; i++) {
                countBuffs[i].Release();
                trigBuffs[i].Release();
                trigBuffs_PP[i].Release();
                noiseBuffs[i].Release();
            }

        }

    }

    [Serializable]
    public struct CraterConfig {
        public float verticalScale;
        public float radius;
        public float rimSharpness;
        public float rimThickness;
    }

    public struct Crater {
        public Vector2 position;
        public float radius;
        public float verticalScale;
        public float rimSharpness;
        public float rimThickness;
    }

    public struct Triangle {
        public Vector3 vertexA;
        public Vector3 vertexB;
        public Vector3 vertexC;
        public Vector3 normalA;
        public Vector3 normalB;
        public Vector3 normalC;

        public Vector3 this[int i] {
            get {
                switch (i) {
                    case 0:
                        return vertexC;
                    case 1:
                        return vertexB;
                    default:
                        return vertexA;
                }
            }
        }

        override public string ToString() {
            return "("+vertexA +", " + vertexB + ", " + vertexC + ")";
        }
    }
}
