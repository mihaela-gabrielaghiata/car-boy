using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class GenerateRoadColliders : MonoBehaviour
{
    [Header("Wall Settings")]
    public float wallHeight = 2f;

    private Mesh sourceMesh;

    void Start()
    {
        GenerateWalls();
    }

    public void GenerateWalls()
    {
        MeshFilter mf = GetComponent<MeshFilter>();
        sourceMesh = mf.sharedMesh;

        if (sourceMesh == null)
        {
            Debug.LogError("Track has no mesh!");
            return;
        }

        Vector3[] verts = sourceMesh.vertices;
        int vCount = verts.Length;

        if (vCount % 2 != 0)
        {
            Debug.LogError("Mesh vertex count is not divisible by 2. Expected 2 verts per segment.");
            return;
        }

        int segmentCount = vCount / 2;

        // Generate left wall
        GameObject leftWall = new GameObject("LeftWall");
        leftWall.transform.SetParent(transform, false);
        GenerateWall(leftWall, verts, segmentCount, leftSide: true);
        leftWall.layer = LayerMask.NameToLayer("TrackLimit");

        // Generate right wall
        GameObject rightWall = new GameObject("RightWall");
        rightWall.transform.SetParent(transform, false);
        GenerateWall(rightWall, verts, segmentCount, leftSide: false);
        rightWall.layer = LayerMask.NameToLayer("TrackLimit");
    }

    void GenerateWall(GameObject wallObj, Vector3[] trackVerts, int segments, bool leftSide)
    {
        MeshFilter mf = wallObj.AddComponent<MeshFilter>();
        // MeshRenderer mr = wallObj.AddComponent<MeshRenderer>();
        MeshCollider mc = wallObj.AddComponent<MeshCollider>();

        // optional, poți pune material
        // mr.material = new Material(Shader.Find("Standard"));

        // Prepare wall vertices
        Vector3[] wallVerts = new Vector3[segments * 2];
        int idx = 0;

        for (int i = 0; i < segments; i++)
        {
            int trackIndex = leftSide ? i * 2 : i * 2 + 1;

            Vector3 baseVert = trackVerts[trackIndex];

            wallVerts[idx++] = baseVert;
            wallVerts[idx++] = baseVert + Vector3.up * wallHeight;
        }

        // Generate triangles
        List<int> tris = new List<int>();

        for (int i = 0; i < segments - 1; i++)
        {
            int a = i * 2;
            int b = a + 1;
            int c = a + 2;
            int d = a + 3;

            if (leftSide)
            {
                // First triangle
                tris.Add(a);
                tris.Add(b);
                tris.Add(c);

                // Second triangle
                tris.Add(c);
                tris.Add(b);
                tris.Add(d);
            }
            else
            {
                // First triangle
                tris.Add(a);
                tris.Add(c);
                tris.Add(b);

                // Second triangle
                tris.Add(c);
                tris.Add(d);
                tris.Add(b);
            }
            
        }
        
        Mesh wallMesh = new Mesh();
        wallMesh.name = leftSide ? "LeftWallMesh" : "RightWallMesh";
        wallMesh.vertices = wallVerts;
        wallMesh.triangles = tris.ToArray();
        wallMesh.RecalculateNormals();
        wallMesh.RecalculateBounds();

        mf.sharedMesh = wallMesh;
        mc.sharedMesh = wallMesh;
    }
}
