using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WarFog {
    public class WarFog_Plane_Config {
        /// <summary>
        /// the interval of fade fog from old data to new data.
        /// </summary>
        public const float FadeInterval = 1.0f;
    }

    public class WarFog_Plane {
        private Transform m_ParentTrans = null;
        private Material m_RenderMat = null;
        private Texture2D m_FogTex = null;

        private int[,] currentFogData = null;
        private Vector3 m_OriginPos = Vector3.zero;
        private float m_WidthPerCell, m_HeightPerCell;
        private int m_H_Vertices_Counts, m_V_Vertices_Counts;

        private bool m_FadeTimeValid = true;
        private float m_FadeStartTime = Time.unscaledTime;

        public void setParments(Transform pParentTrans, Material pRenderMat, Vector3 pOriginPos, float pWidthPerCell, float pHeightPerCell, int pH_Cell_Counts, int pV_Cell_Counts) {
            this.m_ParentTrans = pParentTrans;
            this.m_RenderMat = pRenderMat;
            this.m_OriginPos = pOriginPos;
            this.m_WidthPerCell = pWidthPerCell;
            this.m_HeightPerCell = pHeightPerCell;
            this.m_H_Vertices_Counts = pH_Cell_Counts + 1;
            this.m_V_Vertices_Counts = pV_Cell_Counts + 1;
            this.currentFogData = new int[pH_Cell_Counts, pV_Cell_Counts];
            for(int vIndex = 0; vIndex < pV_Cell_Counts; ++vIndex) {
                for(int hIndex = 0; hIndex < pH_Cell_Counts; ++hIndex) {
                    this.currentFogData[vIndex, hIndex] = 1;
                }
            }

            this.buildWarFog();
        }

        private void buildWarFog() {
            //create game object.
            GameObject fogObject = new GameObject("WarFog_Plane");
            fogObject.transform.SetParent(this.m_ParentTrans);

            fogObject.AddComponent<MeshFilter>();
            fogObject.AddComponent<MeshRenderer>();
            fogObject.GetComponent<Renderer>().material = this.m_RenderMat;

            //Build fog mesh.
            Mesh fogMesh = new Mesh();
            fogObject.GetComponent<MeshFilter>().mesh = fogMesh;

            //Build positions and uv for vertices.
            Vector3[] fogVertices = new Vector3[this.m_H_Vertices_Counts * this.m_V_Vertices_Counts];
            Vector2[] fogUV = new Vector2[fogVertices.Length];

            Vector2 uvScale = new Vector2(1.0f/ (this.m_H_Vertices_Counts-1), 1.0f / (this.m_V_Vertices_Counts-1));

            for(int hIndex = 0; hIndex < this.m_V_Vertices_Counts; ++hIndex) {
                for(int vIndex = 0; vIndex < this.m_H_Vertices_Counts; ++vIndex) {
                    fogVertices[hIndex * this.m_H_Vertices_Counts + vIndex] = this.m_OriginPos + 
                        (new Vector3(this.m_WidthPerCell*vIndex, 0, this.m_HeightPerCell*hIndex));
                    fogUV[hIndex * this.m_H_Vertices_Counts + vIndex] = Vector2.Scale(uvScale, (new Vector2(this.m_WidthPerCell * vIndex, this.m_HeightPerCell * hIndex)));
                }
            }
            fogMesh.vertices = fogVertices;
            fogMesh.uv = fogUV;

            // Build triangle indices: 3 indices into vertex array for each triangle
            var triangles = new int[(m_V_Vertices_Counts - 1) * (m_H_Vertices_Counts - 1) * 6];
            var tIndex = 0;
            for (int y = 0; y < m_V_Vertices_Counts - 1; y++) {
                for (int x = 0; x < m_H_Vertices_Counts - 1; x++) {
                    // For each grid cell output two triangles
                    triangles[tIndex++] = (y * m_H_Vertices_Counts) + x;
                    triangles[tIndex++] = ((y + 1) * m_H_Vertices_Counts) + x;
                    triangles[tIndex++] = (y * m_H_Vertices_Counts) + x + 1;

                    triangles[tIndex++] = ((y + 1) * m_H_Vertices_Counts) + x;
                    triangles[tIndex++] = ((y + 1) * m_H_Vertices_Counts) + x + 1;
                    triangles[tIndex++] = (y * m_H_Vertices_Counts) + x + 1;
                }
            }
            // And assign them to the mesh
            fogMesh.triangles = triangles;

            fogMesh.RecalculateNormals();

            //set material parments.
            this.m_RenderMat.SetFloat("_Width", m_H_Vertices_Counts - 1);
            this.m_RenderMat.SetFloat("_Height", m_V_Vertices_Counts - 1);

            //init fog texture.
            this.m_FogTex = new Texture2D(m_H_Vertices_Counts - 1, m_V_Vertices_Counts - 1);
            this.m_FogTex.wrapMode = TextureWrapMode.Clamp;
            this.m_RenderMat.mainTexture = this.m_FogTex;
        }

        public void setFogData(int[,] pData) {
            //bool isNotEqual = false;
            //for (int y = 0; y < m_V_Vertices_Counts - 1; ++y) {
            //    for (int x = 0; x < m_H_Vertices_Counts - 1; ++x) {
            //        if(currentFogData[y, x] != pData[y, x]) {
            //            isNotEqual = true;
            //            break;
            //        }
            //    }
            //    if(isNotEqual) {
            //        break;
            //    }
            //}

            //if(isNotEqual) {
                for (int y = 0; y < m_V_Vertices_Counts - 1; ++y) {
                    for (int x = 0; x < m_H_Vertices_Counts - 1; ++x) {
                        this.m_FogTex.SetPixel(x, y, new Color(currentFogData[y, x], pData[y, x], 0, 1));
                        currentFogData[y, x] = pData[y, x];
                    }
                }
                this.m_FogTex.Apply();
                this.m_RenderMat.SetFloat("_FadeTime", 0);
                this.m_FadeTimeValid = true;
                this.m_FadeStartTime = Time.unscaledTime;
            //}
        }

        public void updateFog() {
            if(this.m_FadeTimeValid) {
                float currentFadeTime = (Time.unscaledTime - this.m_FadeStartTime) / WarFog_Plane_Config.FadeInterval;
                if (currentFadeTime > 1.0f) {
                    currentFadeTime = 1.0f;
                    this.m_FadeTimeValid = false;
                }

                this.m_RenderMat.SetFloat("_FadeTime", currentFadeTime);
            }
        }
    }
}
