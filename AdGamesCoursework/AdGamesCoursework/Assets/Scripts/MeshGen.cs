using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshGen : MonoBehaviour
{
    public bool autoUpdate = true;
  private Mesh mesh;
  private Vector3[] vertices;
  private int[] triangles;
  private Color[] colors;
  private Vector2[] uvs;
  private float minTerrainHeight;
  private float maxTerrainHeight;
  public float frequency;
  public float persistence;
  public float amplitude;
  public int octaves;
  public float lacunarity;
  private Vector3[] OctaveOffsets;
  public int xSize;
  public int zSize;

  private void Start()
  {
    mesh = new Mesh();
    GetComponent<MeshFilter>().mesh =  mesh;
    xSize = 200;
    zSize = 200;
    octaves = 3;
    persistence = 0.9f;
    lacunarity = 1f;
    OctaveOffsets = new Vector3[octaves];
    for (int index = 0; index <  octaves; ++index)
    {
      float x =   Random.Range(-10000, 10000);
      float y =   Random.Range(-10000, 10000);
      float z =   Random.Range(-10000, 10000);
      OctaveOffsets[index] = new Vector3(x, y, z);
    }
     CreateShape();
  }

  private void Update()
  {
    if (! autoUpdate)
      return;
     UpdateMesh();
  }

  private void CreateShape()
  {
    vertices = new Vector3[( xSize + 1) * ( zSize + 1)];
    for (int i = 0, z = 0; z <= zSize; z++)
    {
      for (int x = 0; x <=  xSize; x++)
      {
        amplitude = 10f;
        frequency = 0.02f;
        float noiseHeight = 0.0f;
        for (int o = 0; o < octaves; ++o)
        {
          float offsetX = x / 0.3f * frequency + OctaveOffsets[o].x;
          float offsetZ = z / 0.3f *  frequency +  OctaveOffsets[o].z;
          float perlinValue = PerlinGenerator(offsetX, noiseHeight, offsetZ);
          noiseHeight += perlinValue * amplitude;
          amplitude *=  persistence;
          frequency *=  lacunarity;
        }
        float y = noiseHeight;
        if (y > maxTerrainHeight)
          maxTerrainHeight = y;
        if (y < minTerrainHeight)
          minTerrainHeight = y;
        float round = 1f - Mathf.Round(Mathf.InverseLerp( minTerrainHeight,  maxTerrainHeight, y) * 10f) / 10f;
        vertices[i] = new Vector3(x,(round *y * 2), z);
        i++;
      }
    }
    triangles = new int[xSize * zSize * 6];
    int vert = 0;
    int tris = 0;
    for (int z = 0; z < zSize; ++z)
    {
      for (int x = 0; x < xSize; ++x)
      {
         triangles[tris] = vert;
         triangles[tris + 1] = vert +  xSize + 1;
         triangles[tris + 2] = vert + 1;
         triangles[tris + 3] = vert + 1;
         triangles[tris + 4] = vert +  xSize + 1;
         triangles[tris + 5] = vert +  xSize + 2;
        vert++;
        tris += 6;
      }
      vert++;
    }
    uvs = new Vector2[vertices.Length];
    for (int i = 0, z = 0; z <= zSize; z++)
    {
      for (int x = 0; x <= xSize; x++)
      {
        uvs[i] = new Vector2((float)x/xSize, (float)z/zSize);
        i++;
      }
    }
  }

  private void UpdateMesh()
  {
    if (Input.GetKeyDown(KeyCode.Escape))
      Application.Quit();
     mesh.Clear();
     mesh.vertices =  vertices;
     mesh.triangles =  triangles;
     mesh.uv = uvs;
     CreateShape();
     mesh.RecalculateNormals();
  }

  public float PerlinGenerator(float x, float y, float z)
  {
    float ab =   Mathf.PerlinNoise(x, y);
    float bc = Mathf.PerlinNoise(y, z);
    float ac = Mathf.PerlinNoise(x, z);
    float ba = Mathf.PerlinNoise(y, x);
    float cb = Mathf.PerlinNoise(z, y);
    float ca = Mathf.PerlinNoise(z, x);
    return((ab + bc + ac + ba + cb + ca) / 6);
  }
}
