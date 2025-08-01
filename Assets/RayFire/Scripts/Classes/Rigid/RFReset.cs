﻿using System;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace RayFire
{
    [Serializable]
    public class RFReset
    {
        public enum PostDemolitionType
        {
            DestroyWithDelay  = 0,
            DeactivateToReset = 1
        }
        
        public enum MeshResetType
        {
            Destroy             = 0,
            ReuseFragmentMeshes = 4
        }
        
        public enum FragmentsResetType
        {
            Destroy     = 0,
            Reuse       = 2,
            Preserve    = 4
        }
        
        // UI
        public bool               transform;
        public bool               damage;
        public bool               connectivity;
        public PostDemolitionType action;
        public float              destroyDelay;
        public MeshResetType      mesh;
        public FragmentsResetType fragments;

        // Non serialized
        [NonSerialized] public bool toBeDestroyed;

        /// /////////////////////////////////////////////////////////
        /// Constructor
        /// /////////////////////////////////////////////////////////

        // Constructor
        public RFReset()
        {
            InitValues();
        }
        
        void InitValues()
        {
            transform     = true;
            damage        = true;
            connectivity  = false;
            action        = PostDemolitionType.DestroyWithDelay;
            destroyDelay  = 1;
            mesh          = MeshResetType.ReuseFragmentMeshes;
            fragments     = FragmentsResetType.Destroy;
            toBeDestroyed = false;
        }
        
        // Pool Reset
        public void GlobalReset()
        {
            InitValues();
        }

        // Copy from
        public void CopyFrom (RFReset source, ObjectType objectType)
        {
            transform    = source.transform;
            damage       = source.damage;
            action       = source.action;
            destroyDelay = source.destroyDelay;
            
            // Copy to initial object: mesh root copy
            if (objectType == ObjectType.MeshRoot)
            {
                mesh      = source.mesh;
                fragments = source.fragments;
            }

            // Copy to cluster shards
            else if (objectType == ObjectType.ConnectedCluster)
            {
                mesh      = source.mesh;
                fragments = source.fragments;
            }
            
            // Copy to demolished mesh fragments
            else if (objectType == ObjectType.Mesh)
            {
                mesh      = MeshResetType.Destroy;
                fragments = FragmentsResetType.Destroy;

                // Do not keep fragments at destroy if parent not going to reuse fragments or getting destroyed
                if (source.action == PostDemolitionType.DestroyWithDelay || 
                    source.fragments == FragmentsResetType.Destroy)
                    action = PostDemolitionType.DestroyWithDelay;
            }
        }
        
        /// /////////////////////////////////////////////////////////
        /// Rigid Mesh
        /// /////////////////////////////////////////////////////////
        
        // Rigid 
        public static void ResetRigid (RayfireRigid scr)
        {
            // Object can't be reused
            if (ObjectReuseState (scr) == false)
                return;
            
            // Mesh Root reset
            if (MeshRootReset (scr) == true)
                return;
            
            // Save faded/demolished state before reset
            int faded = scr.fading.state;
            bool demolished = scr.lim.demolished;

            // Reset tm
            if (scr.reset.transform == true)
                RestoreTransform(scr);
            
            // Reset activation TODO check if it was Kinematic
            if (scr.act.activated == true)
                scr.simTp = SimType.Inactive;
            
            // ReSet activation layer. IMPORTANT before Reset()
            RFActivation.RestoreActivationLayer (scr);
            
            // Reset rigid props
            Reset (scr);
            
            // Stop all cors in case object restarted
            scr.StopAllCoroutines();
            
            // Reset if object fading/faded
            if (faded >= 1)
                ResetFade(scr);
            
            // Demolished. Restore
            if (demolished == true)
                ResetMeshDemolition (scr);
            
            // Restore cluster even if it was not demolished
            ResetClusterDemolition (scr);
            
            // Reset sound
            ResetSound(scr.sound);
            
            // Remove particles
            DestroyRigidParticles (scr);
            
            // Enable Rigid because of cluster fade and reset
            if (scr.enabled == false)
                scr.enabled = true;
            
            // Activate if deactivated
            if (scr.gameObject.activeSelf == false)
                scr.gameObject.SetActive (true);

            // Start all coroutines
            scr.StartAllCoroutines();
            
            // Restart restrictions cors
            if (scr.rest != null)
                scr.rest.InitRestriction (scr);
        }

        // Reset if object fading/faded
        public static void ResetFade (RayfireRigid scr)
        {
            // Was excluded
            if (scr.fading.fadeType == FadeType.SimExclude)
            {
                // Null check because of Planar check fragments without collider
                if (scr.physics.mc != null)
                    scr.physics.mc.enabled = true;// TODO CHECK CLUSTER COLLIDERS
            }   
               
            // Was fall down
            else if (scr.fading.fadeType == FadeType.FallDown)
            {
                // Null check because of Planar check fragments without collider
                if (scr.physics.mc != null)
                    scr.physics.mc.enabled = true;// TODO CHECK CLUSTER COLLIDERS
                
                scr.gameObject.SetActive (true);
            } 
            
            // Was scaled down
            else if (scr.fading.fadeType == FadeType.ScaleDown)
            {
                scr.tsf.localScale = scr.physics.initScale;
                scr.gameObject.SetActive (true);
            }
            
            // Was moved down
            if (scr.fading.fadeType == FadeType.MoveDown)
            {
                // Null check because of Planar check fragments without collider
                if (scr.physics.mc != null)
                    scr.physics.mc.enabled = true; // TODO CHECK CLUSTER COLLIDERS

                // Reset gravity
                if (scr.simTp != SimType.Inactive)
                    scr.physics.rb.useGravity = scr.physics.gr;
                
                scr.gameObject.SetActive (true);
            }

            // Was destroyed
            else if (scr.fading.fadeType == FadeType.Destroy)
                scr.gameObject.SetActive (true);
            
            // Was set static
            if (scr.fading.fadeType == FadeType.SetStatic)
                scr.gameObject.SetActive (true);
            
            // Was set static
            if (scr.fading.fadeType == FadeType.SetKinematic)
                scr.gameObject.SetActive (true);
        }
        
        /// /////////////////////////////////////////////////////////
        /// Rigid Mesh Root
        /// /////////////////////////////////////////////////////////

        // Mesh Root 
        static bool MeshRootReset (RayfireRigid scr)
        {
            // Not mesh root
            if (scr.objTp != ObjectType.MeshRoot)
                return false;

            // Cleanup destroyed/faded fragments
            if (MeshRootCleanup (scr) == false)
                return true;

            // Destroy particles
            DestroyMeshRootParticles (scr);
            
            // Reset tm
            scr.physics.LoadInitTransform (scr.transform);

            // Reset fragments first
            foreach (var fragment in scr.fragments)
            {
                // Add rigid body to Rigid if it was deleted because of clustering
                if (fragment.physics.rb == null)
                    fragment.physics.rb = fragment.gameObject.AddComponent<Rigidbody>();
                
                // Set object type back in case of clustering->demolition
                fragment.simTp = scr.simTp;

                // Set parent in case of clustering->demolition
                fragment.tsf.parent = scr.tsf;
                
                // Reset rigid
                ResetRigid (fragment);
                
                // Set density. After collider defined TODO save mass at first apply, reuse now
                RFPhysic.SetDensity (fragment);

                // Set drag properties
                RFPhysic.SetDrag (fragment);
                
                // Destroy parent connected cluster if rigid was clustered
                if (fragment.rtP != null)
                    Object.Destroy (fragment.rtP.gameObject);
                
                // TODO Test fragments reuse with transform state copied to fragments
            }
            
            // Reset uny data
            RayfireUnyielding.SetMeshRootUnyState (scr.transform, null);
            
            // Restore connectivity cluster
            RFBackupCluster.RestoreConnectivity (scr.act.cnt);
            
            return true;
        }

        // Cleanup and check for mesh root fragments
        static bool MeshRootCleanup (RayfireRigid scr)
        {
            // Cleanup destroyed/faded fragments
            for (int i = scr.fragments.Count - 1; i >= 0; i--)
                if (scr.fragments[i] == null)
                {
                    RayfireMan.Log (RFLog.rig_dbgn + scr.name + RFLog.rig_res1, scr.gameObject);
                    scr.fragments.RemoveAt (i);
                }

            // Check after cleanup
            if (scr.HasFragments == false)
                return false;

            return true;
        }

        // Destroy particles
        static void DestroyMeshRootParticles (RayfireRigid scr)
        {
            if (scr.particleList.Count > 0)
            {
                for (int i = scr.particleList.Count - 1; i >= 0; i--)
                    if (scr.particleList[i] != null)
                        RayfireMan.DestroyGo (scr.particleList[i].gameObject);
                scr.particleList.Clear();
            }
        }
        
        /// /////////////////////////////////////////////////////////
        /// Reset Rigid Root
        /// /////////////////////////////////////////////////////////

        // Reinit demolished mesh object
        public static void RigidRootReset (RayfireRigidRoot scr)
        {
            // Stop all cors in case object restarted
            scr.StopAllCoroutines();
            scr.corState                    = false;
            scr.activation.inactiveCorState = false;
            scr.fading.offsetCorState       = false;

            if (scr.activation.cnt != null)
            {
                scr.activation.cnt.StopAllCoroutines();
                RFCollapse.StopCollapse (scr.activation.cnt);
                RFStress.StopStress (scr.activation.cnt);
            }
            
            // Reset activation
            scr.activation.LocalReset();
            
            // TODO CHECK FOR RESET STATES
            // TODO CLEANUP
            
            // Destroy particle roots
            DestroyRigidRootParticles (scr);
            
            // Reset tm
            scr.transform.position   = scr.cluster.pos;
            scr.transform.rotation   = scr.cluster.rot;
            scr.transform.localScale = scr.cluster.scl;
           
            // Set object type back in case of clustering->demolition
            ResetSimType (scr);

            // ReSet parents for all shards
            ResetParentAndTm (scr);
            
            // Reset shards
            for (int i = 0; i < scr.cluster.shards.Count; i++)
            {
                // Shard faded
                if (scr.cluster.shards[i].fade != 0)
                {
                    // Enable collider
                    if (scr.cluster.shards[i].col.enabled == false)
                        scr.cluster.shards[i].col.enabled = true;
                    
                    // Reset fading
                    scr.cluster.shards[i].fade = 0;
                }
                
                // TODO Destroy parent connected cluster if rigid was clustered
                
                // Activate
                if (scr.cluster.shards[i].tm.gameObject.activeSelf == false)
                    scr.cluster.shards[i].tm.gameObject.SetActive (true);
            }

            // ReSet layer for activated shards
            RFActivation.RestoreActivationLayer (scr);
            
            // Set physics properties for shards
            RFPhysic.SetPhysics(scr.cluster.shards, scr.physics);
            
            /* TODO check if should be here
            // Reset shards with Rigid
            for (int i = 0; i < scr.cluster.shards.Count; i++)
                if (scr.cluster.shards[i].rigid != null)
                    scr.cluster.shards[i].rigid.ResetRigid();
                    */
            
            // Setup list for activation shards
            scr.SetInactiveList ();

            // Setup list with fade by offset shards
            RFFade.SetOffsetFadeList (scr);

            // Destroy child clusters if they were created
            DestroyClusters (scr);

            // Restore connectivity cluster
            RFBackupCluster.RestoreConnectivity (scr.activation.cnt);
            
            // Reset sound
            ResetSound(scr.sound);
            
            // Start coroutines
            scr.StartAllCoroutines();
        }
        
        // ReSet parents and transform for all shards
        static void ResetParentAndTm(RayfireRigidRoot scr)
        {
            // TODO null checks
            for (int i = 0; i < scr.cluster.shards.Count; i++)
            {
                scr.cluster.shards[i].tm.SetParent (null);
                scr.cluster.shards[i].tm.SetPositionAndRotation (scr.cluster.shards[i].pos, scr.cluster.shards[i].rot);
                scr.cluster.shards[i].tm.SetParent (scr.parentList[i], true);
                scr.cluster.shards[i].tm.localScale = scr.cluster.shards[i].scl;
            }
        }
        
        // Set object type back in case of clustering->demolition
        static void ResetSimType(RayfireRigidRoot scr)
        {
            // Reset by RigidRoot and Rigid components
            for (int i = 0; i < scr.cluster.shards.Count; i++)
            {
                if (scr.cluster.shards[i].rigid == null)
                    scr.cluster.shards[i].sm = scr.simTp;
                else 
                {
                    if (scr.cluster.shards[i].rigid.objTp == ObjectType.MeshRoot)
                        scr.cluster.shards[i].sm = scr.cluster.shards[i].rigid.simTp;
                    else if (scr.cluster.shards[i].rigid.objTp == ObjectType.Mesh)
                        scr.cluster.shards[i].rigid.ResetRigid();
                }
                
                // Reset velocity
                if (scr.cluster.shards[i].rb != null)
                {
                    scr.cluster.shards[i].rb.linearVelocity        = Vector3.zero;
                    scr.cluster.shards[i].rb.angularVelocity = Vector3.zero;
                }
            }
            
            // Reset uny states and sim state
            for (int i = 0; i < scr.unyList.Length; i++)
                scr.unyList[i].SetRigidRootUnyShardList();
        }
        
        // Destroy particles
        static void DestroyRigidRootParticles (RayfireRigidRoot scr)
        {
            if (scr.particleList.Count > 0)
            {
                for (int i = scr.particleList.Count - 1; i >= 0; i--)
                    if (scr.particleList[i] != null)
                        RayfireMan.DestroyGo (scr.particleList[i].gameObject);
                scr.particleList.Clear();
            }
        }
        
        // Destroy clusters
        static void DestroyClusters (RayfireRigidRoot scr)
        {
            for (int i = 0; i < scr.clusters.Count; i++)
                if (scr.clusters[i].tm != null)
                    RayfireMan.DestroyGo (scr.clusters[i].tm.gameObject);
            
            scr.clusters.Clear();
        }

        /// /////////////////////////////////////////////////////////
        /// Demolition reset
        /// /////////////////////////////////////////////////////////
        
        // Reinit demolished mesh object
        public static void ResetMeshDemolition (RayfireRigid scr)
        {
            // Edit meshes and fragments only if object was demolished
            if (scr.objTp == ObjectType.Mesh)
            {
                // Reset Meshes
                if (scr.reset.mesh == MeshResetType.Destroy)
                {
                    scr.meshes          = null;
                    scr.mshDemol.engine = null;
                }

                // Fragments need to be reused
                if (scr.reset.fragments == FragmentsResetType.Reuse)
                {
                    // Can be reused. Destroyed if can not
                    if (FragmentReuseState (scr) == true)
                        ReuseFragments (scr);
                    else
                        DestroyFragments (scr);
                }
                
                // Destroy fragments
                else if (scr.reset.fragments == FragmentsResetType.Destroy)
                    DestroyFragments (scr);
                
                // Fragments should be kept in scene. Forget about them
                else if (scr.reset.fragments == FragmentsResetType.Preserve)
                    PreserveFragments (scr);
            }
      
            // Activate
            scr.gameObject.SetActive (true);
        }
        
        // Destroy fragments and root  // TODO send to pool
        static void DestroyFragments (RayfireRigid scr)
        {
            // Destroy fragments    
            if (scr.HasFragments == true)
            {
                // Get amount of fragments
                int fragmentNum = scr.fragments.Count (t => t != null);

                // Destroy fragments and root
                for (int i = scr.fragments.Count - 1; i >= 0; i--)
                {
                    if (scr.fragments[i] != null)
                    {
                        // Destroy particles
                        DestroyRigidParticles (scr.fragments[i]);
                        
                        // Destroy fragment
                        scr.fragments[i].gameObject.SetActive (false);
                        RayfireMan.DestroyGo (scr.fragments[i].gameObject);

                        // Destroy root
                        if (scr.fragments[i].rtP != null)
                        {
                            scr.fragments[i].rtP.gameObject.SetActive (false);
                            RayfireMan.DestroyGo (scr.fragments[i].rtP.gameObject);
                        }
                    }
                }
                
                // Nullify
                scr.fragments = null;

                // Subtract amount of deleted fragments
                RayfireMan.inst.advancedDemolitionProperties.ChangeCurrentAmount (-fragmentNum);

                // Destroy descendants
                if (scr.lim.desc != null && scr.lim.desc.Count > 0)
                {
                    // Get amount of descendants
                    int descendantNum = scr.lim.desc.Count (t => t != null);
                    
                    // Destroy fragments and root
                    for (int i = 0; i < scr.lim.desc.Count; i++)
                    {
                        if (scr.lim.desc[i] != null)
                        {
                            // Destroy fragment
                            scr.lim.desc[i].gameObject.SetActive (false);
                            RayfireMan.DestroyGo (scr.lim.desc[i].gameObject);

                            // Destroy root
                            if (scr.lim.desc[i].rtP != null)
                            {
                                scr.lim.desc[i].rtP.gameObject.SetActive (false);
                                RayfireMan.DestroyGo (scr.lim.desc[i].rtP.gameObject);
                            }
                        }
                    }
                    
                    // Clear
                    scr.lim.desc.Clear();
                    
                    // Subtract amount of deleted fragments
                    RayfireMan.inst.advancedDemolitionProperties.ChangeCurrentAmount (-descendantNum);
                }
            }
        }

        // Destroy particles // TODO send to pool
        static void DestroyRigidParticles (RayfireRigid scr)
        {
            // Destroy debris
            if (scr.HasDebris == true)
                for (int d = 0; d < scr.debrisList.Count; d++)
                    if (scr.debrisList[d].hostTm != null)
                    {
                        scr.debrisList[d].hostTm.gameObject.SetActive (false);
                        RayfireMan.DestroyGo (scr.debrisList[d].hostTm.gameObject);
                    }

            // Destroy debris
            if (scr.HasDust == true)
                for (int d = 0; d < scr.dustList.Count; d++)
                    if (scr.dustList[d].hostTm != null)
                    {
                        scr.dustList[d].hostTm.gameObject.SetActive (false);
                        RayfireMan.DestroyGo (scr.dustList[d].hostTm.gameObject);
                    }
        }
        
        // Fragments need and can be reused
        static void ReuseFragments (RayfireRigid scr)
        {
            // Sub amount
            RayfireMan.inst.advancedDemolitionProperties.ChangeCurrentAmount (-scr.fragments.Count);
            
            // Activate root
            if (scr.rtC != null)
            {
                scr.rtC.gameObject.SetActive (false);
                scr.rtC.position = scr.tsf.position;
                scr.rtC.rotation = scr.tsf.rotation;
                
                // V2 rotation to 
            }

            // Reset fragments tm
            for (int i = scr.fragments.Count - 1; i >= 0; i--)
            {
                // Destroy particles
                DestroyRigidParticles (scr.fragments[i]);
                
                scr.fragments[i].tsf.localScale = scr.fragments[i].physics.initScale;
                scr.fragments[i].tsf.position = scr.tsf.position + scr.pivots[i];

                if (scr.mshDemol.engTp == RayfireShatter.RFEngineType.V1)
                    scr.fragments[i].tsf.rotation = Quaternion.identity;
                else
                    scr.fragments[i].tsf.localRotation = Quaternion.identity;

                // Reset activation TODO check if it was Kinematic
                if (scr.fragments[i].act.activated == true)
                    scr.fragments[i].simTp = SimType.Inactive;
                
                // Reset fading
                if (scr.fragments[i].fading.state >= 1)
                    ResetFade(scr.fragments[i]);
                
                // Reset rigid props
                Reset (scr.fragments[i]);

                // Disable runtime demolition
                scr.fragments[i].dmlTp = DemolitionType.None;
            }
            
            // Clear descendants
            scr.lim.desc.Clear();
        }
        
        // Preserve Fragments
        static void PreserveFragments (RayfireRigid scr)
        {
            scr.fragments = null;
            scr.rtC = null;
            scr.lim.desc.Clear();
        }
          
        // Reinit demolished mesh object
        static void ResetClusterDemolition (RayfireRigid scr)
        {
            if (scr.objTp == ObjectType.ConnectedCluster || scr.objTp == ObjectType.NestedCluster)
            {
                RFBackupCluster.ResetRigidCluster (scr);
            }
        }

        /// /////////////////////////////////////////////////////////
        /// Reuse state
        /// /////////////////////////////////////////////////////////          
        
        // Check fragments reuse state
        static bool ObjectReuseState (RayfireRigid scr)
        {
            // Mesh Root reset
            if (scr.objTp == ObjectType.MeshRoot)
                return true;
            
            // Excluded from sim
            if (scr.physics.exclude == true)
            {
                RayfireMan.Log (RFLog.rig_dbgn + scr.name + RFLog.rig_res2, scr.gameObject);
                return false;
            }
            
            // Not mesh object type
            if (scr.objTp == ObjectType.Mesh 
                || scr.objTp == ObjectType.ConnectedCluster
                || scr.objTp == ObjectType.NestedCluster)
                return true;
            
            // Object can be reused
            return false;
        }
                
        // Check fragments reuse state
        static bool FragmentReuseState (RayfireRigid scr)
        {
            // Do not reuse reference demolition
            if (scr.dmlTp == DemolitionType.ReferenceDemolition)
                return false;
            
            // Fragments list null or empty
            if (scr.HasFragments == false)
                return false;

            // One of the fragment null
            if (scr.fragments.Any (t => t == null))
                return false;
            
            // One of the fragment going to be destroyed TODO make reusable
            if (scr.fragments.Any (t => t.reset.toBeDestroyed == true))
                return false;
            
            // One of the fragment demolished TODO make reusable
            if (scr.fragments.Any (t => t.lim.demolished == true))
                return false;
  
            // Fragments can be reused
            return true;
        }
        
        /// /////////////////////////////////////////////////////////
        /// Other
        /// /////////////////////////////////////////////////////////      
        
        // Restore transform or initial
        static void RestoreTransform (RayfireRigid scr)
        {
            // Restore tm
            scr.physics.LoadInitTransform (scr.tsf);
            scr.physics.velocity = Vector3.zero;
            
            // Restore rigidbody TODO save initial velocity into vars and reset to them
            if (scr.physics.rb != null)
            {
                scr.physics.rb.linearVelocity        = Vector3.zero;
                scr.physics.rb.angularVelocity = Vector3.zero;
            }
        }
        
        // Restore rigid properties
        static void Reset (RayfireRigid scr)
        {
            // Reset caching if it is on
            scr.mshDemol.ch.StopRuntimeCaching();
            
            scr.physics.LocalReset();
            scr.act.LocalReset();
            if (scr.rest != null)
                scr.rest.Reset();
            scr.lim.LocalReset();
            scr.mshDemol.LocalReset();
            scr.clsDemol.LocalReset();
            scr.fading.LocalReset();
            if (scr.reset.damage == true)
                scr.damage.LocalReset();
            
            // Set physical simulation type. Important. Should after collider material define
            RFPhysic.SetSimulationType (scr.physics.rb, scr.simTp, scr.objTp, scr.physics.gr, scr.physics.si, scr.physics.st);
            
            // Set sleeping state TODO
            if (scr.simTp == SimType.Sleeping)
            {
                scr.physics.velocity           = Vector3.zero;
                scr.physics.rb.linearVelocity        = Vector3.zero;
                scr.physics.rb.angularVelocity = Vector3.zero;
                scr.physics.rb.Sleep();
            }
        }

        // Reset sound
        static void ResetSound (RayfireSound scr)
        {
            if (scr != null)
            {
                scr.initialization.played = false;
                scr.activation.played = false;
                scr.demolition.played = false;
            }
        }
    }
}