/*
 * Copyright (C) 2006, Greg McIntyre
 * All rights reserved. See the file named COPYING in the distribution for more details.
 * the algorithm is based on Greg McIntyre's libfov:https://sourceforge.net/projects/libfov/
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace WarFog {
    public class WarFog_Caculate_Parments 
    {
        public const int MultiplyX = 10000;
    }

    public class WarFog_Caculate 
    {
        public bool[,] CurrentFogData;

        private bool[,] MapData;
        private int Width;
        private int Height;

        private Dictionary<int, WarFog_CheckPointStoreElement> m_StaticCheckPointDic = new Dictionary<int, WarFog_CheckPointStoreElement>();
        private List<WarFog_Element> m_StaticCheckData = new List<WarFog_Element>();

        /// <summary>
        /// init war fog caculater.
        /// </summary>
        /// <param name="pMapData">block data in map</param>
        /// <param name="pWidth">the width of map</param>
        /// <param name="pHeight">the height of map</param>
        public void SetMapData(bool[,] pMapData, int pWidth, int pHeight) 
        {
            this.MapData = pMapData;
            this.Width = pWidth;
            this.Height = pHeight;

            this.CurrentFogData = new bool[pWidth, pHeight];
            for (int x = 0; x < Width; ++x) {
                for (int y = 0; y < Height; ++y) {
                    CurrentFogData[x, y] = true;
                }
            }
        }

        /// <summary>
        /// add or update static check points.
        /// </summary>
        /// <param name="pPointList">the new list of checkpoints</param>
        public void AddOrUpdateStaticCheckPoints(List<WarFog_CheckPoint> pPointList) 
        {
            List<WarFog_CheckPoint> calulateList = new List<WarFog_CheckPoint>();

            for(int pointIndex = 0; pointIndex < pPointList.Count; ++pointIndex) {
                var pointNode = pPointList[pointIndex];
                int hashKey = pointNode.X * WarFog_Caculate_Parments.MultiplyX + pointNode.Y;

                if (!(m_StaticCheckPointDic.ContainsKey(hashKey) && m_StaticCheckPointDic[hashKey].ValidDistance == pointNode.Radius)) {
                    if (m_StaticCheckPointDic.ContainsKey(hashKey)) {
                        m_StaticCheckPointDic[hashKey].reset();
                        ScriptObjectPool<WarFog_CheckPointStoreElement>.getInstance().pushPoolObject(m_StaticCheckPointDic[hashKey]);
                        m_StaticCheckPointDic.Remove(hashKey);
                    }

                    var oneCheckPoint = ScriptObjectPool<WarFog_CheckPoint>.getInstance().getPoolObject();
                    oneCheckPoint.setCheckPoint(pointNode.X, pointNode.Y, pointNode.Radius);
                    calulateList.Add(oneCheckPoint);
                }
            }

            WarFog_CheckPoint.clearLinkList(pPointList);
            CaculateFogData(calulateList, m_StaticCheckPointDic);
            assemblyStaticCheckPoints(m_StaticCheckPointDic, m_StaticCheckData);
        }

        /// <summary>
        /// remove not used static checkpoints.
        /// </summary>
        /// <param name="pPointList"></param>
        public void RemoveStaticCheckPoints(List<WarFog_CheckPoint> pPointList) 
        {
            for (int pointIndex = 0; pointIndex < pPointList.Count; ++pointIndex)
            {
                var pointNode = pPointList[pointIndex];
                int hashKey = pointNode.X * WarFog_Caculate_Parments.MultiplyX + pointNode.Y;
                if (m_StaticCheckPointDic.ContainsKey(hashKey)) {
                    m_StaticCheckPointDic[hashKey].reset();
                    ScriptObjectPool<WarFog_CheckPointStoreElement>.getInstance().pushPoolObject(m_StaticCheckPointDic[hashKey]);
                    m_StaticCheckPointDic.Remove(hashKey);
                }
            }

            WarFog_CheckPoint.clearLinkList(pPointList);
            assemblyStaticCheckPoints(m_StaticCheckPointDic, m_StaticCheckData);
        }

        /// <summary>
        /// set dynamic checkpoints.
        /// </summary>
        /// <param name="pPointList">the new list of checkpoints</param>
        public void SetDynamicCheckPoints(List<WarFog_CheckPoint> pPointList) 
        {
            this.clearFogData();

            //assemble static fog.
            for(int fogIndex = 0; fogIndex < this.m_StaticCheckData.Count; ++fogIndex) {
                var oneFogElement = this.m_StaticCheckData[fogIndex];
                this.CurrentFogData[oneFogElement.X, oneFogElement.Y] = oneFogElement.IsFog;
            }

            CaculateFogData(pPointList, null);
        }

        public void CaculateFogData(List<WarFog_CheckPoint> pCaculateList, Dictionary<int, WarFog_CheckPointStoreElement> pStoreDic) 
        {
            for (int pointIndex = 0; pointIndex < pCaculateList.Count; ++pointIndex)
            {
                var pointNode = pCaculateList[pointIndex];
                if (pStoreDic != null) {
                    int hashKey = pointNode.X * WarFog_Caculate_Parments.MultiplyX + pointNode.Y;
                    var oneCheckPointElement = ScriptObjectPool<WarFog_CheckPointStoreElement>.getInstance().getPoolObject();
                    var oneWarElement = ScriptObjectPool<WarFog_Element>.getInstance().getPoolObject();
                    oneWarElement.setElement(pointNode.X, pointNode.Y, false);
                    List<WarFog_Element> fogData = new List<WarFog_Element>() { oneWarElement };
                    oneCheckPointElement.setData(pointNode.X, pointNode.Y, pointNode.Radius, fogData);
                    pStoreDic.Add(hashKey, oneCheckPointElement);
                } else {
                    this.CurrentFogData[pointNode.X, pointNode.Y] = false;
                }

                CaculateOneCorner(pointNode, 1, 1, 1, true, 0, 1.0f, true, true, pStoreDic);
                CaculateOneCorner(pointNode, 1, 1, 1, false, 0, 1.0f, true, true, pStoreDic);
                CaculateOneCorner(pointNode, 1, 1, -1, true, 0, 1.0f, true, true, pStoreDic);
                CaculateOneCorner(pointNode, 1, 1, -1, false, 0, 1.0f, true, true, pStoreDic);
                CaculateOneCorner(pointNode, 1, -1, 1, true, 0, 1.0f, true, true, pStoreDic);
                CaculateOneCorner(pointNode, 1, -1, 1, false, 0, 1.0f, true, true, pStoreDic);
                CaculateOneCorner(pointNode, 1, -1, -1, true, 0, 1.0f, true, true, pStoreDic);
                CaculateOneCorner(pointNode, 1, -1, -1, false, 0, 1.0f, true, true, pStoreDic);
            }

            WarFog_CheckPoint.clearLinkList(pCaculateList);
        }

        private void CaculateOneCorner(WarFog_CheckPoint pCheckPoint, int pStepDistance, int pSignX, int pSignY, bool pIsAlongX, 
                                        float pStartSlope, float pEndSlope, bool pCaculateDiagonal, 
                                        bool pCaculateEdge, Dictionary<int, WarFog_CheckPointStoreElement> pStoreDic) {
            int x = 0;
            int y = 0;
            int dy, dy0, dy1;
            uint h = 0;
            int prev_blocked = -1;
            float end_slope_next;

            if (pStepDistance == 0) {
                CaculateOneCorner(pCheckPoint, pStepDistance + 1, pSignX, pSignY, pIsAlongX, pStartSlope, pEndSlope, pCaculateDiagonal, pCaculateEdge, pStoreDic);
                return;
            } else if ((uint)pStepDistance > pCheckPoint.Radius) {
                return;
            }

            dy0 = (int)(0.5f + ((float)pStepDistance) * pStartSlope);                                            
            dy1 = (int)(0.5f + ((float)pStepDistance) * pEndSlope);

            if (pIsAlongX) {
                x = pCheckPoint.X + pSignX * pStepDistance;
                y = pCheckPoint.Y + pSignY * dy0;
            } else {
                y = pCheckPoint.Y + pSignX * pStepDistance;
                x = pCheckPoint.X + pSignY * dy0;
            }                                                 
                                                                                                    
            if (!pCaculateDiagonal && dy1 == pStepDistance) {                                                         
                /* We do diagonal lines on every second octant, so they don't get done twice. */    
                --dy1;                                                                              
            }

            h = (uint)Mathf.Sqrt((float)((pCheckPoint.Radius+0.5f) * (pCheckPoint.Radius + 0.5f) - pStepDistance * pStepDistance));
            if ((uint)dy1 > h) {
                if (h == 0) {
                    return;
                }
                dy1 = (int)h;
            }

            for (dy = dy0; dy <= dy1; ++dy) {
                if(pIsAlongX) {
                    y = pCheckPoint.Y + pSignY * dy;
                } else {
                    x = pCheckPoint.X + pSignY * dy;
                }                                                
                                                                                                    
                if (this.isMapOpaque(x, y)) {                                            
                    if (pCaculateEdge || dy > 0) {     
                        //settings->apply(data->map, x, y, pStepDistance, dy, data->source);     
                        if(pStoreDic == null) {

                        }
                    }                                                                               
                    if (prev_blocked == 0) {                                                        
                        end_slope_next = caculateSlope((float)pStepDistance + 0.5f, (float)dy - 0.5f);
                        CaculateOneCorner(pCheckPoint, pStepDistance + 1, pSignX, pSignY, pIsAlongX, pStartSlope, end_slope_next, pCaculateDiagonal, pCaculateEdge, pStoreDic);
                    }                                                                               
                    prev_blocked = 1;                                                               
                } else {                                                                            
                    if (pCaculateEdge || dy > 0) {                                                     
                        //settings->apply(data->map, x, y, dx, dy, data->source);
                        if (pStoreDic != null) {
                            int hashKey = pCheckPoint.X * WarFog_Caculate_Parments.MultiplyX + pCheckPoint.Y;
                            var oneWarElement = ScriptObjectPool<WarFog_Element>.getInstance().getPoolObject();
                            oneWarElement.setElement(x, y, false);

                            pStoreDic[hashKey].FogData.Add(oneWarElement);
                        } else {
                            this.CurrentFogData[x, y] = false;
                        }
                    }                                                                               
                    if (prev_blocked == 1) {
                        pStartSlope = caculateSlope((float)pStepDistance - 0.5f, (float)dy - 0.5f);         
                    }                                                                               
                    prev_blocked = 0;                                                               
                }                                                                                   
            }                                                                                       
                                                                                                    
            if (prev_blocked == 0) {                                                                    
                CaculateOneCorner(pCheckPoint, pStepDistance +1, pSignX, pSignY, pIsAlongX, pStartSlope, pEndSlope, pCaculateDiagonal, pCaculateEdge, pStoreDic);
            }
        }

        private float caculateSlope(float dx, float dy) {
            if(dx <= -Mathf.Epsilon || dx >= Mathf.Epsilon) {
                return dy/dx;
            }

            return 0;
        }

        private void assemblyStaticCheckPoints(Dictionary<int, WarFog_CheckPointStoreElement> pStoreDic, List<WarFog_Element> pCheckData) {
            //clear old data.
            for(int checkIndex = 0; checkIndex < pCheckData.Count; ++checkIndex) {
                ScriptObjectPool<WarFog_Element>.getInstance().pushPoolObject(pCheckData[checkIndex]);
            }
            pCheckData.Clear();

            //assembly
            foreach(var oneCheckPoint in pStoreDic) {
                pCheckData.AddRange(oneCheckPoint.Value.FogData);
                oneCheckPoint.Value.FogData.Clear();
                oneCheckPoint.Value.reset();

                ScriptObjectPool<WarFog_CheckPointStoreElement>.getInstance().pushPoolObject(oneCheckPoint.Value);
            }
            pStoreDic.Clear();
        }

        private bool isMapOpaque(int pX, int pY) 
        {
            if(pX < 0 || pY < 0 || pX >= this.Width || pY >= this.Height) {
                return true;
            }

            return this.MapData[pX, pY];
        }

        private void clearFogData() {
            for(int x = 0; x < Width; ++x) {
                for(int y = 0; y < Height; ++y) {
                    CurrentFogData[x, y] = true;
                }
            }
        }
    }
}
