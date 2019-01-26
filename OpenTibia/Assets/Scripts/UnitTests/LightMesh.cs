using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightMesh : MonoBehaviour
{
    private Mesh m_CustomMesh;
    private Matrix4x4 m_LightTransformationMatrix;
    private Material m_Material;

    private int m_CachedScreenHeight;
    private int m_CachedScreenWidth;

    // Start is called before the first frame update
    void Start()
    {
        m_Material = new Material(Shader.Find("Hidden/Internal-Colored"));

        m_CustomMesh = new Mesh();
        m_CustomMesh.vertices = new Vector3[] { new Vector3(0, 0, 0), new Vector3(0, 1, 0), new Vector3(1, 1, 0), new Vector3(1, 0, 0) };
        m_CustomMesh.colors = new Color[] { Color.red, Color.black, Color.white, Color.blue };
        m_CustomMesh.SetIndices(new int[] { 0, 1, 2, 3 }, MeshTopology.Quads, 0);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnGUI() {
        if (Event.current.type != EventType.Repaint)
            return;

        if (m_CachedScreenHeight != Screen.height || m_CachedScreenWidth != Screen.width) {
            m_CachedScreenWidth = Screen.width;
            m_CachedScreenHeight = Screen.height;
            m_LightTransformationMatrix = Matrix4x4.Scale(new Vector3(m_CachedScreenWidth, m_CachedScreenHeight, 0));
        }

        m_Material.SetPass(0);
        Graphics.DrawMeshNow(m_CustomMesh, m_LightTransformationMatrix);
    }
}
