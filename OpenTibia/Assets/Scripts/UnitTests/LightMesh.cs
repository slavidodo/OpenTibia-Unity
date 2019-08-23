using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightMesh : MonoBehaviour
{
    private Mesh _customMesh;
    private Matrix4x4 _lightTransformationMatrix;
    private Material _material;

    private int _cachedScreenHeight;
    private int _cachedScreenWidth;

    // Start is called before the first frame update
    void Start()
    {
        _material = new Material(Shader.Find("Hidden/public-Colored"));

        _customMesh = new Mesh();
        _customMesh.vertices = new Vector3[] { new Vector3(0, 0, 0), new Vector3(0, 1, 0), new Vector3(1, 1, 0), new Vector3(1, 0, 0) };
        _customMesh.colors = new Color[] { Color.red, Color.black, Color.white, Color.blue };
        _customMesh.SetIndices(new int[] { 0, 1, 2, 3 }, MeshTopology.Quads, 0);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnGUI() {
        if (Event.current.type != EventType.Repaint)
            return;

        if (_cachedScreenHeight != Screen.height || _cachedScreenWidth != Screen.width) {
            _cachedScreenWidth = Screen.width;
            _cachedScreenHeight = Screen.height;
            _lightTransformationMatrix = Matrix4x4.Scale(new Vector3(_cachedScreenWidth, _cachedScreenHeight, 0));
        }

        _material.SetPass(0);
        Graphics.DrawMeshNow(_customMesh, _lightTransformationMatrix);
    }
}
