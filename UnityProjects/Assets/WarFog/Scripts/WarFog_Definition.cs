using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WarFog 
{
    /// <summary>
    ///definition of map element.
    /// </summary>
    public class WarFog_Map_Element 
    {
        public readonly int X;
        public readonly int Y;
        public readonly bool IsBlock;

        public WarFog_Map_Element(int pX, int pY, bool pIsBlock) 
        {
            this.X = pX;
            this.Y = pY;
            this.IsBlock = pIsBlock;
        }
    }

    /// <summary>
    ///definition of map element.
    /// </summary>
    public class WarFog_Element : PoolTypeInterface 
    {
        public int X { get; private set; }
        public int Y { get; private set; }
        public bool IsFog { get; private set; }

        public void setElement(int pX, int pY, bool pIsFog) {
            this.X = pX;
            this.Y = pY;
            this.IsFog = pIsFog;
        }

        public void reset() {
            this.X = -1;
            this.Y = -1;
            this.IsFog = true;
        }
    }

    /// <summary>
    ///definition of checkpoint in map.
    /// </summary>
    public class WarFog_CheckPoint : PoolTypeInterface 
    {
        public int X { get; private set; }
        public int Y { get; private set; }
        public int Radius { get; private set; }

        public WarFog_CheckPoint() { }

        public void setCheckPoint(int pX, int pY, int pRadius) {
            this.X = pX;
            this.Y = pY;
            this.Radius = pRadius;
        }

        public void reset() {
            this.X = -1;
            this.Y = -1;
            this.Radius = 0;
        }

        public static void clearLinkList(List<WarFog_CheckPoint> pPointList) 
        {
            for (int pointIndex = 0; pointIndex < pPointList.Count; ++pointIndex)
            {
                ScriptObjectPool<WarFog_CheckPoint>.getInstance().pushPoolObject(pPointList[pointIndex]);
            }

            pPointList.Clear();
        }
    }

    /// <summary>
    /// element for check point.
    /// </summary>
    public class WarFog_CheckPointStoreElement : PoolTypeInterface {
        public int X { get; private set; }
        public int Y { get; private set; }
        public int ValidDistance { get; private set; }
        public List<WarFog_Element> FogData { get; private set; }

        public void setData(int pX, int pY, int pValidDistance, List<WarFog_Element> pFogData) 
        {
            this.X = pX;
            this.Y = pY;
            this.ValidDistance = pValidDistance;
            this.FogData = pFogData;
        }

        public void reset() 
        {
            this.X = -1;
            this.Y = -1;
            this.ValidDistance = 0;

            if(this.FogData != null) {
                for (int fogIndex = 0; fogIndex < this.FogData.Count; ++fogIndex) {
                    ScriptObjectPool<WarFog_Element>.getInstance().pushPoolObject(this.FogData[fogIndex]);
                }
                this.FogData.Clear();
            } 
            this.FogData = null;
        }
    }
}
