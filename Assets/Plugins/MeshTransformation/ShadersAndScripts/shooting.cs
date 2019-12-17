using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MeshTransformation;
public class shooting : MonoBehaviour {
    public Transform CustomRayCastObject;

    public Transform MainObject;
    public GameObject[] ObjectsDestroy;
    public LayerMask LayerMask;
    public Material mat;
    ComputeBuffer vert_;
    ComputeBuffer uv_;
    ComputeBuffer colors_;
    public float DistDestroy;
    public ComputeShader _ComputeShader;
    public MeshFilter _MeshFilter;
    public SkinnedMeshRenderer _SkinnedMeshRenderer;
    public float Range = 0.3f;
    public float MinDistanceDamage;
    public float MaxDistanceDamage = 1;
    public Texture2D RangeDamageTexture;
       public Texture2D LimbLevelTexture; 
       public Texture2D _LimbIDTexture;
    public enum UvMaskTexture 
    {
        uv = 0,
        uv2 = 1,
        uv3 = 2
    }
    public UvMaskTexture uvR;

    public enum TypeMeshRenderer
    {
        SkinnedMeshRenderer = 0,
        MeshRenderer = 1,
    }
    public TypeMeshRenderer TMR;
    Mesh mesh;

                Vector3 Point_;
                Quaternion invertRotation;
    private void Start()
    {
        
        mesh = new Mesh();
        mat = GetComponent<TransformationMesh>().material;

        if (TMR == TypeMeshRenderer.SkinnedMeshRenderer)
        {
            _SkinnedMeshRenderer.BakeMesh(mesh);
        }
        else
        {
            mesh = _MeshFilter.mesh;
        }

        vert_ = new ComputeBuffer(mesh.vertexCount, sizeof(float) * 3); 
        colors_ = new ComputeBuffer(mesh.vertexCount, sizeof(float) * 4);
        uv_ = new ComputeBuffer(mesh.vertexCount, sizeof(float) * 2);
        


        if (uvR == UvMaskTexture.uv)
        {
            uv_.SetData(mesh.uv);
        }
        if (uvR == UvMaskTexture.uv2)
        {
            uv_.SetData(mesh.uv2);
        }
        if (uvR == UvMaskTexture.uv3)
        {
            uv_.SetData(mesh.uv3);
        }
    }

    void Update () {
        RaycastHit Hit = new RaycastHit();
        Ray Ray;
if (CustomRayCastObject == null) {
        Ray = Camera.main.ScreenPointToRay(Input.mousePosition);
}
else {
     Ray = new Ray(CustomRayCastObject.position, CustomRayCastObject.forward);
}
        if (Input.GetKeyDown(KeyCode.Mouse0) & Physics.Raycast(Ray, out Hit, 10000, LayerMask))
        {
            for (int i = 0;i < ObjectsDestroy.Length;i++)
            {
                if (ObjectsDestroy[i] != null) {
                    if (Vector3.Distance(ObjectsDestroy[i].transform.position, Hit.point) < DistDestroy) {
                        Destroy(ObjectsDestroy[i]);
                    }
                }
            }
if (vert_ != null)
        {
            vert_.Release();
            vert_.Dispose();
        }
            vert_ = new ComputeBuffer(mesh.vertexCount, sizeof(float) * 3);
            if (TMR == TypeMeshRenderer.SkinnedMeshRenderer)
            {
                _SkinnedMeshRenderer.BakeMesh(mesh);
            } else
            {
                mesh = _MeshFilter.mesh;
            }
            
            
            if (RangeDamageTexture != null) {

                _ComputeShader.SetTexture(0, "_RangeTexture", RangeDamageTexture);
            }
            else
            {
                _ComputeShader.SetTexture(0, "_RangeTexture", Texture2D.whiteTexture);
            } 
            
            if (LimbLevelTexture != null) {
                _ComputeShader.SetTexture(0, "_LimbLevel", LimbLevelTexture);
            }
            else
            {
                _ComputeShader.SetTexture(0, "_LimbLevel", Texture2D.blackTexture);
            }

            if (_LimbIDTexture != null) {
                _ComputeShader.SetTexture(0, "_LimbID", _LimbIDTexture);
                _ComputeShader.SetBool("Limb",true);
            }
                        else
            {
                _ComputeShader.SetBool("Limb",false);
            }
              
            Quaternion rotation = Quaternion.Euler(MainObject.eulerAngles.x, MainObject.eulerAngles.y, MainObject.eulerAngles.z);
           Vector3 invertPos;
           Vector3 invertScale;
           invertPos.x = MainObject.position.x * -1;
           invertPos.y = MainObject.position.y * -1;
           invertPos.z = MainObject.position.z * -1;
           invertScale.x = MainObject.localScale.x;
           invertScale.y = MainObject.localScale.y;
           invertScale.z = MainObject.localScale.z * -1;
           invertRotation.x = rotation.x * -1;
           invertRotation.y = rotation.y * -1;
           invertRotation.z = rotation.z * -1;
          Matrix4x4 m = Matrix4x4.TRS(Vector3.zero, rotation, MainObject.localScale);
vert_.SetData(mesh.vertices);

            Point_ = m.MultiplyPoint3x4(Hit.point);


            _ComputeShader.SetBuffer(0, "uv", uv_);
            _ComputeShader.SetBuffer(0, "vert", vert_);
            _ComputeShader.SetBuffer(0, "_colors", colors_);
            _ComputeShader.SetVector("PosHit", Hit.point);
            _ComputeShader.SetMatrix("_Matrix", m);
             _ComputeShader.SetVector("PosTransform", MainObject.position);
             _ComputeShader.SetInt("CountVertex", mesh.vertexCount);
            _ComputeShader.SetFloat("Range", Range);
            _ComputeShader.SetFloat("_Min", MinDistanceDamage);
            _ComputeShader.SetFloat("_Max", MaxDistanceDamage);
            _ComputeShader.Dispatch(0, mesh.vertices.Length / 512 + 1, 1, 1);

            vert_.Release(); 
            vert_.Dispose();
        }
        mat.SetBuffer("_colors", colors_);
    }
    void OnDisable()
    {
        if (vert_ != null)
        {
            vert_.Release();
            vert_.Dispose();
        }

    }
    void OnDestroy()
    {
        if (vert_ != null)
        {
            vert_.Release();
            vert_.Dispose();
        }
        if (colors_ != null)
        {
            colors_.Release();
            colors_.Dispose();
        }
        if (uv_ != null)
        {
            uv_.Release();
            uv_.Dispose();
        }
        
    }

}
