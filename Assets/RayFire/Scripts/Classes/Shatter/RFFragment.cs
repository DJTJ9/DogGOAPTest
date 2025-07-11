﻿using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

// https://docs.unity3d.com/2021.3/Documentation/Manual/PlatformDependentCompilation.html
#if (UNITY_EDITOR || UNITY_STANDALONE || UNITY_IOS || UNITY_ANDROID || UNITY_XBOXONE)
using RayFire.DotNet;

namespace RayFire
{
    // Static class to handle all shatter methods
    public static class RFFragment
    {
        static bool                       silentMode       = true;
        static List<Mesh>                 meshListStatic   = new List<Mesh>();
        static List<Vector3>              pivotListStatic  = new List<Vector3>();
        static List<Dictionary<int, int>> subIdsListStatic = new List<Dictionary<int, int>>();

        /// /////////////////////////////////////////////////////////
        /// Shatter
        /// /////////////////////////////////////////////////////////

        // Cache for shatter
        public static RFShatter CacheMeshes(ref Mesh[] meshes, ref Vector3[] pivots, ref RFDictionary[] rfOrigSubMeshIds, RayfireShatter scrShatter)
        {
            // TODO check vars by type: slice list, etc

            // Min face filter for internal slice ops
            int removeMinFaceFilter = 4;

            // Turn off fast mode for tets and slices:
            // 0:classic:      Custom, tets, clustering: old original algorithm. 
            // 1:voro simple:  Decompose type or Rigid without Shatter.
            // 2:slice
            // 3:bricks
            int shatterMode = GetShatterMode (scrShatter);

            // Get mesh
            Mesh mesh = GetDemolitionMesh (scrShatter);

            // Decompose in Editor only, slice runtime only
            // Runtime = 0,
            // Editor  = 1
            FragmentMode fragmentMode = scrShatter.mode;
            if (scrShatter.type == FragType.Decompose) // TODO FIX
                fragmentMode = FragmentMode.Editor;
            if (scrShatter.type == FragType.Slices)
            {
                fragmentMode        = FragmentMode.Runtime;
                removeMinFaceFilter = 1; // IMPORTANT should be 1 for slices in order to cut tree like objects with a lot of disconnected planar leafs
            }

            // Set up shatter
            RFShatter shatter = SetShatter (
                fragmentMode,
                shatterMode,
                mesh,
                scrShatter.transform,
                scrShatter.material,
                scrShatter.advanced.dec,
                scrShatter.advanced.coll,
                scrShatter.advanced.Seed,
                scrShatter.advanced.inpCap,
                scrShatter.advanced.outCap,
                scrShatter.advanced.dbl,
                scrShatter.advanced.inner,
                scrShatter.advanced.smooth,
                scrShatter.advanced.element,
                removeMinFaceFilter,
                scrShatter.advanced.weld);

            // Failed input
            if (shatter == null)
            {
                meshes = null;
                pivots = null;
                return null;
            }

            // Get innerSubId
            int innerSubId = RFSurface.SetInnerSubId (scrShatter);

            // Set fragmentation properties
            SetShatterFragmentProperties (shatter, scrShatter);

            // Custom/HexGrid points check
            if (NoCustomPoints (scrShatter) == true)
            {
                RayfireMan.Log (RFLog.sht_dbgn + scrShatter.name + RFLog.sht_noPnt, scrShatter.gameObject);
                meshes = null;
                pivots = null;
                return null;
            }

            // Calculate fragments
            List<Dictionary<int, int>> origSubMeshIds = new List<Dictionary<int, int>>();
            bool successState = Compute (
                shatterMode,
                shatter,
                scrShatter.transform,
                ref meshes,
                ref pivots,
                mesh,
                innerSubId,
                ref origSubMeshIds,
                scrShatter);
            
            // Create RF dictionary
            if (origSubMeshIds != null)
                rfOrigSubMeshIds = RFDictionary.GetRFDictionary (origSubMeshIds);

            // Failed fragmentation
            if (successState == false)
            {
                RayfireMan.Log (RFLog.sht_dbgn + scrShatter.name + RFLog.sht_bad1, scrShatter.gameObject);
                return null;
            }

            // Filter out planar meshes
            RFShatterAdvanced.RemovePlanar (ref meshes, ref pivots, ref rfOrigSubMeshIds, scrShatter);

            // Filter out meshes by size
            RFShatterAdvanced.RemoveBySize (ref meshes, ref pivots, ref rfOrigSubMeshIds, scrShatter);

            // Rename meshes
            string nameApp = scrShatter.name + "_";
            for (int i = 0; i < meshes.Length; i++)
                meshes[i].name = nameApp + i;

            /*
            // Bake collider meshes
            if (scrShatter.advanced.bake == true)
            {
                Debug.Log ("bake");
                for (int i = 0; i < meshes.Length; i++)
                    Physics.BakeMesh (meshes[i].GetInstanceID(), true);
            }
            */

            return shatter;
        }

        // Check for no custom or hex grid points
        static bool NoCustomPoints(RayfireShatter scrShatter)
        {
            if (scrShatter.type == FragType.Custom && scrShatter.custom.noPoints == true)
                return true;
            if (scrShatter.type == FragType.Hexagon && scrShatter.hexagon.noPc == true)
                return true;
            return false;
        }
        
        /// /////////////////////////////////////////////////////////
        /// Rigid
        /// /////////////////////////////////////////////////////////

        // Prepare rigid component to cache fragment meshes
        public static bool InputMesh(RayfireRigid scr)
        {
            // Set up shatter
            if (SetRigidShatter (scr) == false)
                return false;

            // Get innerSubId
            scr.mshDemol.innerSubId = RFSurface.SetInnerSubId (scr);

            // Set fragmentation properties
            SetRigidFragmentProperties (scr.mshDemol.rfShatter, scr.mshDemol.sht, scr);

            return true;
        }

        // Set up rigid shatter
        static bool SetRigidShatter(RayfireRigid scr)
        {
            // Set up shatter
            if (scr.mshDemol.rfShatter == null)
            {
                // Save rotation at caching to fix fragments rotation at demolition
                scr.chRot = scr.tsf.rotation;

                // Turn off fast mode for tets and slices
                scr.mshDemol.shatterMode = GetShatterMode (scr.mshDemol.sht);

                // Get innerSubId
                scr.mshDemol.mesh = GetDemolitionMesh (scr);

                // Get shatter
                scr.mshDemol.rfShatter = SetShatter (
                    FragmentMode.Runtime,
                    scr.mshDemol.shatterMode,
                    scr.mshDemol.mesh,
                    scr.transform,
                    scr.materials,
                    scr.mshDemol.prp.dec, // decompose
                    scr.mshDemol.prp.rem, // removeCollinear
                    scr.mshDemol.sd,      // seed
                    scr.mshDemol.prp.cap, // input precap
                    false,                      // output precap
                    false,                      // removeDoubleFaces
                    false,                      // inner fragments
                    false,                      // smooth
                    3,                          // elementSizeThreshold
                    4,                          // RemoveMinFaceFilter
                    true                        // postWeld
                    );
            }

            // Failed input. Instant bad mesh.
            if (scr.mshDemol.rfShatter == null)
            {
                scr.lim.demolitionShould =  false;
                scr.mshDemol.badMesh             += 10;
                scr.mshDemol.mesh                =  null;
                return false;
            }

            return true;
        }

        // Cache for rigid
        public static void CacheMeshesInst(ref Mesh[] meshes, ref Vector3[] pivots, ref RFDictionary[] rfOrigSubMeshIds, RayfireRigid scrRigid)
        {
            // Local data lists
            List<Dictionary<int, int>> origSubMeshIds = new List<Dictionary<int, int>>();

            // Calculate fragments
            bool successState = Compute (
                scrRigid.mshDemol.shatterMode,
                scrRigid.mshDemol.rfShatter,
                scrRigid.transform,
                ref meshes,
                ref pivots,
                scrRigid.mshDemol.mesh,
                scrRigid.mshDemol.innerSubId,
                ref origSubMeshIds,
                scrRigid);

            // Create RF dictionary
            if (origSubMeshIds != null)
                rfOrigSubMeshIds = RFDictionary.GetRFDictionary (origSubMeshIds);

            // Failed fragmentation. Increase bad mesh 
            if (successState == false)
            {
                scrRigid.mshDemol.badMesh++;
                RayfireMan.Log ("Bad mesh: " + scrRigid.name);
            }

            // Mesh naming
            else
                for (int i = 0; i < meshes.Length; i++)
                    meshes[i].name = i.ToString();
        }

        // Cache for rigid
        public static void CacheMeshesMult(Transform tmSaved, ref List<Mesh> meshesList, ref List<Vector3> pivotsList, ref List<RFDictionary> subList, RayfireRigid scrRigid, List<int> batchAmount, int batchInd)
        {
            // Get list of meshes to calc
            List<int> markedElements = RFRuntimeCaching.GetMarkedElements (batchInd, batchAmount);

            // Local iteration data lists
            Mesh[]                     meshesLocal    = new Mesh[batchAmount.Count];
            Vector3[]                  pivotsLocal    = new Vector3[batchAmount.Count];
            List<Dictionary<int, int>> origSubMeshIds = new List<Dictionary<int, int>>();

            // Compute
            bool state = scrRigid.mshDemol.rfShatter.SimpleCompute (
                tmSaved,
                ref meshesLocal,
                ref pivotsLocal,
                scrRigid.mshDemol.mesh,
                scrRigid.mshDemol.innerSubId,
                ref origSubMeshIds,
                markedElements,
                batchInd == 0);

            // Set names
            if (state == false || meshesLocal == null || meshesLocal.Length == 0)
                return;

            // Set names
            for (int i = 0; i < meshesLocal.Length; i++)
            {
                meshesLocal[i].RecalculateTangents();
                meshesLocal[i].name = scrRigid.name + "_fr"; // + markedElements[i].ToString();
            }

            // Add data to main lists
            for (int i = 0; i < origSubMeshIds.Count; i++)
                subList.Add (new RFDictionary (origSubMeshIds[i]));

            meshesList.AddRange (meshesLocal);
            pivotsList.AddRange (pivotsLocal);
        }

        // Get demolition mesh
        static Mesh GetDemolitionMesh(RayfireRigid scr)
        {
            if (scr.skr != null)
                return RFMesh.BakeMesh (scr.skr);
            return scr.mFlt.sharedMesh;
        }

        // Get demolition mesh
        static Mesh GetDemolitionMesh(RayfireShatter scr)
        {
            // Multymesh fragmentation
            if (scr.advanced.children == true && scr.meshFilters.Count > 0)
                return RFCombineMesh.CombineShatter (scr, scr.transform, scr.meshFilters);

            // Skinned mesh
            if (scr.skinnedMeshRend != null)
                return RFMesh.BakeMesh (scr.skinnedMeshRend);

            return scr.meshFilter.sharedMesh;
        }

        /// /////////////////////////////////////////////////////////
        /// Slice
        /// /////////////////////////////////////////////////////////

        // Cache for slice
        public static void SliceMeshes(ref Mesh[] meshes, ref Vector3[] pivots, ref RFDictionary[] rfOrigSubMeshIds, RayfireRigid scr, List<Vector3> sliceData)
        {
            // Get mesh
            scr.mshDemol.mesh = GetDemolitionMesh (scr);

            // Set up shatter
            RFShatter shatter = SetShatter (
                FragmentMode.Runtime,
                2,                          // shatterMode. 2 for slices
                scr.mshDemol.mesh,
                scr.transform,
                scr.materials,
                scr.mshDemol.prp.dec, // decompose
                scr.mshDemol.prp.rem, // removeCollinear
                scr.mshDemol.sd,      // seed
                scr.mshDemol.prp.cap, // inputPrecap
                false,                      // outputPrecap. Do not cap for tree like objects
                false,                      // removeDoubleFaces
                false,                      // inner
                false,                      // smooth
                3,                          // ElementSizeThreshold
                1,                          // RemoveMinFaceFilter. // IMPORTANT should be 1 for slices in order to cut tree like objects
                true                        // postWeld
                );
            
            // Failed input
            if (shatter == null)
            {
                meshes = null;
                pivots = null;
                scr.mshDemol.badMesh++;
                return;
            }

            // Get innerSubId
            scr.mshDemol.innerSubId = RFSurface.SetInnerSubId (scr);

            // Get slice data
            List<Vector3> points = new List<Vector3> (sliceData.Count / 2);
            List<Vector3> norms  = new List<Vector3> (sliceData.Count / 2);
            for (int i = 0; i < sliceData.Count; i++)
            {
                points.Add (sliceData[i]);
                norms.Add (sliceData[i + 1]);
                i++;
            }
            
            // Set params
            shatter.SetBricksParams (points.ToArray(), norms.ToArray(), scr.transform);

            // Calculate fragments
            List<Dictionary<int, int>> origSubMeshIds = new List<Dictionary<int, int>>();
            bool successState = Compute (
                2,  // shatterMode. 2 for slices
                shatter,
                scr.transform,
                ref meshes,
                ref pivots,
                scr.mshDemol.mesh,
                scr.mshDemol.innerSubId,
                ref origSubMeshIds,
                scr);

            // Create RF dictionary
            if (origSubMeshIds != null)
                rfOrigSubMeshIds = RFDictionary.GetRFDictionary (origSubMeshIds);

            // Failed fragmentation. Increase bad mesh 
            if (successState == false)
            {
                scr.meshes = null;
                scr.mshDemol.badMesh++;
                RayfireMan.Log (RFLog.sht_dbgn + scr.name + RFLog.sht_bad2, scr.gameObject);
                return;
            }
      
            // Rename meshes
            for (int i = 0; i < meshes.Length; i++)
                meshes[i].name = i.ToString();
        }

        /// /////////////////////////////////////////////////////////
        /// Compute
        /// /////////////////////////////////////////////////////////

        // Compute
        static bool Compute(int shatterMode, RFShatter shatter, Transform tm, ref Mesh[] meshes, ref Vector3[] pivots,
            Mesh mesh, int innerSubId, ref List<Dictionary<int, int>> origSubMeshIDs, Object obj, List<int> markedElements = null)
        {
            // Compute fragments
            bool state = shatterMode == 0
                ? shatter.Compute (tm, ref meshes, ref pivots, mesh, innerSubId, ref origSubMeshIDs)
                : shatter.SimpleCompute (tm, ref meshes, ref pivots, mesh, innerSubId, ref origSubMeshIDs, markedElements);
            
            // Mesh singleMesh = RFShatter.WeldMeshes (ref meshes, ref pivots, ref origSubMeshIDs, 0.5f);
            
            // Failed fragmentation
            if (state == false)
            {
                meshes         = null;
                pivots         = null;
                origSubMeshIDs = null;
                return false;
            }

            // Null check
            if (meshes == null)
            {
                RayfireMan.Log (RFLog.sht_dbgn + tm.name + RFLog.sht_null, tm.gameObject);
                meshes         = null;
                pivots         = null;
                origSubMeshIDs = null;
                return false;
            }

            // Empty mesh fix
            if (EmptyMeshState (meshes) == true)
            {
                for (int i = 0; i < meshes.Length; i++)
                {
                    if (meshes[i].vertexCount > 2)
                    {
                        meshListStatic.Add (meshes[i]);
                        pivotListStatic.Add (pivots[i]);
                        subIdsListStatic.Add (origSubMeshIDs[i]);
                    }
                }

                pivots         = pivotListStatic.ToArray();
                meshes         = meshListStatic.ToArray();
                origSubMeshIDs = subIdsListStatic;
                meshListStatic.Clear();
                pivotListStatic.Clear();
                subIdsListStatic.Clear();
                RayfireMan.Log (RFLog.sht_dbgn + tm.name + RFLog.sht_empt, tm.gameObject);
            }

            // Single mesh after mesh fix check
            if (meshes.Length <= 1)
            {
                RayfireMan.Log (RFLog.sht_dbgn + tm.name + RFLog.sht_low + meshes.Length,  tm.gameObject);
                meshes         = null;
                pivots         = null;
                origSubMeshIDs = null;
                return false;
            }

            // Post
            for (int i = 0; i < meshes.Length; i++)
            {
                //Debug.Log(meshes[i].vertexCount);
                //meshes[i].indexFormat = IndexFormat.UInt32;
               
                meshes[i].RecalculateTangents();
            }

            return true;
        }

        // Get shatter mode
        // 0:classic:      Custom, tets, clustering: old original algorithm. 
        // 1:voro simple:  Decompose type or Rigid without Shatter.
        // 2:slice
        // 3:bricks
        static int GetShatterMode(RayfireShatter scrShatter = null)
        {
            // Voro Simple mode: for rigid without shatter
            if (scrShatter == null)
                return 1;

            // Brick Simple mode
            if (scrShatter.type == FragType.Slices)
                return 2;

            // Voro Simple mode
            if (scrShatter.type == FragType.Decompose)
                return 1;

            // Voro Simple mode
            if (scrShatter.type == FragType.Bricks || scrShatter.type == FragType.Voxels)
                if (scrShatter.type == FragType.Bricks || scrShatter.type == FragType.Voxels)
                    return 3;

            // Classic mode
            int shatterMode = scrShatter.shatterMode;
            if (scrShatter.type == FragType.Hexagon)
                shatterMode = 0;
            if (scrShatter.type == FragType.Custom)
                shatterMode = 0;
            //if (scrShatter.type == FragType.Mirrored) 
            //    shatterMode = 0;
            if (scrShatter.type == FragType.Tets)
                shatterMode = 0;

            // Classic way for clustering. Not for slices
            if (scrShatter.clusters.enable == true)
                shatterMode = 0;

            return shatterMode;
        }

        // Check for at least one empty mesh in cached meshes
        static bool EmptyMeshState(Mesh[] meshes)
        {
            for (int i = 0; i < meshes.Length; i++)
                if (meshes[i].vertexCount <= 2)
                    return true;
            return false;
        }

        /// /////////////////////////////////////////////////////////
        /// Set properties by fragment type
        /// /////////////////////////////////////////////////////////

        // Set fragmentation properties
        static void SetShatterFragmentProperties(RFShatter shatter, RayfireShatter scrSh)
        {
            // Center position from local to global
            Vector3 centerPos = scrSh.transform.TransformPoint (scrSh.centerPosition);

            // Clustering
            if (scrSh.clusters.enable == true)
                SetClusters (shatter, scrSh.clusters);

            // Set properties
            if (scrSh.type == FragType.Voronoi)
                SetVoronoi (shatter, scrSh.voronoi.Amount, scrSh.transform, centerPos, scrSh.voronoi.centerBias);
            else if (scrSh.type == FragType.Splinters)
                SetSplinters (shatter, scrSh.splinters.Amount, scrSh.splinters, scrSh.transform, centerPos, scrSh.splinters.centerBias);
            else if (scrSh.type == FragType.Slabs)
                SetSlabs (shatter, scrSh.slabs.Amount, scrSh.slabs, scrSh.transform, centerPos, scrSh.splinters.centerBias);
            else if (scrSh.type == FragType.Radial)
                SetRadial (shatter, scrSh.radial, scrSh.transform, centerPos, scrSh.centerDirection);
            else if (scrSh.type == FragType.Hexagon)
                SetHexGrid (shatter, scrSh.hexagon, scrSh.transform, centerPos, scrSh.centerDirection, scrSh.bound, scrSh.advanced.Seed);
            else if (scrSh.type == FragType.Custom)
                SetCustom (shatter, scrSh.custom, scrSh.transform, scrSh.bound, scrSh.advanced.Seed);
            // else if (scrSh.type == FragType.Mirrored) 
            //     SetMirrored (shatter, scrSh.mirrored, scrSh.transform, scrSh.bound, scrSh.advanced.seed);
            else if (scrSh.type == FragType.Slices)
                SetSlices (shatter, scrSh.transform, scrSh.slice);
            else if (scrSh.type == FragType.Bricks)
                SetBricks (shatter, scrSh.transform, scrSh.bricks, scrSh.bound);
            else if (scrSh.type == FragType.Voxels)
                SetVoxels (shatter, scrSh.transform, scrSh.voxels, scrSh.bound);
            else if (scrSh.type == FragType.Tets)
                SetTet (shatter, scrSh.bound, scrSh.tets);
            else if (scrSh.type == FragType.Decompose)
                SetDecompose (shatter);
        }

        // Set fragmentation properties
        static void SetRigidFragmentProperties(RFShatter shatter, RayfireShatter scrSh, RayfireRigid scrRigid)
        {
            // Rigid demolition without shatter. Set and exit. 
            if (scrSh == null)
            {
                scrRigid.mshDemol.totalAmount = scrRigid.mshDemol.Amount;
                SetVoronoi (shatter, scrRigid.mshDemol.totalAmount, scrRigid.transform, scrRigid.lim.contactVector3, scrRigid.mshDemol.bias);
                return;
            }

            // Set shatter center as contact point for awake precache and prefragment
            if (scrRigid.lim.contactVector3.magnitude == 0)
                scrRigid.lim.contactVector3 = scrSh.transform.TransformPoint (scrSh.centerPosition);

            // Set total amount by rigid component
            if (scrSh.type == FragType.Voronoi)
                scrRigid.mshDemol.totalAmount = scrSh.voronoi.Amount;
            else if (scrSh.type == FragType.Splinters)
                scrRigid.mshDemol.totalAmount = scrSh.splinters.Amount;
            else if (scrSh.type == FragType.Slabs)
                scrRigid.mshDemol.totalAmount = scrSh.slabs.Amount;

            // Get final amount with variation
            if (scrRigid.mshDemol.var > 0)
            {
                int percVar = Random.Range (0, scrRigid.mshDemol.totalAmount * scrRigid.mshDemol.var / 100);
                scrRigid.mshDemol.totalAmount += percVar;
            }

            // Clustering
            if (scrSh.clusters.enable == true)
                SetClusters (shatter, scrSh.clusters);

            // Set properties
            if (scrSh.type == FragType.Voronoi)
                SetVoronoi (shatter, scrRigid.mshDemol.totalAmount, scrSh.transform, scrRigid.lim.contactVector3, scrSh.voronoi.centerBias);
            else if (scrSh.type == FragType.Splinters)
                SetSplinters (shatter, scrRigid.mshDemol.totalAmount, scrSh.splinters, scrSh.transform, scrRigid.lim.contactVector3, scrSh.splinters.centerBias);
            else if (scrSh.type == FragType.Slabs)
                SetSlabs (shatter, scrRigid.mshDemol.totalAmount, scrSh.slabs, scrSh.transform, scrRigid.lim.contactVector3, scrSh.splinters.centerBias);
            else if (scrSh.type == FragType.Radial)
                SetRadial (shatter, scrSh.radial, scrSh.transform, scrRigid.lim.contactVector3, scrSh.centerDirection);
            else if (scrSh.type == FragType.Custom)
                SetCustom (shatter, scrSh.custom, scrSh.transform, scrSh.bound, scrSh.advanced.Seed);
            // else if (scrSh.type == FragType.Mirrored) 
            //     SetMirrored (shatter, scrSh.mirrored, scrSh.transform, scrSh.bound, scrSh.advanced.seed);
            else if (scrSh.type == FragType.Slices)
                SetSlices (shatter, scrSh.transform, scrSh.slice);
            else if (scrSh.type == FragType.Bricks)
                SetBricks (shatter, scrSh.transform, scrSh.bricks, scrSh.bound);
            else if (scrSh.type == FragType.Voxels)
                SetVoxels (shatter, scrSh.transform, scrSh.voxels, scrSh.bound);
            else if (scrSh.type == FragType.Tets)
                SetTet (shatter, scrSh.bound, scrSh.tets);
            else if (scrSh.type == FragType.Decompose)
                SetDecompose (shatter);
        }

        /// /////////////////////////////////////////////////////////
        /// Properties setup
        /// /////////////////////////////////////////////////////////

        // Set common fragmentation properties
        static RFShatter  SetShatter(
            FragmentMode fragmentMode,
            int shatterMode,
            Mesh mesh,
            Transform transform,
            RFSurface interior,
            bool decompose,
            bool delete_collinear = false,
            int seed = 1,
            bool pre_cap = true,
            bool remove_cap_faces = false,
            bool remove_double_faces = true,
            bool exclude_inside = false,
            bool post_normals_smooth = false,
            int min_bbox_diag_size_filter_perc = 3,
            int meshRemoveMinFaceFilter = 4,
            bool postWeld = true
        )
        {
            // Creating shatter
            RFShatter shatter = new RFShatter ((RFShatter.RFShatterMode)shatterMode, true);

            // Safe/unsafe properties
            if (fragmentMode == FragmentMode.Editor)
            {
                float min_bbox_diag_size_filter = mesh.bounds.size.magnitude * min_bbox_diag_size_filter_perc / 100f; // TODO check render bound size
                SetShatterEditorMode (shatter, min_bbox_diag_size_filter, pre_cap, remove_cap_faces, remove_double_faces, exclude_inside, meshRemoveMinFaceFilter);
            }
            else
            {
                SetShatterRuntimeMode (shatter, pre_cap, meshRemoveMinFaceFilter);
            }

            // Detach by elements
            shatter.DecomposeResultMesh (decompose);

            // Set custom UV coordinate for inner surface
            SetCustomUV (shatter, interior);

            // Set custom vertex color for inner surface
            SetCustomColor (shatter, interior);

            // Set silent mode
            shatter.SilentModeEnable (silentMode);
            
            // Set properties
            shatter.SetFragmentParameter (RFShatter.FragmentParams.seed, seed);
            shatter.SetGeneralParameter (RFShatter.GeneralParams.pre_weld_threshold,  0.001f);
            shatter.SetGeneralParameter (RFShatter.GeneralParams.delete_collinear,    delete_collinear);
            shatter.SetGeneralParameter (RFShatter.GeneralParams.post_normals_smooth, post_normals_smooth);
            shatter.SetGeneralParameter (RFShatter.GeneralParams.post_weld,           false);
            shatter.SetGeneralParameter (RFShatter.GeneralParams.maping_scale,        interior.mScl);
            shatter.SetGeneralParameter (RFShatter.GeneralParams.restore_normals,     true);

            // Setting shatter params
            bool inputState = shatter.SetInputMesh (transform, mesh);

            // Failed input
            if (inputState == false)
            {
                RayfireMan.Log ("Bad input mesh: " + transform.name, transform.gameObject);
                return null;
            }

            return shatter;
        }

        // Set Shatter Editor Mode properties
        static void SetShatterEditorMode(
            RFShatter shatter,
            float min_bbox_diag_size_filter,
            bool pre_cap,
            bool remove_cap_faces,
            bool remove_double_faces,
            bool exclude_inside,
            int meshRemoveMinFaceFilter)
        {
            shatter.EditorMode (true);
            shatter.SetGeneralParameter (RFShatter.GeneralParams.editor_mode_min_bbox_diag_size_filter,        min_bbox_diag_size_filter);
            shatter.SetGeneralParameter (RFShatter.GeneralParams.editor_mode_pre_cap,                          pre_cap);
            shatter.SetGeneralParameter (RFShatter.GeneralParams.editor_mode_remove_cap_faces,                 remove_cap_faces);
            shatter.SetGeneralParameter (RFShatter.GeneralParams.editor_mode_remove_double_faces,              remove_double_faces);
            shatter.SetGeneralParameter (RFShatter.GeneralParams.editor_mode_remove_inversed_double_faces,     remove_double_faces);
            shatter.SetGeneralParameter (RFShatter.GeneralParams.editor_mode_exclude_inside,                   exclude_inside);
            shatter.SetGeneralParameter (RFShatter.GeneralParams.editor_mode_meshRemoveMinFaceFilter,          meshRemoveMinFaceFilter); // Minimum amount of triangles for element to be fragmented, will be removed otherwise
            shatter.SetGeneralParameter (RFShatter.GeneralParams.editor_mode_elliminateCollinears_maxIterFuse, 150);
            shatter.SetGeneralParameter (RFShatter.GeneralParams.editor_mode_separate_only,                    false);
            shatter.SetGeneralParameter (RFShatter.GeneralParams.minFacesFilter,                               0);
        }

        // Set Shatter Runtime Mode properties
        static void SetShatterRuntimeMode(RFShatter shatter, bool pre_cap, int meshRemoveMinFaceFilter)
        {
            shatter.EditorMode (false);
            shatter.SetGeneralParameter (RFShatter.GeneralParams.pre_shatter,             true);
            shatter.SetGeneralParameter (RFShatter.GeneralParams.pre_cap,                 pre_cap);
            shatter.SetGeneralParameter (RFShatter.GeneralParams.pre_weld,                true);
            shatter.SetGeneralParameter (RFShatter.GeneralParams.minFacesFilter,          3);
            shatter.SetGeneralParameter (RFShatter.GeneralParams.meshRemoveMinFaceFilter, meshRemoveMinFaceFilter);
        }

        // Set custom UV coordinate for inner surface
        static void SetCustomUV(RFShatter shatter, RFSurface interior)
        {
            shatter.SetGeneralParameter (RFShatter.GeneralParams.enable_custUVs, interior.uvE);
            if (interior.uvE == true)
                shatter.SetPoint3Parameter ((int)RFShatter.GeneralParams.cust_UVs, new Vector3 (interior.uvC.x, interior.uvC.y, 0));
        }

        // Set custom vertex color for inner surface
        static void SetCustomColor(RFShatter shatter, RFSurface interior)
        {
            Vector3 vc = new Vector3 (interior.cC.r, interior.cC.g, interior.cC.b);
            shatter.SetPoint3Parameter ((int)RFShatter.GeneralParams.inner_color, vc);
        }

        /// /////////////////////////////////////////////////////////
        /// Fragmentation types
        /// /////////////////////////////////////////////////////////

        // Set Uniform
        static void SetVoronoi(RFShatter shatter, int numFragments, Transform tm, Vector3 centerPos, float centerBias)
        {
            // Get amount. IMPORTANT: Should be min 1 (not 2) because of Runtime caching
            int amount = numFragments;
            if (amount < 1)
                amount = 1;
            if (amount > 20000)
                amount = 2;

            // Set properties
            shatter.SetFragmentParameter (RFShatter.FragmentParams.type,            (int)RFShatter.FragmentType.voronoi);
            shatter.SetFragmentParameter (RFShatter.FragmentParams.voronoi_type,    (int)RFShatter.VoronoiType.irregular);
            shatter.SetFragmentParameter (RFShatter.FragmentParams.voronoi_irr_num, amount);

            // Set bias to center
            if (centerBias > 0)
            {
                shatter.SetFragmentParameter (RFShatter.FragmentParams.voronoi_irr_bias, centerBias);
                shatter.SetCenterParameter (centerPos, tm, Vector3.forward);
            }
        }

        // Set Splinters
        static void SetSplinters(RFShatter shatter, int numFragments, RFSplinters splint, Transform tm, Vector3 centerPos, float centerBias)
        {
            // Set properties
            shatter.SetFragmentParameter (RFShatter.FragmentParams.type,            (int)RFShatter.FragmentType.voronoi);
            shatter.SetFragmentParameter (RFShatter.FragmentParams.voronoi_type,    (int)RFShatter.VoronoiType.irregular);
            shatter.SetFragmentParameter (RFShatter.FragmentParams.voronoi_irr_num, numFragments);

            // Set center
            shatter.SetFragmentParameter (RFShatter.FragmentParams.voronoi_irr_bias, centerBias);
            shatter.SetCenterParameter (centerPos, tm, Vector3.forward);

            // Set Stretching for slabs
            SetStretching (shatter, splint.axis, splint.strength, FragType.Splinters);
        }

        // Set Slabs
        static void SetSlabs(RFShatter shatter, int numFragments, RFSplinters slabs, Transform tm, Vector3 centerPos, float centerBias)
        {
            // Set properties
            shatter.SetFragmentParameter (RFShatter.FragmentParams.type,            (int)RFShatter.FragmentType.voronoi);
            shatter.SetFragmentParameter (RFShatter.FragmentParams.voronoi_type,    (int)RFShatter.VoronoiType.irregular);
            shatter.SetFragmentParameter (RFShatter.FragmentParams.voronoi_irr_num, numFragments);

            // Set center
            shatter.SetFragmentParameter (RFShatter.FragmentParams.voronoi_irr_bias, centerBias);
            shatter.SetCenterParameter (centerPos, tm, Vector3.forward);

            // Set Stretching for slabs
            SetStretching (shatter, slabs.axis, slabs.strength, FragType.Slabs);
        }

        // Set Radial
        static void SetRadial(RFShatter shatter, RFRadial radial, Transform tm, Vector3 centerPos, Quaternion centerDirection)
        {
            // Set radial properties
            shatter.SetFragmentParameter (RFShatter.FragmentParams.type,                       (int)RFShatter.FragmentType.voronoi);
            shatter.SetFragmentParameter (RFShatter.FragmentParams.voronoi_type,               (int)RFShatter.VoronoiType.radial);
            shatter.SetFragmentParameter (RFShatter.FragmentParams.voronoi_rad_radius,         radial.radius);
            shatter.SetFragmentParameter (RFShatter.FragmentParams.voronoi_rad_divergence,     radial.divergence);
            shatter.SetFragmentParameter (RFShatter.FragmentParams.voronoi_rad_restrict,       radial.restrictToPlane);
            shatter.SetFragmentParameter (RFShatter.FragmentParams.voronoi_rad_rings_count,    radial.rings);
            shatter.SetFragmentParameter (RFShatter.FragmentParams.voronoi_rad_rings_focus,    radial.focus);
            shatter.SetFragmentParameter (RFShatter.FragmentParams.voronoi_rad_rings_strenght, radial.focusStr);
            shatter.SetFragmentParameter (RFShatter.FragmentParams.voronoi_rad_rings_random,   radial.randomRings);
            shatter.SetFragmentParameter (RFShatter.FragmentParams.voronoi_rad_rays_count,     radial.rays);
            shatter.SetFragmentParameter (RFShatter.FragmentParams.voronoi_rad_rays_random,    radial.randomRays);
            shatter.SetFragmentParameter (RFShatter.FragmentParams.voronoi_rad_rays_twist,     radial.twist);

            // Get direction axis
            Vector3 directionAxis = DirectionAxis (radial.centerAxis);
            Vector3 centerRot     = tm.rotation * centerDirection * directionAxis;
            shatter.SetCenterParameter (centerPos, tm, centerRot);
        }

        // Set hexgrid custom point cloud
        static void SetHexGrid(RFShatter shatter, RFHexagon hexagon, Transform tm, Vector3 centerPos, Quaternion centerDirection, Bounds bound, int seed)
        {
            // Set properties
            shatter.SetFragmentParameter (RFShatter.FragmentParams.type,         (int)RFShatter.FragmentType.voronoi);
            shatter.SetFragmentParameter (RFShatter.FragmentParams.voronoi_type, (int)RFShatter.VoronoiType.custom);

            // Get Point Cloud
            List<Vector3> pointCloud = RFHexagon.GetHexPointCLoudV1 (hexagon, tm, centerPos, centerDirection, seed, bound);

            // Set points
            shatter.SetVoroCustomPoints (pointCloud.ToArray(), tm);
        }

        // Set custom point cloud
        static void SetCustom(RFShatter shatter, RFCustom custom, Transform tm, Bounds bound, int seed)
        {
            // Set properties
            shatter.SetFragmentParameter (RFShatter.FragmentParams.type,         (int)RFShatter.FragmentType.voronoi);
            shatter.SetFragmentParameter (RFShatter.FragmentParams.voronoi_type, (int)RFShatter.VoronoiType.custom);

            // Get Point Cloud
            List<Vector3> pointCloud = RFCustom.GetCustomPointCLoud (custom, tm, seed, bound);

            // Set points
            shatter.SetVoroCustomPoints (pointCloud.ToArray(), tm);

            // Set Stretching TODO point cloud rescale by transform
            // if (custom.modifier == RFCustom.RFModifierType.Splinters)
            //     SetStretching (shatter, splint.axis, splint.strength, FragType.Splinters);
            // else if (custom.modifier == RFCustom.RFModifierType.Slabs)
            //     SetStretching (shatter, slabs.axis, slabs.strength, FragType.Slabs);
        }

        // Set custom mirrored point cloud
        static void SetMirrored(RFShatter shatter, RFMirrored mirror, Transform tm, Bounds bound, int seed)
        {
            // Set properties
            shatter.SetFragmentParameter (RFShatter.FragmentParams.type,         (int)RFShatter.FragmentType.voronoi);
            shatter.SetFragmentParameter (RFShatter.FragmentParams.voronoi_type, (int)RFShatter.VoronoiType.custom);

            // Get Point Cloud
            List<Vector3> pointCloud = RFMirrored.GetMirroredPointCLoud (mirror, tm, seed, bound);

            // Set points
            shatter.SetVoroCustomPoints (pointCloud.ToArray(), tm);
        }

        // Set slicing objects
        static void SetSlices(RFShatter shatter, Transform tm, RFSlice slices)
        {
            // Vars 
            Vector3[]       points;
            Vector3[]       norms;
            List<Transform> list = new List<Transform>();
            
            // Collect slice transforms
            for (int i = 0; i < slices.sliceList.Count; i++)
                if (slices.sliceList[i] != null)
                    list.Add (slices.sliceList[i]);

            // No objects. Use default center
            if (list.Count == 0)
            {
                points = new[] {tm.position};
                if (slices.plane == PlaneType.XY)
                    norms = new[] {Vector3.up};
                else  if (slices.plane == PlaneType.YZ)
                    norms = new[] {Vector3.right};
                else 
                    norms = new[] {Vector3.forward};
            }

            // Get slice data
            else
            {
                points = list.Select (t => t.position).ToArray();
                norms  = list.Select (slices.Axis).ToArray();
            }
            
            // Set params
            shatter.SetBricksParams (points, norms, tm);
        }

        // Set bricks properties
        static void SetBricks(RFShatter shatter, Transform tm, RFBricks bricks, Bounds bounds)
        {
            // Amount size
            if (bricks.amountType == RFBricks.RFBrickType.ByAmount)
            {
                float X       = bricks.amount_X * bricks.mult;
                if (X == 0) 
                    X = 1;
                float Y       = bricks.amount_Y * bricks.mult;
                if (Y == 0) 
                    Y = 1;
                float Z       = bricks.amount_Z * bricks.mult;
                if (Z == 0) 
                    Z = 1;

                Vector3 amount = new Vector3 (X, Y, Z);
                shatter.SetPoint3Parameter ((int)RFShatter.FragmentParams.bricks_num_bricks, amount);
            }
            else if (bricks.amountType == RFBricks.RFBrickType.BySize)
            {
                // TODO small size check
                Vector3 size = new Vector3 (bricks.size_X * bricks.mult, bricks.size_Y * bricks.mult, bricks.size_Z * bricks.mult);
                shatter.SetPoint3Parameter ((int)RFShatter.FragmentParams.bricks_num_bricks, Vector3.zero);
                shatter.SetPoint3Parameter ((int)RFShatter.FragmentParams.bricks_brick_size, size);
            }

            // Random size
            Vector3 random_size = new Vector3 (bricks.sizeVar_X / 100f, bricks.sizeVar_Y / 100f, bricks.sizeVar_Z / 100f);
            shatter.SetPoint3Parameter ((int)RFShatter.FragmentParams.bricks_random_size, random_size);

            // Offset
            Vector3 offsets = new Vector3 (bricks.offset_X, bricks.offset_Y, bricks.offset_Z);
            shatter.SetPoint3Parameter ((int)RFShatter.FragmentParams.bricks_offsets, offsets); // 0-1

            // Split
            Vector3 random_split = new Vector3 (BoolToFloat (bricks.split_X), BoolToFloat (bricks.split_Y), BoolToFloat (bricks.split_Z));
            shatter.SetPoint3Parameter ((int)RFShatter.FragmentParams.bricks_random_split, random_split);
            shatter.SetFragmentParameter (RFShatter.FragmentParams.brick_slice_probability, bricks.split_probability * 0.01f);
            shatter.SetFragmentParameter (RFShatter.FragmentParams.brick_slice_offset,      bricks.split_offset);
            shatter.SetFragmentParameter (RFShatter.FragmentParams.brick_slice_rotate,      bricks.split_rotation);
        }

        // Set voxels properties
        static void SetVoxels(RFShatter shatter, Transform tm, RFVoxels voxels, Bounds bounds)
        {
            // TODO small size check
            Vector3 size = new Vector3 (voxels.size, voxels.size, voxels.size);
            shatter.SetPoint3Parameter ((int)RFShatter.FragmentParams.bricks_num_bricks, Vector3.zero);
            shatter.SetPoint3Parameter ((int)RFShatter.FragmentParams.bricks_brick_size, size);

            // Offset
            shatter.SetPoint3Parameter ((int)RFShatter.FragmentParams.bricks_offsets, Vector3.zero);
        }

        static float BoolToFloat(bool v)
        {
            if (v == false) return 0;
            return 1f;
        }

        // Set Custom Voronoi properties
        static void SetTet(RFShatter shatter, Bounds bounds, RFTets tets)
        {
            // Main
            shatter.SetFragmentParameter (RFShatter.FragmentParams.type,       (int)RFShatter.FragmentType.tetra);
            shatter.SetFragmentParameter (RFShatter.FragmentParams.tetra_type, (int)tets.lattice);

            // Get max
            float max = bounds.size.x;
            if (bounds.size.y > max)
                max = bounds.size.y;
            if (bounds.size.z > max)
                max = bounds.size.z;
            if (max == 0)
                max = 0.01f;

            // Get density
            Vector3Int density = new Vector3Int (
                (int)Mathf.Ceil (bounds.size.x / max * tets.density),
                (int)Mathf.Ceil (bounds.size.y / max * tets.density),
                (int)Mathf.Ceil (bounds.size.z / max * tets.density));

            // Limit
            if (density.x > 30) density.x     = 30;
            else if (density.x < 1) density.x = 1;
            if (density.y > 30) density.y     = 30;
            else if (density.y < 1) density.y = 1;
            if (density.z > 30) density.z     = 30;
            else if (density.z < 1) density.z = 1;

            // Set density
            shatter.SetPoint3Parameter ((int)RFShatter.FragmentParams.tetra2_density, density);
            shatter.SetPoint3Parameter ((int)RFShatter.FragmentParams.tetra1_density, density);

            // Noise
            shatter.SetFragmentParameter (RFShatter.FragmentParams.tetra_noise, tets.noise);
        }

        // Decompose to elements
        static void SetDecompose(RFShatter shatter)
        {
            shatter.SetGeneralParameter (RFShatter.GeneralParams.editor_mode_separate_only, true);
        }

        /// /////////////////////////////////////////////////////////
        /// Clusters
        /// /////////////////////////////////////////////////////////

        // Set clusters
        static void SetClusters(RFShatter shatter, RFShatterCluster gluing)
        {
            // Set seed
            int glueSeed = gluing.Seed;

            shatter.InitClustering (true);
            shatter.SetClusterParameter (RFShatter.ClusterParams.enabled,         true);
            shatter.SetClusterParameter (RFShatter.ClusterParams.by_pcloud_count, gluing.count);
            shatter.SetClusterParameter (RFShatter.ClusterParams.options_seed,    glueSeed);
            shatter.SetClusterParameter (RFShatter.ClusterParams.preview_scale,   100f);

            // Debris
            shatter.SetClusterParameter (RFShatter.ClusterParams.debris_layers_count, gluing.layers);
            shatter.SetClusterParameter (RFShatter.ClusterParams.debris_count,        gluing.amount);
            shatter.SetClusterParameter (RFShatter.ClusterParams.debris_scale,        gluing.scale);
            shatter.SetClusterParameter (RFShatter.ClusterParams.debris_min,          gluing.min);
            shatter.SetClusterParameter (RFShatter.ClusterParams.debris_max,          gluing.max);
            shatter.SetClusterParameter (RFShatter.ClusterParams.debris_tessellate,   false);
            shatter.SetClusterParameter (RFShatter.ClusterParams.debris_remove,       false);

            // Glue 
            shatter.SetGeneralParameter (RFShatter.GeneralParams.glue,                true);
            shatter.SetGeneralParameter (RFShatter.GeneralParams.glue_weld_threshold, 0.001f);
            shatter.SetGeneralParameter (RFShatter.GeneralParams.relax,               gluing.relax);
        }

        /// /////////////////////////////////////////////////////////
        /// Stretching
        /// /////////////////////////////////////////////////////////

        // Set stretching
        static void SetStretching(RFShatter shatter, AxisType axis, float strength, FragType fragType)
        {
            // Get slab vector
            Vector3 stretchDir = DirectionAxis (axis);

            // Adjust for slabs
            if (fragType == FragType.Slabs)
            {
                Vector3 vector                   = new Vector3();
                if (stretchDir.x <= 0) vector.x  = 1f;
                if (stretchDir.x >= 1f) vector.x = 0;
                if (stretchDir.y <= 0) vector.y  = 1f;
                if (stretchDir.y >= 1f) vector.y = 0;
                if (stretchDir.z <= 0) vector.z  = 1f;
                if (stretchDir.z >= 1f) vector.z = 0;
                stretchDir = vector;
            }

            // Set stretch vector
            shatter.SetPoint3Parameter ((int)RFShatter.FragmentParams.stretching, stretchDir * Mathf.Lerp (40f, 99f, strength));
        }

        // Get axis by type
        static Vector3 DirectionAxis(AxisType axisType)
        {
            if (axisType == AxisType.YGreen)
                return Vector3.up;
            if (axisType == AxisType.ZBlue)
                return Vector3.forward;
            return Vector3.right;
        }

        /// /////////////////////////////////////////////////////////
        /// Mesh
        /// /////////////////////////////////////////////////////////

        // Scale mesh
        public static void RescaleMesh(Mesh mesh, float scale)
        {
            Vector3[] verts = mesh.vertices;
            for (int j = 0; j < verts.Length; j++)
                verts[j] /= scale;
            mesh.vertices = verts;
        }
        
        /// /////////////////////////////////////////////////////////
        /// Interactive
        /// /////////////////////////////////////////////////////////
        
        // Fragment all meshes into own mesh
        public static void InteractiveStart(RayfireShatter scr)
        {
            // Prepare to cache fragments
            if (scr.PreCache() == false)
            {
                scr.OriginalRenderer(true);
                return;
            }

            // Create interactive object if there is no any
            scr.InteractiveCreate();
            
            // Cache
            CacheMeshes (ref scr.meshes, ref scr.pivots, ref scr.rfOrigSubMeshIds, scr);
            
            // Stop
            if (scr.meshes == null)
            {
                scr.OriginalRenderer(true);
                return;
            }
            
            // Get dictionary list
            List<Dictionary<int, int>> ids = RFDictionary.GetDictionary (scr.rfOrigSubMeshIds);
            
            // Weld into one mesh and set to interactive object
            scr.intMf.sharedMesh = RFShatter.WeldMeshes (ref scr.meshes, ref scr.pivots, ref ids, scr.PreviewScale());
            
            // Disable own Renderer
            scr.OriginalRenderer(false);
        }
        
        // Property changed
        public static void InteractiveChange(RayfireShatter scr)
        {
            if (scr.interactive == false)
                return;
            
            // Prepare to cache fragments
            if (scr.PreCache() == false)
            {
                scr.OriginalRenderer(true);
                return;
            }
            
            // Recache with new properties
            CacheMeshes (ref scr.meshes, ref scr.pivots, ref scr.rfOrigSubMeshIds, scr);
            
            // Stop
            if (scr.meshes == null)
            {
                scr.OriginalRenderer(true);
                return;
            }

            // Get dictionary list
            List<Dictionary<int, int>> ids = RFDictionary.GetDictionary (scr.rfOrigSubMeshIds);

            // Weld into one mesh and set to interactive object
            scr.intMf.sharedMesh = RFShatter.WeldMeshes (ref scr.meshes, ref scr.pivots, ref ids, scr.PreviewScale());
            
            // Disable own Renderer
            scr.OriginalRenderer(false);
        }
    }
}

// Static dummy class for not supported platforms
#else
namespace RayFire
{
    public static class RFFragment
    {
        public static bool PrepareCacheMeshes(RayfireRigid scr)
        {
            BuildTest(scr);
            return false;
        }

        public static void CacheMeshesMult(Transform tmSaved, ref List<Mesh> meshesList, ref List<Vector3> pivotsList, ref List<RFDictionary> subList, RayfireRigid scrRigid, List<int> batchAmount, int batchInd) 
        {
            BuildTest();
        }

        public static void CacheMeshesInst(ref Mesh[] meshes, ref Vector3[] pivots, ref RFDictionary[] origSubMeshIdsRf, RayfireRigid scrRigid)  
        {
            BuildTest();
        }

        public static RFShatter CacheMeshes(ref Mesh[] meshes, ref Vector3[] pivots, ref RFDictionary[] origSubMeshIdsRf, RayfireShatter scrShatter)  
        {
            BuildTest();
            return new RFShatter();
        }

        public static void SliceMeshes(ref Mesh[] meshes, ref Vector3[] pivots, ref RFDictionary[] origSubMeshIdsRf, RayfireRigid scrRigid, List<Vector3> sliceData)  
        {
            BuildTest();
        }

        public static void RescaleMesh (Mesh mesh, float scale)
        {
            BuildTest();
        }

        public static bool InputMesh(RayfireRigid scr)
        {
            BuildTest();
            scr.meshes = null;
            return false;
        }

        public static void InteractiveStart(RayfireShatter scr) 
        {
            BuildTest();
        }

        public static void InteractiveChange(RayfireShatter scr) 
        {
            BuildTest();
        }
        
        static void BuildTest(RayfireRigid scr)
        {
            //Debug.Log ("Dummy");
        }
        
        static void BuildTest()
        {
            //Debug.Log ("Dummy");
        }
        
    }

    public class RFShatter
    {
        public static Mesh WeldMeshes(ref Mesh[] meshes,ref Vector3[] pivots,ref List<Dictionary<int, int>> subIds,float scale)
        {
            return new Mesh();
        }
    }
}

#endif