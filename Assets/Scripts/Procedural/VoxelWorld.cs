using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine.Rendering;
using Unity.Collections;
using Unity.Jobs;
using System;
using System.Threading;
using UnityEngine.Profiling;

namespace Voxel {

    /// <summary>
    /// A voxel space that is infitely expandable
    /// </summary>
    public class VoxelWorld : MonoBehaviour {

        public Octree octree;

        public int nearViewDistance = 2;
        public int midViewDistance = 2;
        public int farViewDistance = 2;
        public bool showRendered = false;

        public GameObject chunkPrefab;
        [HideInInspector]
        public float isolevel = -3;


        public static WorldType worldType;

        public bool gizmos = false;
        public Material material;
        public Transform player;

        public Plane[] frustumPlanes;

        public const int CHUNK_SIZE = Chunk.CHUNK_SIZE;
        

        private ChunkManager chunks;
        private MarchingCube mc;
        private bool initialized = false;
        public CraterConfig craterConfig;

        public enum WorldType {
            Flat,
            Terrain,
            Caves,
            Sponge,
            Maze,
        };




        void Start() {
            Initialize();
        }

        public void Initialize() {
            VoxelWorld.worldType = WorldType.Terrain;
            ClearWorld();
            octree = new Octree(CHUNK_SIZE, new Vector3());
            mc = new MarchingCube();
            chunks = new ChunkManager(transform, chunkPrefab, mc);
            chunks.MarkAllFlush();
            chunks.Flush();

            mc.craterConfig = craterConfig;
            Debug.Log(craterConfig);

            Debug.Log(CHUNK_SIZE);
            Debug.Log(octree.GetRoot().level);
            int a, b;
            ThreadPool.GetMaxThreads(out a, out b);
            Debug.Log(a + " " + b);
            Debug.Log(Environment.ProcessorCount);
        }


        void Update() {
            Culling();
        }

        HashSet<float3> queries = new HashSet<float3>();

        Vector3 prevScanPosition, currScanPosition;

        public void Culling() {
            queries.Clear();


           //currScanPosition = player.position;

           //if (currScanPosition != prevScanPosition) {
                chunks.MarkAllFlush();

                //Profiler.BeginSample("SCAN");
                ScanNodes(currScanPosition, ranges);
                //Profiler.EndSample();

                frustumPlanes = GeometryUtility.CalculateFrustumPlanes(Camera.main);
                LODQuery(octree.GetRoot(), ranges.Length - 1, ranges);

                //prevScanPosition = currScanPosition;
                chunks.Flush();
            //}

            //for (int x = -2; x < 2; x++) {
            //    for (int y = -2; y < 2; y++) {
            //        for (int z = -2; z < 2; z++) {
            //            Vector3 pos = new Vector3(x, y, z);
            //            pos = pos * 64;
            //            //alcAndDisplayChunk(octree.GetRoot().GetNodeAt(pos, 2), false);
            //            CalcAndDisplayChunk(octree.GetRoot().GetNodeAt(pos, 1), true);

            //        }
            //    }
            //}

            //Vector3 pos = new Vector3(-128, -128, -128);
            //CalcAndDisplayChunk(octree.GetRoot().GetNodeAt(pos, 0), false);

            //Debug.Log(chunks.displayedChunks.Count);

        }
        //int[] ranges = new int[] { 100 };
        //int[] ranges = new int[] { 100, 150 };
        //int[] ranges = new int[] {200, 300, 400, 800, 1600};
        int[] ranges = new int[] { 100, 150, 200, 400, 800, 1600, 3200, 6400};
        //int[] ranges = new int[] { 2, 3, 4, 6, 12, 25, 50, 1000 };

        bool LODQuery(Octree node, int LOD, int[] ranges) {
            if (LOD < 0) Debug.LogError("LOD query less than zero: " + LOD);
            if (LOD > ranges.Length) Debug.LogError("LOD overflow: " + LOD);
            if (node == null) Debug.LogError("null Node: " + node);
            if (!node.IntersectSphere(player.position, ranges[LOD])) return false;
            //if (LOD != 0 && !GeometryUtility.TestPlanesAABB(frustumPlanes, node.bounds)) return false;

            if (node.level > LOD) {
                node.Subdivide();
                foreach (Octree child in node.children) LODQuery(child, LOD, ranges);
                return true;
            }
            if (LOD == 0) {
                queries.Add(node.bounds.center);
                chunks.Display(node, false);
                return true;
            } else {
                if (!node.IntersectSphere(player.position, ranges[LOD-1])) {
                    chunks.Display(node, false);
                } else {
                    //Debug.Log("!");
                    node.Subdivide();
                    foreach(Octree child in node.children) {
                        if (!LODQuery(child, LOD -1, ranges)) {
                            chunks.Display(child, true);
                        }
                    }
                }
                return true;
            }
        }

        void ScanNodes(Vector3 center, int[] ranges) {
            for (int i = 0; i < ranges.Length; i++) {
                int range = ranges[i]; 
                int LOD = i;
                //Debug.LogFormat("{0} {1} {2} {3}", root, LOD, center, range);

                int step = (1 << LOD) * CHUNK_SIZE / 2;
                for (int x = -range; x < range + step; x += step) {
                    for (int y = -range; y < range + step; y += step) {
                        for (int z = -range; z < range + step; z += step) {
                            Vector3 pos = center;
                            pos.x += x;
                            pos.y += y;
                            pos.z += z;
                            //Debug.Log(pos);
                            octree.GetRoot().GetNodeAt(pos, LOD);
                        }
                    }
                }
            }
        }

        

        void RoundUp(ref Vector3 pos, int n) {

            int m = n / nearViewDistance;

            //float z = pos.z;

            pos.x = pos.x < 0 ? pos.x - m : pos.x;
            pos.y = pos.y < 0 ? pos.y - m : pos.y;
            pos.z = pos.z < 0 ? pos.z - m : pos.z;
            
            //Debug.Log(pos.z + " " + Mathf.Round((pos.z / n)));
            
            pos.x = Mathf.Round((pos.x / n)) * n;
            pos.y = Mathf.Round((pos.y / n)) * n;
            pos.z = Mathf.Round((pos.z / n)) * n;

        }

        void SnapToReferencePoint(ref Vector3 pos, int LOD) {
            //The position is snapped to a grid with size = nearViewDistance X CHUNK_SIZE
            pos -= Vector3.up * 3;
            RoundUp(ref pos, CHUNK_SIZE * (2 << LOD));
            pos += Vector3.one * CHUNK_SIZE / 2;        //Offset point to sit on a grid intersection
        }


        void Awake() {
            //Debug.Log(transform.childCount);
            foreach (Transform child in transform) {
                GameObject.Destroy(child.gameObject);
            }
        }

        public void ClearWorld() {
            //if (chunks != null) chunks.Discard();
            int guard = 0;
            while (transform.childCount != 0 && guard < 100) {
                foreach (Transform c in transform) {
                    DestroyImmediate(c.gameObject);
                }
                guard++;
            }
            
            Debug.Log(transform.childCount);
            chunks = null;
            octree = null;
        }

        IEnumerator DisplayChunk(Octree node, bool partialParent) {
            while (node.chunk.mesh == null) yield return null;
            chunks.Display(node, partialParent);
        }

        void OnDestroy() {

        }




        //private IEnumerator GenMeshCoroutine(Chunk chunk) {
        //    GenMesh(chunk);
        //    yield return null;
        //}

        void RemoveMesh(Chunk chunk) {
            if (chunk.meshGO != null) {
                DestroyImmediate(chunk.meshGO);
            }
        }

        public void InitializeBuffers() {
 

        }

        void OnDrawGizmos() {
            // Draw a yellow sphere at the transform's position
            Gizmos.color = Color.yellow;
            if (gizmos && octree != null) {
                drawTree(octree.GetRoot());
            }

            if (queries != null) {
                foreach(float3 q in queries) {
                    Gizmos.DrawSphere(q, 5f);
                }
            }

            if (ranges != null) {
                foreach(int r in ranges) {
                    Gizmos.DrawWireSphere(player.position, r);
                }
            }
        }

        void drawTree(Octree root) {
            if (root == null) return;
            if ((showRendered && chunks.displayedChunks.ContainsKey(root.chunk)) || (!showRendered && root.chunk.active)) {
                Gizmos.DrawWireCube(root.chunk.center, Vector3.one * root.chunk.size);

            }
            if (root.children == null) return;
            foreach(Octree child in root.children) {
                drawTree(child);
            }
        }


        //public static float4[] InitializeChunkData(float3 minCorner) {
        //    var PNkernel = PerlinShader.FindKernel("PerlinMain");

        //}


    }




    /// <summary>
    /// Manages calculated chunks
    /// </summary>
    public class ChunkManager {
        //holds all calculated chunks
        public Queue<Chunk> calculatedChunkQueue;
        public Dictionary<Chunk, GameObject> displayedChunks;
        public GameObjectPool pool;

        private MarchingCube mc;

        private List<Chunk> toBeRemoved;

        private Dictionary<Chunk, Mesh[]> meshCache;
        private Dictionary<Chunk, Mesh> PP_meshCache;

        private Queue<Chunk> meshCacheQueue;

        int poolSize = 500;
        int meshCacheSize = 1000;

        public ChunkManager(Transform parent, GameObject chunkPrefab,  MarchingCube mc) {
            calculatedChunkQueue = new Queue<Chunk>();
            displayedChunks = new Dictionary<Chunk, GameObject>();
            pool = new GameObjectPool();
            toBeRemoved = new List<Chunk>();
            this.mc = mc;
            meshCache = new Dictionary<Chunk, Mesh[]>();
            PP_meshCache = new Dictionary<Chunk, Mesh>();
            meshCacheQueue = new Queue<Chunk>();

            for (int i = 0; i < poolSize; i++) {
                pool.AddToPool(GameObject.Instantiate(chunkPrefab, parent));
            }
        }

        public void Display(Octree node, bool partialParent) {
            Chunk chunk = node.chunk;
            chunk.active = true;
            if (chunk.calculated && !chunk.hasMesh) return;
            if (displayedChunks.ContainsKey(chunk) && partialParent == chunk.partialParent) {
                //Debug.LogWarning("Display Chunk already displayed.");
                return;
            } else {
                Mesh mesh = GetMeshFromCache(chunk, false);
                if (mesh == null) CacheMesh(chunk, mc.MarchChunk(node));
                if (!chunk.hasMesh) return;
                if (!displayedChunks.ContainsKey(chunk)) {
                    displayedChunks.Add(chunk, pool.DePool());
                }
                GameObject chunkObject = displayedChunks[chunk];
                chunk.partialParent = partialParent;

                //chunkObject.transform.position = chunk.center;
               
                mesh = GetMeshFromCache(chunk, false);

                int lod = node.level;
                if (partialParent) {
                    mesh = GetMeshFromCache(chunk, true);
                    lod++;
                }
                Color color = Color.HSVToRGB(lod * 0.1f, 0.5f, 1);

                if (lod == 0) {
                    chunkObject.GetComponent<MeshCollider>().sharedMesh = GetMeshFromCache(chunk, false);
                }
                chunkObject.GetComponent<MeshFilter>().mesh = mesh;
                chunkObject.GetComponent<ChunkBehavior>().chunk = chunk;
                //Collision generate
                // chunkObject.GetComponent<MeshRenderer>().material.SetColor("_BaseColor", color);

            }
        }

        public void CacheMesh(Chunk chunk, Mesh[] meshes) {
            if (meshCache.ContainsKey(chunk)) Debug.LogError("Chunk mesh already cached!");
            if (meshCacheQueue.Count == meshCacheSize) {
                Chunk c = meshCacheQueue.Dequeue();
                if (c.hasMesh) c.calculated = false;
                meshCache.Remove(c);
            }
            meshCache.Add(chunk, meshes);
            meshCacheQueue.Enqueue(chunk);
        }

        public Mesh GetMeshFromCache(Chunk chunk, bool partialParent) {
            if (!meshCache.ContainsKey(chunk)) return null;
            if (partialParent) return meshCache[chunk][1];
            else return meshCache[chunk][0];
        }

        /// <summary>
        /// Removes displayed chunks that are not set active
        /// </summary>
        public void Flush() {
            toBeRemoved.Clear();
            foreach (Chunk c in displayedChunks.Keys) {
                if (!c.active) {
                    toBeRemoved.Add(c);
                }
            }
            foreach (Chunk c in toBeRemoved) {
                Hide(c);
            }
        }

        /// <summary>
        /// Set all displayed chunks.active to false
        /// </summary>
        public void MarkAllFlush() {
            foreach (Chunk c in displayedChunks.Keys) {
                c.active = false;
            }
        }


        public void Hide(Chunk chunk) {
            if (!displayedChunks.ContainsKey(chunk)) Debug.LogError("Hide Chunk not displayed.");
            pool.Pool(displayedChunks[chunk]);
            displayedChunks.Remove(chunk);
            chunk.startedDisplay = false;
        }

        public void Discard() {
            pool.Discard();
        }

    }


}
