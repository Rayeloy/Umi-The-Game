using UnityEngine;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.IO;
namespace MeshTransformation
{
    [ExecuteInEditMode]
    public class TransformationMesh : MonoBehaviour
    {
        Vector3[] vertices;
        Vector3[] vertices2;
        Vector3[] Newnormals;
        Vector3[] normals;
        Vector3[] newVert;
        Vector2[] uvs2__;
        public Mesh mesh;
        public Mesh mesh2;
        Vector2[] mesh2__uv;

        public enum Uv_ // selection of UV for matching
        {
            uv = 0,
            uv2 = 1,
            uv3 = 2
        }
        public Uv_ UV;
        public Uv_ UVMesh2;
        public bool CreateBlendShapes;
        public string NameBlendShape = "Name";
        Vector3[] norm;
        Vector3[] vert_;
        Vector3[] tangents;
        Vector4[] tangents4;
        Vector3[] Newtangents;
        public Transform Rig;
        Vector3 delta_;
        Mesh skinmesh;
        bool OnCalculateVert;

        int time;
        ComputeBuffer buffer;
        ComputeBuffer bufferNormal;
        [Range(0, 1)]
        public float Transformation;
        float OldTransformValue;
        public SkinnedMeshRenderer SR;
        public MeshRenderer MR;
        Mesh bakeMesh;
        public Material material;
        public MeshRenderer[] meshRenderers;
        public SkinnedMeshRenderer[] SkinMeshRenderers;
        public bool UpdateSkinedMeshrenderer;
        public bool UpdateMeshRenderer;
        public Mesh NewMesh_;

        public void OnEnable()
        {

            if (SR == null && GetComponent<SkinnedMeshRenderer>() != null)
            {
                SR = GetComponent<SkinnedMeshRenderer>();
            }

            if (MR == null && SR == null & GetComponent<MeshRenderer>() != null)
            {
                MR = GetComponent<MeshRenderer>();
            }




            if (mesh == null && GetComponent<SkinnedMeshRenderer>() != null)
            {
                mesh = GetComponent<SkinnedMeshRenderer>().sharedMesh;
            }

            if (mesh == null && GetComponent<MeshRenderer>() != null)
            {
                mesh = GetComponent<MeshFilter>().sharedMesh;
            }

            if (SR != null)
            {
                Material Mat;
                Mat = Instantiate(SR.sharedMaterial);
                material = Mat;
            }

            if (MR != null)
            {
                Material Mat;
                Mat = Instantiate(MR.sharedMaterial);
                material = Mat;
            }

        }

        public void Update()
        {
            if (NewMesh_ != null && UpdateMeshRenderer)
            {
                if (buffer != null)
                {
                    buffer.Release();
                    bufferNormal.Release();
                }
                buffer = new ComputeBuffer(mesh.vertexCount, 12);
                bufferNormal = new ComputeBuffer(mesh.normals.Length, 12);

                buffer.SetData(NewMesh_.vertices);
                bufferNormal.SetData(NewMesh_.normals);
                material.SetBuffer("_vert_", buffer);
                material.SetBuffer("_normal_", bufferNormal);

                if (Rig != null)
                {
                    material.SetVector("RigPos", Rig.position);
                }
                MR.material = material;
            }
            if (OldTransformValue != Transformation)
            {
                OldTransformValue = Transformation;
                material.SetFloat("_Transformation", Transformation);
                if (SkinMeshRenderers.Length != 0)
                {
                    for (int i = 0; i < SkinMeshRenderers.Length; i++)
                    {
                        SkinMeshRenderers[i].sharedMaterial.SetFloat("_Transformation", Transformation);
                    }
                }

                if (meshRenderers.Length != 0)
                {
                    for (int i = 0; i < meshRenderers.Length; i++)
                    {
                        meshRenderers[i].sharedMaterial.SetFloat("_Transformation", Transformation);
                    }
                }


                if (MR != null)
                {
                    MR.material = material;
                }
                if (SR != null)
                {
                    SR.material = material;
                }


            }
            if (UpdateSkinedMeshrenderer)
            {
                if (NewMesh_ != null)
                {

                    if (buffer != null)
                    {
                        buffer.Release();

                    }
                    if (bufferNormal != null)
                    {
                        bufferNormal.Release();
                    }

                    if (UpdateSkinedMeshrenderer)
                    {
                        if (bakeMesh != null)
                        {
                            DestroyImmediate(bakeMesh);
                        }
                        if (SR != null)
                        {
                            bakeMesh = new Mesh();
                            buffer = new ComputeBuffer(mesh.vertexCount, 12);
                            bufferNormal = new ComputeBuffer(mesh.normals.Length, 12);
                            Mesh oldmesh;
                            oldmesh = new Mesh();
                            oldmesh = mesh;

                            SR.sharedMesh = NewMesh_;
                            SR.BakeMesh(bakeMesh);
                            SR.sharedMesh = oldmesh;
                            buffer.SetData(bakeMesh.vertices);
                            bufferNormal.SetData(bakeMesh.normals);
                            material.SetBuffer("_vert_", buffer);
                            material.SetBuffer("_normal_", bufferNormal);
                            material.SetVector("RigPos", Rig.position);
                            material.SetVector("posObj", transform.position);
                            if (MR != null)
                            {
                                MR.material = material;
                            }
                            if (SR != null)
                            {
                                SR.material = material;
                            }
                        }
                    }

                }
            }

        }
        void OnDestroy()
        {
            if (buffer != null)
            {
                buffer.Release();
            }
            if (bufferNormal != null)
            {
                bufferNormal.Release();
            }

        }

        void OnDisable()
        {
            if (buffer != null)
            {
                buffer.Release();
            }
            if (bufferNormal != null)
            {
                bufferNormal.Release();
            }

        }
        public void Start_()
        {

            if (mesh == null && GetComponent<SkinnedMeshRenderer>() != null)
            {
                mesh = GetComponent<SkinnedMeshRenderer>().sharedMesh;
            }
            if (GetComponent<SkinnedMeshRenderer>() != null)
            {
                mesh = GetComponent<SkinnedMeshRenderer>().sharedMesh;

            }
            if (GetComponent<MeshFilter>() != null)
            {
                mesh = GetComponent<MeshFilter>().sharedMesh;
            }
            delta_ = Vector3.zero;
            if (GetComponent<SkinnedMeshRenderer>() != null)
            {
                skinmesh = new Mesh();
                GetComponent<SkinnedMeshRenderer>().BakeMesh(skinmesh);

                Vector3 rrr;
                rrr = Rig.position;
                delta_ = transform.position;
                delta_ = rrr - delta_;

            }

            vertices2 = null;
            vertices = null;
            newVert = null;
            uvs2__ = null;
            mesh2__uv = null;
            vertices2 = mesh2.vertices;
            vertices = mesh.vertices;
            normals = mesh2.normals;
            newVert = mesh.vertices;

            if (UVMesh2 == Uv_.uv)
            {
                uvs2__ = mesh2.uv;
            }
            if (UVMesh2 == Uv_.uv2)
            {
                uvs2__ = mesh2.uv2;
            }
            if (UVMesh2 == Uv_.uv3)
            {
                uvs2__ = mesh2.uv3;
            }

            if (UV == Uv_.uv)
            {
                mesh2__uv = mesh.uv;
            }
            if (UV == Uv_.uv2)
            {
                mesh2__uv = mesh.uv2;
            }
            if (UV == Uv_.uv3)
            {
                mesh2__uv = mesh.uv3;
            }
            Newtangents = new Vector3[vertices.Length];
            Newnormals = new Vector3[vertices.Length];
            tangents = new Vector3[vertices.Length];
            tangents4 = new Vector4[vertices.Length];
            norm = new Vector3[vertices.Length];
            vert_ = new Vector3[vertices.Length];

            Calculate2_();

        }

        void Calculate2_()
        {
            if (mesh == null & GetComponent<SkinnedMeshRenderer>() != null)
            {
                mesh = GetComponent<SkinnedMeshRenderer>().sharedMesh;
            }

            for (int i = 0; i < vertices.Length; i++)
            {
                float dist__ = 10000;
                int index_ = 0;
                for (int i_ = 0; i_ < vertices2.Length; i_++)
                {
                    if (Vector2.Distance(mesh2__uv[i], uvs2__[i_]) < dist__) // check for matches
                    {
                        dist__ = Vector2.Distance(mesh2__uv[i], uvs2__[i_]);
                        index_ = i_;
                    }

                }
                newVert[i] = vertices2[index_];
                Newnormals[i] = normals[index_];
                Newtangents[i] = mesh2.tangents[index_];
            }
            norm = mesh.normals;
            for (int i__ = 0; i__ < vertices.Length; i__++)
            { // creating data for BlendShapes
                if (CreateBlendShapes == true)
                {
                    norm[i__] = Newnormals[i__] - norm[i__];
                    vert_[i__] = newVert[i__] - vertices[i__];
                    tangents[i__] = Newtangents[i__];
                    tangents4[i__] = Newtangents[i__];
                }

            }
            //mesh.ClearBlendShapes();
#if UNITY_EDITOR
            Mesh newnesh = (true) ? Object.Instantiate(mesh) as Mesh : mesh;
            newnesh.normals = Newnormals;
            newnesh.tangents = tangents4;
            newnesh.vertices = newVert;
            if (transform.parent)
            {
                AssetDatabase.CreateAsset(newnesh, "Assets/MeshTransformation/Meshes/" + transform.parent.name + transform.name + "_Recycled_" + "_.asset"); // save the modified model
            }
            else
            {
                AssetDatabase.CreateAsset(newnesh, "Assets/MeshTransformation/Meshes/" + transform.name + "_Recycled_" + "_.asset");
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            if (GetComponent<SkinnedMeshRenderer>() != null)
            {
                if (CreateBlendShapes == true)
                {
                    Mesh BlendShapeMesh = (true) ? Object.Instantiate(GetComponent<SkinnedMeshRenderer>().sharedMesh) as Mesh : GetComponent<SkinnedMeshRenderer>().sharedMesh;
                    BlendShapeMesh.AddBlendShapeFrame(NameBlendShape, 10.0f, vert_, norm, tangents);
                    mesh = BlendShapeMesh;
                    AssetDatabase.CreateAsset(BlendShapeMesh, "Assets/MeshTransformation/Meshes/" + transform.name + "_BlendShapes_" + "id" + Random.Range(0, 10000) + "_.asset");
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();

                }

            }

            if (GetComponent<MeshFilter>() != null)
            {
            }
            NewMesh_ = newnesh;

            buffer = new ComputeBuffer(mesh.vertexCount, 12);
            bufferNormal = new ComputeBuffer(mesh.normals.Length, 12);

            buffer.SetData(NewMesh_.vertices);
            bufferNormal.SetData(NewMesh_.normals);
            material.SetBuffer("_vert_", buffer);
            material.SetBuffer("_normal_", bufferNormal);

            if (Rig != null)
            {
                material.SetVector("RigPos", Rig.position);
                material.SetVector("posObj", transform.position);
            }
            if (MR != null)
            {
                MR.material = material;
            }
            if (SR != null)
            {
                SR.material = material;
            }
#endif
        }
    }
}