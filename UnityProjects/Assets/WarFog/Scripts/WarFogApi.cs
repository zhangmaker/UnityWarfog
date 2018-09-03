using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace WarFog {
    /// <summary>
    /// type of war-fog, if you choose nothing, the war-fog will not works.
    /// 1. war-fog is created by mesh api and rendering fog on triangles.
    /// </summary>
    public enum WarFogType {
        Nothing = 0,
        WarFog_Plane = 1,
    }

    /// <summary>
    /// Api for invoked by application.
    /// </summary>
    public class WarFogApi {
        #region GetInstance
        private static WarFogApi m_APIInstance = null;
        public static WarFogApi getInstance() {
            if(m_APIInstance == null) {
                m_APIInstance = new WarFogApi();
            }
            return m_APIInstance;
        }
        #endregion

        public WarFog_Caculate WarFogCaculater { get { return this.m_WarFogCaculater; } }
        private WarFog_Caculate m_WarFogCaculater = new WarFog_Caculate();

        public WarFog_Plane PlaneDisplayer { get { return this.m_PlaneDisplayer; } }
        private WarFog_Plane m_PlaneDisplayer = new WarFog_Plane();

        public WarFogType m_CurrentType = WarFogType.Nothing;
        private Transform m_ParentTransForPlaneWarFogTrans = null;

        private Vector3 m_OriginPos = Vector3.zero;
        private float m_WidthPerCell, m_HeightPerCell;
        private int m_H_Cell_Counts, m_V_Cell_Counts;

        /// <summary>
        /// init war fog api.
        /// </summary>
        /// <param name="pCurrentType">warfog type</param>
        /// <param name="pPostProcessMainCamera">Camera for postprocess model</param>
        /// <param name="pParentTransForPlaneWarFogTrans">Parent transform for plane model, if parment is null, the parment will world.</param>
        public void initWarFog(WarFogType pCurrentType, Transform pParentTransForPlaneWarFogTrans) {
            this.m_CurrentType = pCurrentType;
            this.m_ParentTransForPlaneWarFogTrans = pParentTransForPlaneWarFogTrans;

            if (this.m_CurrentType == WarFogType.WarFog_Plane && this.m_ParentTransForPlaneWarFogTrans == null) {
                Debug.LogError("You choose plane fog model but pParentTransForPlaneWarFogTrans is null, the warfog will not works!");
            }
        }

        /// <summary>
        /// set parments of warfog.
        /// </summary>
        /// <param name="pRenderMat">the material for render fog plane</param>
        /// <param name="pOriginPos">the origin of fog plane.</param>
        /// <param name="pWidthPerCell">width of cell</param>
        /// <param name="pHeightPerCell">height of cell</param>
        /// <param name="pH_Cell_Counts">the count of horizontal cells</param>
        /// <param name="pV_Cell_Counts">the count of veritacal cells</param>
        public void setParments(Material pRenderMat, Vector3 pOriginPos, float pWidthPerCell, float pHeightPerCell, int pH_Cell_Counts, int pV_Cell_Counts) {
            m_OriginPos = pOriginPos;
            m_WidthPerCell = pWidthPerCell;
            m_HeightPerCell = pHeightPerCell;
            m_H_Cell_Counts = pH_Cell_Counts;
            m_V_Cell_Counts = pV_Cell_Counts;

            if (this.m_CurrentType == WarFogType.WarFog_Plane) {
                PlaneDisplayer.setParments(this.m_ParentTransForPlaneWarFogTrans, pRenderMat, pOriginPos, pWidthPerCell, pHeightPerCell, pH_Cell_Counts, pV_Cell_Counts);
            }
        }

        /// <summary>
        /// update fog texture data
        /// </summary>
        /// <param name="pFogData">the fog texture for render</param>
        public void setFogData(int[,] pFogData) {
            if(this.m_CurrentType == WarFogType.WarFog_Plane) {
                PlaneDisplayer.setFogData(pFogData);
            }
        }

        /// <summary>
        /// update war-fog data
        /// </summary>
        public void updateWarFog() {

        }
    }
}
