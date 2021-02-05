using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Assertions;
using UnityEditor.Rendering.Universal;
using UnityEngine;

public class Subdivision : MonoBehaviour {
    public MeshFilter meshFilter;
    void Start() {
    }

    // Update is called once per frame
    void Update() {

    }

    public Mesh originalMesh;
    private MeshData data;

    public void Test() {
        data = ProcessMesh(originalMesh);
        Debug.Log(data.faces.Count);
        Debug.Log(data.edges.Count);

        //foreach (int i in originalMesh.triangles) {
        //    Debug.Log(i);
        //}
        //foreach (Vertex v in data.vertices.Values) {
        //    Debug.Log(v);
        //}
        //Debug.Log(data.vertices.Count);
        List<Vector3> verts = new List<Vector3>();
        List<int> trigs = new List<int>();
        Dictionary<Vector3, int> vertsDict = new Dictionary<Vector3, int>();
        foreach (Face f in data.faces) {
            foreach (Vertex v in f.vertices) {
                Edge[] edges = f.GetSharedEdges(v);
                QuadToTrigs(v.VertexPoint(), edges[0].EdgePoint(), f.FacePoint(), edges[1].EdgePoint(), verts, trigs, vertsDict);
            }
        }
        //Debug.Log(verts.Count);
        //Debug.Log(trigs.Count);
        Mesh mesh = new Mesh();
        mesh.vertices = verts.ToArray();
        mesh.triangles = trigs.ToArray();
        mesh.RecalculateNormals();
        Vector2[] uvs = new Vector2[verts.Count];

        for (int i = 0; i < uvs.Length; i++) {
            uvs[i] = new Vector2(verts[i].x, verts[i].z);
        }
        mesh.uv = uvs;


        meshFilter.mesh = mesh;
        originalMesh = mesh;
        //foreach (Vector3 i in mesh.vertices) Debug.Log(i);


    }

    public void QuadToTrigs(Vector3 A, Vector3 B, Vector3 C, Vector3 D, List<Vector3> vertices, List<int> triangles, Dictionary<Vector3, int> vertsDict) {
        int start = vertices.Count;
        int a, b, c, d;
        if (vertsDict.ContainsKey(A)) a = vertsDict[A];
        else {
            vertices.Add(A);
            a = vertices.Count -1;
            vertsDict.Add(A, a);
        }
        if (vertsDict.ContainsKey(B)) b = vertsDict[B];
        else {
            vertices.Add(B);
            b = vertices.Count - 1;
            vertsDict.Add(B, b);
        }
        if (vertsDict.ContainsKey(C)) c = vertsDict[C];
        else {
            vertices.Add(C);
            c = vertices.Count - 1;
            vertsDict.Add(C, c);
        }
        if (vertsDict.ContainsKey(D)) d = vertsDict[D];
        else {
            vertices.Add(D);
            d = vertices.Count - 1;
            vertsDict.Add(D, d);
        }

        //int a = start;
        //int b = start + 1;
        //int c = start + 2;
        //int d = start + 3;
        //vertices.Add(A); //v
        //vertices.Add(B); //e1
        //vertices.Add(C); //f
        //vertices.Add(D); //e2
        triangles.AddRange(new int[] { a, b, c });
        triangles.AddRange(new int[] { a, c, d });
    }


    private void OnDrawGizmos() {
        if (data.faces != null) {
            //foreach (Triangle triangle in data.triangles) {
            //    Vector3 c = triangle.FacePoint();
            //    Gizmos.DrawSphere(c + transform.position, .2f);

            //}
            //foreach (Edge e in data.edges.Values) {
            //    Vector3 c = e.EdgePoint();
            //    Gizmos.DrawSphere(c + transform.position, .05f);

            //}

            foreach (Vertex v in data.vertices.Values) {
                Gizmos.DrawSphere(v.VertexPoint() + transform.position, 0.1f);
            }
        }

    }

    MeshData ProcessMesh(Mesh mesh) {
        int[] triangles = mesh.triangles;
        Vector3[] vertices = mesh.vertices;
        MeshData data;
        data.faces = new HashSet<Face>();
        data.edges = new Dictionary<Tuple<Vector3, Vector3>, Edge>(new EdgeTupleComparer());
        data.vertices = new Dictionary<Vector3, Vertex>();
        data.mesh = mesh;
        for (int i = 0; i < vertices.Length; i++) {
            Vector3 pos = vertices[i];
            if (!data.vertices.ContainsKey(pos)) {
                Vertex v = new Vertex(pos);
                data.vertices.Add(pos, v);
            }
        }

        for (int i = 0; i < triangles.Length; i += 6) {
            Vertex A = data.vertices[vertices[triangles[i]]];
            Vertex B = data.vertices[vertices[triangles[i + 1]]];
            Vertex C = data.vertices[vertices[triangles[i + 2]]];
            Vertex D = data.vertices[vertices[triangles[i + 5]]];
            int a = triangles[i];
            int b = triangles[i + 1];
            int c = triangles[i + 2];
            int d = triangles[i + 5];
            int[] vertIndices = new int[4] { a, b, c, d };
            Face f = new Face(A, B, C, D);
            data.faces.Add(f);
            for (int j = 0; j < 4; j++) {
                int x = vertIndices[j % 4];
                int y = vertIndices[(j + 1) % 4];

                Vector3 X = vertices[x];
                Vector3 Y = vertices[y];

                Tuple<Vector3, Vector3> key = new Tuple<Vector3, Vector3>(X, Y);
                if (!data.edges.ContainsKey(key)) {
                    Edge e = new Edge(X, Y);
                    data.edges.Add(key, e);
                }
                f.edges.Add(key, data.edges[key]);
                data.edges[key].faces.Add(f);
            }
        }



        foreach (KeyValuePair<Tuple<Vector3, Vector3>, Edge> pair in data.edges) {
            data.vertices[pair.Value.a].edges.Add(pair.Key, pair.Value);
            data.vertices[pair.Value.b].edges.Add(pair.Key, pair.Value);
        }

        //foreach(Triangle f in data.triangles) {
        //    f.vertices[0] = data.vertices[f.a];
        //    f.vertices[1] = data.vertices[f.b];
        //    f.vertices[2] = data.vertices[f.c];
        //}

        return data;
    }





    struct MeshData {
        public Mesh mesh;
        public HashSet<Face> faces;
        public Dictionary<Vector3, Vertex> vertices;
        public Dictionary<Tuple<Vector3, Vector3>, Edge> edges;
    }

    struct Vertex {
        public Dictionary<Tuple<Vector3, Vector3>, Edge> edges;
        public Vector3 position;
        public Vertex(Vector3 position) {
            this.position = position;
            edges = new Dictionary<Tuple<Vector3, Vector3>, Edge>(new EdgeTupleComparer());
        }
        override public string ToString() {
            string edgesString = "[";
            foreach (Edge e in edges.Values) {
                edgesString += e;
            }
            edgesString += "]";
            return "Vertex " + position + "Edges " + edgesString;
        }

        public Vector3 VertexPoint() {
            HashSet<Face> faces = new HashSet<Face>();
            foreach (Edge e in edges.Values) {
                //foreach (Face f in e.faces) {
                //    faces.Add(f);
                //}
                faces.UnionWith(e.faces);
            }
            float n = faces.Count;
            Vector3 avgFP = Vector3.zero;
            foreach (Face f in faces) {
                avgFP += f.FacePoint();
            }
            avgFP /= n;
            Vector3 avgEMP = Vector3.zero;
            foreach (Edge e in edges.Values) {
                avgEMP += e.MidPoint();
            }
            avgEMP /= edges.Count;

            Vector3 p = ((n - 3) / n * position) +
                        (1 / n * avgFP) +
                        (2 / n * avgEMP);
            //Vector3 X = (n - 3) / n * position;
            //Vector3 Y = (1 / n * avgFP);
            //Vector3 Z = (2 / n * avgEMP);
            //Debug.Log(n +" " + position + avgFP + avgEMP + p);
            //Debug.Log(n + " " + X +  Y + Z);

            return p;
        }
    }


    struct Edge {
        public Edge(Vector3 a, Vector3 b) {
            this.a = a;
            this.b = b;
            faces = new HashSet<Face>();
        }
        public Vector3 a, b;
        public HashSet<Face> faces;
        public Vector3 EdgePoint() {
            Vector3 p = (a + b) / 2;

            Vector3 fpSum = Vector3.zero;
            int count = 0;
            foreach (Face f in faces) {
                fpSum += f.FacePoint();
                count++;
            }
            if (count == 2) {
                return (p + fpSum / 2) / 2;
            }
            return p;
        }

        public bool HasVertex(Vertex v) {
            return (a == v.position) || (b == v.position);
        }

        public override string ToString() {
            return "E<" + a + "," + b + ">";
        }

        public Vector3 MidPoint() {
            return (a + b) / 2;
        }
    }

    struct Face {
        public Vertex a, b, c, d;
        public Vertex[] vertices;
        public Dictionary<Tuple<Vector3, Vector3>, Edge> edges;

        public Face(Vertex a, Vertex b, Vertex c, Vertex d) {
            this.a = a;
            this.b = b;
            this.c = c;
            this.d = d;
            edges = new Dictionary<Tuple<Vector3, Vector3>, Edge>(new EdgeTupleComparer());
            vertices = new Vertex[4] { a, b, c, d };
        }

        public Vector3 FacePoint() {
            return (a.position + b.position + c.position + d.position) / 4;
        }

        public Edge[] GetSharedEdges(Vertex v) {
            if (v.position == a.position) {
                //A return AB DA
                return new Edge[2] { FindEdge(0, 1), FindEdge(0, 3) };
            } else if (v.position == b.position) {
                //B return BC AB
                return new Edge[2] { FindEdge(1, 2), FindEdge(0, 1) };
            } else if (v.position == c.position) {
                //C return CD BC
                return new Edge[2] { FindEdge(2, 3), FindEdge(1, 2) };
            } else {
                //D return DA CD
                return new Edge[2] { FindEdge(3, 0), FindEdge(2, 3) };
            }
        }

        public Edge FindEdge(int i, int j) {
            return edges[new Tuple<Vector3, Vector3>(vertices[i].position, vertices[j].position)];
        }
    }

    struct Triangle {
        public Triangle(Vertex a, Vertex b, Vertex c) {
            this.a = a;
            this.b = b;
            this.c = c;
            edges = new Dictionary<Tuple<Vector3, Vector3>, Edge>(new EdgeTupleComparer());
            vertices = new Vertex[3] { a, b, c };
        }
        public Vertex[] vertices;
        public Vertex a, b, c;
        public Dictionary<Tuple<Vector3, Vector3>, Edge> edges;
        public Vector3 FacePoint() {
            return (a.position + b.position + c.position) / 3;
        }

        public Edge[] GetSharedEdges(Vertex v) {
            if (v.position == a.position) {
                //A return AB CA
                return new Edge[2] { FindEdge(0, 1), FindEdge(0, 2) };
            } else if (v.position == b.position) {
                //B return BC AB
                return new Edge[2] { FindEdge(1, 2), FindEdge(0, 1) };
            } else {
                //C return CA BC
                return new Edge[2] { FindEdge(2, 0), FindEdge(1, 2) };
            }
        }

        public Edge FindEdge(int i, int j) {
            return edges[new Tuple<Vector3, Vector3>(vertices[i].position, vertices[j].position)];
        }

    }

    class EdgeTupleComparer : IEqualityComparer<Tuple<Vector3, Vector3>> {
        bool IEqualityComparer<Tuple<Vector3, Vector3>>.Equals(Tuple<Vector3, Vector3> x, Tuple<Vector3, Vector3> y) {
            return (x.Item1 == y.Item1 && x.Item2 == y.Item2) || (x.Item1 == y.Item2 && x.Item2 == y.Item1);
        }

        int IEqualityComparer<Tuple<Vector3, Vector3>>.GetHashCode(Tuple<Vector3, Vector3> obj) {
            return obj.Item1.GetHashCode() + obj.Item2.GetHashCode();
        }
    }

}




