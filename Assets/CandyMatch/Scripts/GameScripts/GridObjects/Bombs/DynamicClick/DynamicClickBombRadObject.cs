﻿using System;
using System.Collections.Generic;
using UnityEngine;

namespace Mkey
{
    public class DynamicClickBombRadObject : DynamicClickBombObject
    {
        [SerializeField]
        private int radius = 2;

        #region temp vars
        //private BombObject r1;
        //private BombObject r2;
        #endregion temp vars

        #region override
        internal override void PlayExplodeAnimation(GridCell gCell, float delay, Action completeCallBack)
        {
            if (!gCell) completeCallBack?.Invoke();
            playExplodeTS = new TweenSeq();
            GameObject g = null;

            playExplodeTS.Add((callBack) => { delayAction(gameObject, delay, callBack); });

            playExplodeTS.Add((callBack) =>
            {
                g = Creator.InstantiateAnimPrefab(explodeAnimPrefab, gCell.transform, gCell.transform.position, SortingOrder.MainExplode);

                if (g)
                {
                    // g.transform.localScale = new Vector3(g.transform.localScale.x * scale, g.transform.localScale.y, 1);
                    delayAction(g, 0.3f, callBack);
                }
                else
                {
                    callBack?.Invoke();
                }
            });

            playExplodeTS.Add((callBack) =>
            {
                CollectEvent?.Invoke(TargetGroupID);
                completeCallBack?.Invoke();
                callBack();
            });

            playExplodeTS.Start();
        }

        public override CellsGroup GetArea(GridCell hitGridCell)
        {
            CellsGroup cG = new CellsGroup();
            List<GridCell> area = MBoard.MainGrid.GetAroundArea(hitGridCell, radius).Cells;
            cG.Add(hitGridCell);

            foreach (var item in area)
            {
                if (!item.IsDisabled) cG.Add(item); // (!item.Match)
            }

            return cG;
        }

        public override void ExplodeArea(GridCell gCell, float delay, bool showPrefab, bool hitProtection, Action completeCallBack)
        {
            Destroy(gameObject);
            explodePT = new ParallelTween();
            explodeTS = new TweenSeq();

            explodeTS.Add((callBack) => { delayAction(gCell.gameObject, delay, callBack); });

            // set hidden
            List<GridCell> area = GetArea(gCell).Cells;
            List<GridCell> areaFull = new List<GridCell>(area);
            areaFull.Add(gCell);
            MBoard.SetHiddenObject(areaFull);
            Vector3 mcPos;
            Transform mcTransform;
            GameObject mcGO;
            float distance;
            float t;
            foreach (GridCell mc in area) //parallel explode all cells
            {
                if (!mc) continue;
                mcPos = mc.transform.position;
                mcTransform = mc.transform;
                mcGO = mc.gameObject;

                t = 0;
                if (sequenced)
                {
                    distance = Vector2.Distance(mcPos, gCell.transform.position);
                    t = distance / 15f;
                }

                explodePT.Add((callBack) =>
                {
                    if (matchExplodePrefab)
                    {
                        delayAction(mcGO, t, () => { Instantiate(matchExplodePrefab, mcPos, Quaternion.identity, mcTransform); });
                    }
                    ExplodeCell(mc, t, showPrefab, hitProtection, callBack);
                });
            }

            explodeTS.Add((callBack) => { explodePT.Start(callBack); });
            explodeTS.Add((callBack) => { completeCallBack?.Invoke(); callBack(); });

            explodeTS.Start();
        }

        public override string ToString()
        {
            return "DynamicClickBombRad: " + ID;
        }
        #endregion override
    }
}