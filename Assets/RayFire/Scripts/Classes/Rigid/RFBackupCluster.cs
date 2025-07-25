﻿using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace RayFire
{
    public class RFBackupCluster
    {
        // Connected
        public RFCluster cluster;
        bool             saved;

        // Constructor
        RFBackupCluster()
        {
            saved = false;
        }
        
        /// /////////////////////////////////////////////////////////
        /// Save / Restore
        /// /////////////////////////////////////////////////////////

        // Save backup cluster to restore it later
        public static void BackupConnectedCluster (RayfireRigid scr)
        {
            // No need to save
            if (scr.reset.action != RFReset.PostDemolitionType.DeactivateToReset)
                return;
            
            // Do not backup child clusters
            if (scr.clsDemol.cluster.id > 1)
                return;
            
            // Create backup if not exist
            if (scr.clsDemol.backup == null)
                scr.clsDemol.backup = new RFBackupCluster();
            
            // Already saved
            if (scr.clsDemol.backup.saved == true)
                return;
            
            // Copy class
            scr.clsDemol.backup.cluster = new RFCluster(scr.clsDemol.cluster);
            
            // Init shards: set non serialized vars
            RFCluster.InitCluster (scr, scr.clsDemol.backup.cluster);
            
            // Save nested clusters shards and clusters position and rotation
            SaveTmRecursive (scr.clsDemol.backup.cluster);
            
            // Backup created, do not create again at next reset
            scr.clsDemol.backup.saved = true;
            
            // Debug.Log ("Saved");
        }
        
        // Restore cluster using backup cluster
        public static void ResetRigidCluster (RayfireRigid scr)
        {
            if (scr.reset.action == RFReset.PostDemolitionType.DeactivateToReset)
            {
                // Do not restore child clusters
                if (scr.clsDemol.cluster.id > 1)
                    return;

                // Has no backup
                if (scr.clsDemol.backup == null)
                    return;
                
                // Cluster was not demolished. Stop
                if (scr.objTp == ObjectType.ConnectedCluster)
                    if (scr.clsDemol.cluster.shards.Count == scr.clsDemol.backup.cluster.shards.Count)
                        return;

                // TODO check if nested cluster was demolished
                // if (false) if (scr.objectType == ObjectType.NestedCluster)
                //     if (scr.clusterDemolition.cluster.tm.gameObject.activeSelf == true)
                //return;
                
                // Completely demolished child clusters do not deactivates if saved
                // Unyielding component with inactive overlap bug
                
                // Reset fragments list
                scr.fragments = null;
                
                // Remove particles
                DestroyParticles (scr);
                
                // Reset local shard rigid, destroy components TODO INPUT ORIGINAL CLUSTER, GET RIGIDS
                ResetDeepShardRigid (scr, scr.clsDemol.backup.cluster);
                
                // Create new child clusters roots destroy by nested cluster. BEFORE reparent shards
                if (scr.objTp == ObjectType.NestedCluster)
                {
                    ResetNestedRootsRecursive (scr.clsDemol.backup.cluster);
                    ResetNestedTransformRecursive (scr.clsDemol.backup.cluster);
                    ResetNestedParentsRecursive (scr.clsDemol.backup.cluster);
                }
                
                // Restore shards parent, position and rotation 
                RestoreShardTmRecursive (scr.clsDemol.backup.cluster);
                
                // Destroy new child clusters roots created by connected cluster. AFTER reparent shards
                if (scr.objTp == ObjectType.ConnectedCluster)
                    DestroyRoots (scr);
                
                // Copy class
                scr.clsDemol.cluster = new RFCluster(scr.clsDemol.backup.cluster);
                
                // Reset colliders 
                RFPhysic.CollectClusterColliders (scr, scr.clsDemol.cluster);
                
                // Init shards: set non serialized vars
                RFCluster.InitCluster (scr, scr.clsDemol.cluster);

                scr.clsDemol.collapse.inProgress = false;
            }
        }

        // Remove particles
        static void DestroyParticles(RayfireRigid scr)
        {
            if (scr.HasDebris == true)
                for (int i = 0; i < scr.debrisList.Count; i++)
                {
                    for (int c = scr.debrisList[i].children.Count - 1; c >= 0; c--)
                    {
                        if (scr.debrisList[i].children[c] != null)
                        {
                            if (scr.debrisList[i].children[c].hostTm != null)
                                Object.Destroy (scr.debrisList[i].children[c].hostTm.gameObject);
                            Object.Destroy (scr.debrisList[i].children[c]);
                        }
                        scr.debrisList[i].children.RemoveAt (c);
                    }
                    scr.debrisList[i].children = null;
                }
            if (scr.HasDust == true)
                for (int i = 0; i < scr.dustList.Count; i++)
                {
                    for (int c = scr.dustList[i].children.Count - 1; c >= 0; c--)
                    {
                        if (scr.dustList[i].children[c] != null)
                        {
                            if (scr.dustList[i].children[c].hostTm != null)
                                Object.Destroy (scr.dustList[i].children[c].hostTm.gameObject);
                            Object.Destroy (scr.dustList[i].children[c]);
                        }
                        scr.dustList[i].children.RemoveAt (c);
                    }
                    scr.dustList[i].children = null;
                }
        }

        /// /////////////////////////////////////////////////////////
        /// Reset shard rigid
        /// /////////////////////////////////////////////////////////      
        
        // Reset local shard rigid, destroy components
        static void ResetDeepShardRigid (RayfireRigid scr, RFCluster cluster)
        {
            // Collect shards colliders
            for (int i = 0; i < cluster.shards.Count; i++)
                ResetShardRigid (cluster.shards[i]);

            // Set child cluster colliders
            if (scr.objTp == ObjectType.NestedCluster)
                if (cluster.HasChildClusters == true)
                    for (int i = 0; i < cluster.childClusters.Count; i++)
                        ResetDeepShardRigid (scr, cluster.childClusters[i]);
        }

        // Reset local shard rigid, destroy components
        static void ResetShardRigid (RFShard shard)
        {
            // Set parent
            shard.tm.parent = shard.cluster.tm;
            
            // Enable collider in case of fade disable
            if (shard.col.enabled == false)
                shard.col.enabled = true;
            
            // Get shard's rigid
            if (shard.rigid == null)
                shard.rigid = shard.tm.GetComponent<RayfireRigid>();

            if (shard.rigid != null && shard.rigid.initialized == true)
            {
                // Stop all cors in case object restarted
                shard.rigid.StopAllCoroutines();
                shard.rigid.initialized = false;
                
                // Destroy rigid body
                if (shard.rigid.physics.rb != null)
                {
                    shard.rigid.physics.rb.linearVelocity = Vector3.zero;
                    Object.Destroy (shard.rigid.physics.rb);
                }
                
                // Save faded/demolished state before reset
                int  faded      = shard.rigid.fading.state;
                bool demolished = shard.rigid.lim.demolished;
                
                // Reset activation TODO check if it was Kinematic
                if (shard.rigid.act.activated == true)
                    shard.rigid.simTp = SimType.Inactive;
                
                // Reset rigid props
                shard.rigid.physics.LocalReset();
                shard.rigid.act.LocalReset();
                shard.rigid.lim.LocalReset();
                shard.rigid.mshDemol.LocalReset();
                shard.rigid.clsDemol.LocalReset();
                shard.rigid.fading.LocalReset();
                shard.rigid.damage.LocalReset();
                
                // Reset if object fading/faded
                if (faded >= 1)
                    RFReset.ResetFade(shard.rigid);
                
                // Demolished. Restore
                if (demolished == true)
                    RFReset.ResetMeshDemolition (shard.rigid);
                
                // Remove particles
                //                DestroyParticles (shard.rigid);

                // shard.rigid.initialized = false;
                
                
                
                // TODO COPY RIGID TO BACKUP CLUSTER SHARD.RIGID TO REUSE AT NEXT RESET. AVOID GETCOMPONENT
            }
        }
        
        /// /////////////////////////////////////////////////////////
        /// Transform / parent
        /// /////////////////////////////////////////////////////////
        
        // Save cluster/shards tm
        public static void SaveTmRecursive(RFCluster cluster)
        {
            // Save cluster tm
            cluster.pos = cluster.tm.position;
            cluster.rot = cluster.tm.rotation;
            cluster.scl = cluster.tm.lossyScale;
            
            // Save shards tm
            for (int i = 0; i < cluster.shards.Count; i++)
            {
                cluster.shards[i].pos = cluster.shards[i].tm.position;
                cluster.shards[i].rot = cluster.shards[i].tm.rotation;
                cluster.shards[i].scl = cluster.shards[i].tm.localScale;
            }

            // Repeat for child clusters
            if (cluster.HasChildClusters == true)
                for (int i = 0; i < cluster.childClusters.Count; i++)
                    SaveTmRecursive (cluster.childClusters[i]);
        }
        
        // Save cluster/shards tm
        static void RestoreShardTmRecursive(RFCluster cluster)
        {
            // Save shards tm
            for (int i = 0; i < cluster.shards.Count; i++)
            {
                cluster.shards[i].tm.SetParent (null);
                cluster.shards[i].tm.SetPositionAndRotation (cluster.shards[i].pos, cluster.shards[i].rot);
                cluster.shards[i].tm.SetParent (cluster.tm, true);
                cluster.shards[i].tm.localScale = cluster.shards[i].scl;
            }

            // Repeat for child clusters
            if (cluster.HasChildClusters == true)
                for (int i = 0; i < cluster.childClusters.Count; i++)
                    RestoreShardTmRecursive (cluster.childClusters[i]);
        }
        
        // Save cluster/shards tm
        static void ResetNestedTransformRecursive(RFCluster cluster)
        {
            // Save cluster tm
            cluster.tm.rotation   = cluster.rot;
            cluster.tm.position   = cluster.pos;
            
            // TODO FIND PROPER ORDER cluster.tm.localScale = cluster.scl;
            
            // Repeat for child clusters
            if (cluster.HasChildClusters == true)
                for (int i = 0; i < cluster.childClusters.Count; i++)
                    ResetNestedTransformRecursive (cluster.childClusters[i]);
        }
        
        /// /////////////////////////////////////////////////////////
        /// Roots
        /// /////////////////////////////////////////////////////////
        
        // Create deleted roots and restore their tm back
        static void ResetNestedRootsRecursive(RFCluster cluster)
        {
            if (cluster.HasChildClusters == true)
            {
                for (int i = 0; i < cluster.childClusters.Count; i++)
                {
                    // Reset parent
                    cluster.childClusters[i].tm.parent = null;
                    //cluster.tm.localScale = cluster.scl;
                    
                    // Get rigid component
                    if (cluster.childClusters[i].rigid == null)
                        cluster.childClusters[i].rigid = cluster.childClusters[i].tm.GetComponent<RayfireRigid>();
                    
                    // Destroy rigid
                    if (cluster.childClusters[i].rigid != null)
                    {
                        // Destroy rigid body
                        if (cluster.childClusters[i].rigid.physics.rb != null)
                            Object.Destroy (cluster.childClusters[i].rigid.physics.rb);
                        
                        // Destroy rigid
                        Object.Destroy (cluster.childClusters[i].rigid);
                    }

                    // Activate
                    cluster.childClusters[i].tm.gameObject.SetActive (true);

                    // Repeat for children
                    ResetNestedRootsRecursive (cluster.childClusters[i]);
                }
            }
        }
        
        // Create deleted roots and restore their tm back
        static void ResetNestedParentsRecursive(RFCluster cluster)
        {
            if (cluster.HasChildClusters == true)
                for (int i = cluster.childClusters.Count - 1; i >= 0; i--)
                {
                    cluster.childClusters[i].tm.parent = cluster.tm;
                    ResetNestedParentsRecursive (cluster.childClusters[i]);
                    
                }
        }
        
        // Destroy new child clusters roots created by connected cluster
        static void DestroyRoots (RayfireRigid scr)
        {
            for (int i = 0; i < scr.clsDemol.minorClusters.Count; i++)
            {
                if (scr.clsDemol.minorClusters[i].tm != null)
                {
                    scr.clsDemol.minorClusters[i].tm.gameObject.SetActive (false);
                    Object.Destroy (scr.clsDemol.minorClusters[i].tm.gameObject);
                }
            }
        }

        /// /////////////////////////////////////////////////////////
        /// Connectivity
        /// /////////////////////////////////////////////////////////

        // Backup abd restore state
        static bool BackupState (RayfireConnectivity scr, bool warning = false)
        {
            // Not for in editor setup. Runtime only
            if (Application.isPlaying == false)
                return false;
            
            // Has no connectivity
            if (scr == null)
                return false;
            
            // No Rigid or Rigid Root
            if (scr.meshRootHost == null && scr.rigidRootHost == null)
                return false;
            
            // Rigid not mesh root
            if (scr.meshRootHost != null && scr.meshRootHost.objTp != ObjectType.MeshRoot)
                return false;
                
            // Backup disabled
            if (scr.meshRootHost != null && scr.meshRootHost.reset.connectivity == false)
            {
                if (warning == true)
                    RayfireMan.Log (RFLog.rig_dbgn + scr.name + RFLog.rig_conRes, scr.gameObject);
                return false;
            }
            
            // Backup disabled
            if (scr.rigidRootHost != null && scr.rigidRootHost.reset.connectivity == false)
            {
                if (warning == true)
                    RayfireMan.Log (RFLog.rig_dbgn + scr.name + RFLog.rig_conRes, scr.gameObject);
                return false;
            }

            return true;
        }
        
        // Save backup cluster to restore it later
        public static void BackupConnectivity (RayfireConnectivity scr)
        {
            // Checks state
            if (BackupState(scr) == false)
                return;

            // Create backup if not exist
            if (scr.backup == null)
                scr.backup = new RFBackupCluster();
            
            // Already saved
            if (scr.backup.saved == true)
                return;
            
            // Copy class TODO copy shards stress 
            scr.backup.cluster = new RFCluster(scr.cluster);
            
            // TODO init non serialized vars
            
            // Save nested clusters shards and clusters position and rotation
            SaveTmRecursive (scr.backup.cluster);
            
            // Backup created, do not create again at next reset
            scr.backup.saved = true;
        }
        
        // Restore cluster using backup cluster
        public static void RestoreConnectivity (RayfireConnectivity scr)
        {
            // Checks state
            if (BackupState(scr, true) == false)
                return;
            
            // Has no backup
            if (scr.backup == null)
                return;
         
            // Stop all coroutines
            scr.StopAllCoroutines();

            // Reset connectivity
            scr.ResetConnectivity();
            
            // Mesh Root Rigid init
            if (scr.meshRootHost != null)
            {
                // Restore shards parent, position and rotation
                RestoreShardTmRecursive (scr.backup.cluster);
                
                // Copy class
                scr.cluster = new RFCluster(scr.backup.cluster);
                
                // Shards were cached, reinit non serialized vars, clear list otherwise
                if (RayfireConnectivity.InitCachedShardsByRigidList (scr.rigidList, scr.cluster) == true)
                    scr.cluster.shards.Clear();

                // Set this connectivity as main connectivity node
                for (int i = 0; i < scr.rigidList.Count; i++)
                    scr.rigidList[i].act.cnt = scr;
            }

            // Rigid Root' Connectivity cluster shards init
            if (scr.rigidRootHost != null)
            {
                // Copy connection data from backup cluster back to RigidRoot cluster
                for (int i = 0; i < scr.rigidRootHost.cluster.shards.Count; i++)
                {
                    // if (scr.rigidRootHost.cluster.shards[i].sm == SimType.Inactive || scr.rigidRootHost.cluster.shards[i].sm == SimType.Kinematic)
                    {
                        // Neib ids
                        scr.rigidRootHost.cluster.shards[i].nIds = new List<int> (scr.backup.cluster.shards[i].nIds.Count);
                        for (int j = 0; j < scr.backup.cluster.shards[i].nIds.Count; j++)
                            scr.rigidRootHost.cluster.shards[i].nIds.Add (scr.backup.cluster.shards[i].nIds[j]);

                        // Neib areas
                        scr.rigidRootHost.cluster.shards[i].nArea = new List<float> (scr.backup.cluster.shards[i].nArea.Count);
                        for (int j = 0; j < scr.backup.cluster.shards[i].nArea.Count; j++)
                            scr.rigidRootHost.cluster.shards[i].nArea.Add (scr.backup.cluster.shards[i].nArea[j]);
                    }
                }

                if (scr.stress.enable == true)
                {
                    for (int s = 0; s < scr.rigidRootHost.cluster.shards.Count; s++)
                    {
                        scr.rigidRootHost.cluster.shards[s].sIds = new List<int> (scr.backup.cluster.shards[s].sIds.Count);
                        for (int i = 0; i < scr.backup.cluster.shards[s].sIds.Count; i++)
                            scr.rigidRootHost.cluster.shards[s].sIds.Add (scr.backup.cluster.shards[s].sIds[i]);

                        scr.rigidRootHost.cluster.shards[s].nSt = new List<float>();
                        for (int i = 0; i < scr.backup.cluster.shards[s].nSt.Count; i++)
                            scr.rigidRootHost.cluster.shards[s].nSt.Add (scr.backup.cluster.shards[s].nSt[i]);

                        scr.rigidRootHost.cluster.shards[s].sSt = scr.backup.cluster.shards[s].sSt;
                    }
                }

                // Shards were cached, reinit non serialized vars, clear list otherwise
                RayfireConnectivity.InitCachedShardsByRigidRoot (scr.rigidRootHost, scr.cluster);
                
                // Set range for area and size
                RFCollapse.CopyRangeData (scr.cluster, scr.backup.cluster);
            }
            
            // Reset coroutines
            scr.childrenChanged       = false;
            scr.connectivityCheckNeed = false;
            scr.collapse.inProgress   = false;
            scr.stress.inProgress     = false;
            
            // Start all coroutines
            scr.StartAllCoroutines();
        }
    }
}