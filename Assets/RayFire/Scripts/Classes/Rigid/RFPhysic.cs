﻿using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Object = UnityEngine.Object;


namespace RayFire
{
    [Serializable]
    public class RFPhysic
    {
        [FormerlySerializedAs ("materialType")]     public MaterialType   mt;
        [FormerlySerializedAs ("material")]         public PhysicsMaterial ma;
        [FormerlySerializedAs ("massBy")]           public MassType       mb;
        [FormerlySerializedAs ("mass")]             public float          ms;
        [FormerlySerializedAs ("colliderType")]     public RFColliderType ct;
        [FormerlySerializedAs ("planarCheck")]      public bool           pc;
        [FormerlySerializedAs ("ignoreNear")]       public bool           ine;
        [FormerlySerializedAs ("useGravity")]       public bool           gr;
        [FormerlySerializedAs ("solverIterations")] public int            si;
        public                                             float          st; // Sleeping threshold
        [FormerlySerializedAs ("dampening")]        public float          dm;
        [FormerlySerializedAs ("rigidBody")]        public Rigidbody      rb;
        [FormerlySerializedAs ("meshCollider")]     public Collider       mc;
        [FormerlySerializedAs ("clusterColliders")] public List<Collider> cc;
        [FormerlySerializedAs ("ignoreList")]       public List<int>      ign;
        
        // TODO set no serialized
        public                 bool           exclude;
        public                 int            solidity;
        public                 bool           destructible;
        
        [NonSerialized] public bool       rec;
        [NonSerialized] public bool       velCache;
        [NonSerialized] public Vector3    velocity;
        [NonSerialized] public Vector3    initScale;
        [NonSerialized] public Vector3    initPosition;
        [NonSerialized] public Quaternion initRotation;
        [NonSerialized] public Vector3    localPosition;
        
        /// /////////////////////////////////////////////////////////
        /// Constructor
        /// /////////////////////////////////////////////////////////

        // Constructor
        public RFPhysic()
        {
            InitValues();
            LocalReset();
        }
        
        // Pool Reset
        void InitValues()
        {
            mt       = MaterialType.Concrete;
            ma       = null;
            mb       = MassType.MaterialDensity;
            ms       = 1f;
            ct       = RFColliderType.Mesh;
            pc       = true;
            ine      = false;
            gr       = true;
            si       = 6;
            st       = 0.005f;
            dm       = 0.7f;
            solidity = 1;
            
            ign           = null;
            initScale     = Vector3.one;
            initPosition  = Vector3.zero;
            initRotation  = Quaternion.identity;
            localPosition = Vector3.zero;
        }
        
        // Reset
        public void LocalReset()
        {
            rec      = false;
            exclude  = false;
            velocity = Vector3.zero;
        }
        
        // Pool Reset
        public void GlobalReset()
        {
            InitValues();
            LocalReset();
            
            cc = null;
            destructible     = false;
            
            // Reset components
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
            }
            
            if (mc != null)
            {
                if (mc is MeshCollider collider)
                {
                    collider.convex         = false;
                    collider.sharedMesh     = null;
                    collider.sharedMaterial = null;
                }
            }
        }

        // Copy from
        public void CopyFrom(RFPhysic physics)
        {
            mt  = physics.mt;
            ma  = physics.ma;
            mb  = physics.mb;
            ms  = physics.ms;
            ct  = physics.ct;
            pc  = physics.pc;
            ine = false;
            gr  = physics.gr;
            si  = physics.si;
            st  = physics.st;
            dm  = physics.dm;
            
            ign = null;
            
            LocalReset();
        }

        // Save init transform. Birth tm for activation check and reset
        public void SaveInitTransform(Transform tm)
        {
            initScale     = tm.localScale;
            initPosition  = tm.position;
            initRotation  = tm.rotation;
            localPosition = tm.localPosition;
        }
        
        // Save init transform. Birth tm for activation check and reset
        public void LoadInitTransform(Transform tm)
        {
            tm.localScale = initScale;
            tm.position   = initPosition;
            tm.rotation   = initRotation;
        }
        
        /// /////////////////////////////////////////////////////////
        /// Simulation Type
        /// /////////////////////////////////////////////////////////
        
        // Set simulation type properties
        public static void SetSimulationType(Rigidbody rb, SimType simulationType, ObjectType objectType, bool useGravity, int solver, float sleep = 0.005f)
        {
            if (simulationType == SimType.Static)
                return;    
            
            // Properties
            rb.interpolation            = RayfireMan.inst.interpolation;
            rb.solverIterations         = solver;
            rb.solverVelocityIterations = solver;
            rb.sleepThreshold           = sleep;

            // Dynamic
            if (simulationType == SimType.Dynamic)
            {
                SetDynamic (rb, useGravity);
                SetCollisionDetection (rb, objectType);
            }

            // Sleeping 
            else if (simulationType == SimType.Sleeping)
            {
                SetSleeping (rb, useGravity);
                SetCollisionDetection (rb, objectType);
            }

            // Inactive
            else if (simulationType == SimType.Inactive)
            {
                SetInactive (rb);
                SetCollisionDetection (rb, objectType);
            }

            // Kinematic
            else if (simulationType == SimType.Kinematic)
                SetKinematic(rb, useGravity);
        }

        // Set as dynamic
        static void SetDynamic(Rigidbody rb, bool useGravity)
        {
            rb.isKinematic = false;
            rb.useGravity  = useGravity;
        }

        // Set as sleeping
        static void SetSleeping(Rigidbody rb, bool useGravity)
        {
            rb.isKinematic = false;
            rb.useGravity  = useGravity;
            rb.Sleep();
        }

        // Set as inactive
        static void SetInactive(Rigidbody rb)
        {
            rb.isKinematic = false;
            rb.useGravity  = false;
            rb.linearDamping        = 100f;
            rb.angularDamping = 100f;
            rb.Sleep();
        }

        // Set as Kinematic
        static void SetKinematic(Rigidbody rb, bool useGravity)
        {
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
            rb.isKinematic            = true;
            rb.useGravity             = useGravity;
        }

        // Collision detection
        static void SetCollisionDetection(Rigidbody rb, ObjectType objectType)
        {
            if (objectType == ObjectType.NestedCluster || objectType == ObjectType.ConnectedCluster)
                rb.collisionDetectionMode = RayfireMan.inst.clusterCollision;
            else
                rb.collisionDetectionMode = RayfireMan.inst.meshCollision;
        }
        
        // Set custom fragment simulation type if not inherited
        public static void SetFragmentSimulationType (RayfireRigid frag, SimType sim)
        {
            frag.simTp = sim;
            if (frag.mshDemol.sim != FragSimType.Inherit)
                frag.simTp = (SimType)frag.mshDemol.sim;
        }
        
        /// /////////////////////////////////////////////////////////
        /// Density
        /// /////////////////////////////////////////////////////////
        
        // Set density. After collider defined.
        public static void SetDensity(RayfireRigid scr)
        {
            // Default mass from inspector
            float m = scr.physics.ms;

            // Mass by rigid body
            if (scr.physics.mb == MassType.RigidBodyComponent)
            {
                // Return if has rigidbody component with defined mass
                if (scr.physics.rb != null)
                    return;

                // Set to by density if has no rigid component
                scr.physics.mb = MassType.MaterialDensity;
            } 
            
            // Get mass by density
            if (scr.physics.mb == MassType.MaterialDensity)
            {
                scr.physics.rb.SetDensity(RayfireMan.inst.materialPresets.Density(scr.physics.mt));
                m = scr.physics.rb.mass;
                
                // SetDensity API deprecated By Design in UNITY 6 and will be removed         
                #if UNITY_6    
                if (scr.physics.mc != null)
                    m = scr.physics.mc.bounds.size.magnitude * RayfireMan.inst.materialPresets.Density(scr.physics.mt) * 0.8f;
                #endif
            }
            
            // Check for min/max mass
            m = MassLimit (m);
            
            // Update mass in inspector
            scr.physics.rb.mass = m;
        }
        
        // Set density. After collider defined.
        public static void SetDensity(RFShard shard, RFPhysic physics, float density)
        {
            // Set mass if it was already defined before
            if (shard.m > 0)
            {
                shard.rb.mass = shard.m;
                // TODO STOP??? Check if mass need to be updated and reset it to 0
            }
            
            // Default mass from inspector
            float m = physics.ms;

            // Set mass by density
            if (physics.mb == MassType.MaterialDensity)
            {
                shard.rb.SetDensity (density);
                m = shard.rb.mass;
               
                // SetDensity API deprecated By Design in UNITY 6 and will be removed    
                #if UNITY_6    
                if (shard.col != null)
                    m = shard.col.bounds.size.magnitude * RayfireMan.inst.materialPresets.Density(scr.physics.mt) * 0.8f;
                #endif
            }
            
            // Set mass by rb component. Stop
            else if (physics.mb == MassType.RigidBodyComponent)
                return;

            // Check for min/max mass
            m = MassLimit (m);
            
            // set mass in shard properties
            shard.m = m;
            
            // Update mass in rigidbody
            shard.rb.mass = m;
        }

        // Limit mass with min max range
        static float MassLimit(float m)
        {
            if (RayfireMan.inst.minimumMass > 0)
                if (m < RayfireMan.inst.minimumMass)
                    return RayfireMan.inst.minimumMass;
            if (RayfireMan.inst.maximumMass > 0)
                if (m > RayfireMan.inst.maximumMass)
                    return RayfireMan.inst.maximumMass;
            return m;
        }

        // Set mass by mass value accordingly to parent
        public static void SetMassByParent(RFPhysic target, float targetSize, float parentMass, float parentSize)
        {
            target.ms      = parentMass * (targetSize / parentSize) * 0.7f;
            target.rb.mass = target.ms;
        }
        
        /// /////////////////////////////////////////////////////////
        /// Drag
        /// /////////////////////////////////////////////////////////
        
        // Set drag properties
        public static void SetDrag(RayfireRigid scr)
        {
            if (scr.simTp != SimType.Inactive)
            {
                scr.physics.rb.linearDamping        = RayfireMan.inst.materialPresets.Drag(scr.physics.mt);
                scr.physics.rb.angularDamping = RayfireMan.inst.materialPresets.AngularDrag(scr.physics.mt);
            }
            else
            {
                scr.physics.rb.linearDamping        = 100f;
                scr.physics.rb.angularDamping = 100f;
            }
        }

        // Set drag properties
        public static void SetDrag(RFShard shard, float drag, float dragAngular)
        {
            if (shard.sm != SimType.Inactive)
            {
                shard.rb.linearDamping        = drag;
                shard.rb.angularDamping = dragAngular;
            }
            else
            {
                shard.rb.linearDamping        = 100f;
                shard.rb.angularDamping = 100f;
            }
        }
        
        /// /////////////////////////////////////////////////////////
        /// Rigid body
        /// /////////////////////////////////////////////////////////
        
        // Set velocity
        public static void SetFragmentsVelocity (RayfireRigid scr)
        {
            // TODO different for clusters, get rigid body center of mass
            
            // Current velocity
            if (scr.mshDemol.ch.wasUsed == true && scr.mshDemol.ch.skp == false)
            {
                for (int i = 0; i < scr.fragments.Count; i++)
                    if (scr.fragments[i] != null)
                        scr.fragments[i].physics.rb.linearVelocity = scr.physics.rb.GetPointVelocity (scr.fragments[i].tsf.position) * scr.physics.dm;
            }

            // Previous frame velocity
            else
            {
                Vector3 baseVelocity = scr.physics.velocity * scr.physics.dm;
                for (int i = 0; i < scr.fragments.Count; i++)
                    if (scr.fragments[i] != null)
                        if (scr.fragments[i].physics.rb != null && scr.fragments[i].physics.rb.isKinematic == false)
                            scr.fragments[i].physics.rb.linearVelocity = baseVelocity;
            }
        }
        
        /// /////////////////////////////////////////////////////////
        /// Mesh Collider
        /// /////////////////////////////////////////////////////////
        
        // Set fragments collider
        public static void SetFragmentCollider(RayfireRigid scr, Mesh mesh)
        {
            // Custom collider
            scr.physics.ct = scr.mshDemol.prp.col;
            
            // Size filter check
            if (scr.mshDemol.prp.szF > 0 && mesh.bounds.size.magnitude < scr.mshDemol.prp.szF)
                scr.physics.ct = RFColliderType.None;
            
            // Skip collider
            SetRigidCollider (scr, mesh);
        }
        
        // Set fragments collider
        public static void SetRigidCollider (RayfireRigid scr, Mesh mesh = null)
        {
            // Skip collider
            if (scr.physics.ct == RFColliderType.None)
                return;
            
            // Discard collider if just trigger
            if (scr.physics.mc != null && scr.physics.mc.isTrigger == true)
                scr.physics.mc = null;

            // Size check
            if (RayfireMan.inst != null && RayfireMan.inst.colliderSize > 0)
                if (scr.mRnd.bounds.size.magnitude < RayfireMan.inst.colliderSize)
                    return;
            
            // No collider. Add own
            if (scr.physics.mc == null)
            {
                // Mesh collider
                if (scr.physics.ct == RFColliderType.Mesh)
                {
                    // Low vert check
                    if (scr.mFlt.sharedMesh.vertexCount <= 3)
                        return;
                    
                    // Optional coplanar check
                    if (scr.physics.pc == true && scr.mFlt.sharedMesh.vertexCount < RayfireMan.coplanarVertLimit)
                    {
                        if (RFShatterAdvanced.IsCoplanar (scr.mFlt.sharedMesh, RFShatterAdvanced.planarThreshold) == true)
                        {
                            RayfireMan.Log (RFLog.rig_dbgn + scr.name + RFLog.rig_plane, scr.gameObject);
                            scr.physics.ct = RFColliderType.None;
                            return;
                        }
                    }

                    // Add Mesh collider
                    MeshCollider mCol = scr.gameObject.AddComponent<MeshCollider>();
                    mCol.cookingOptions = RayfireMan.cookingOptionsStatic;
                    
                    // Set mesh
                    if (mesh != null)
                        mCol.sharedMesh = mesh;

                    // Set convex
                    if (scr.simTp != SimType.Static)
                        mCol.convex = true;
                    scr.physics.mc = mCol;
                }
                    
                // Box.Sphere collider
                else if (scr.physics.ct == RFColliderType.Box)
                    scr.physics.mc = scr.gameObject.AddComponent<BoxCollider>();
                else if (scr.physics.ct == RFColliderType.Sphere)
                    scr.physics.mc = scr.gameObject.AddComponent<SphereCollider>();
            }
        }
        
        // Set fragments collider
        public static void SetRigidRootCollider (RayfireRigidRoot root, RFPhysic physics, RFShard shard)
        {
            // Get collider
            shard.col = shard.tm.GetComponent<Collider>(); 
            
            // Skip collider
            if (physics.ct == RFColliderType.None)
                return;
            
            // No collider. Add own
            if (shard.col == null)
            {
                // Mesh collider
                if (physics.ct == RFColliderType.Mesh)
                {
                    // Add Mesh collider
                    MeshCollider col = shard.tm.gameObject.AddComponent<MeshCollider>();
                    col.cookingOptions = RayfireMan.cookingOptionsStatic;
                    col.sharedMesh     = shard.mf.sharedMesh;
                    col.convex         = true;
                    shard.col          = col;
                }
                    
                // Box / Sphere collider
                else if (physics.ct == RFColliderType.Box)
                    shard.col = shard.tm.gameObject.AddComponent<BoxCollider>();
                else if (physics.ct == RFColliderType.Sphere)
                    shard.col = shard.tm.gameObject.AddComponent<SphereCollider>();
                
                // Collect applied collider to destroy at setup reset
                root.collidersList.Add (shard.col);
            }
        }

        // Set collider for mesh root fragments in editor setup
        public static void SetupMeshRootColliders(RayfireRigid scr)
        {
            Collider col;
            scr.physics.cc = new List<Collider>(scr.fragments.Count);
            for (int i = 0; i < scr.fragments.Count; i++)
            {
                // Collect own colliders
                col = scr.fragments[i].GetComponent<Collider>();
                if (col != null)
                    scr.physics.cc.Add (col);

                // Add Collider
                SetRigidCollider (scr.fragments[i]);
            }
        }
        
        /// /////////////////////////////////////////////////////////
        /// Add Connected/Nested Cluster Colliders
        /// /////////////////////////////////////////////////////////
        
        // Create mesh colliders for every input mesh TODO input cluster to control all nest roots for correct colliders
        public static void SetClusterCollidersByShards (RayfireRigid scr)
        {
            // Check colliders list
            CollidersRemoveNull (scr);

            // Already clusterized
            if (scr.physics.HasClusterColliders == true)
                return;
            
            // Colliders list
            if (scr.physics.cc == null)
                scr.physics.cc = new List<Collider>();
            
            // Connected/Nested colliders
            if (scr.objTp == ObjectType.ConnectedCluster)
                SetShardColliders (scr, scr.clsDemol.cluster);
            else if (scr.objTp == ObjectType.NestedCluster)
                SetDeepShardColliders (scr, scr.clsDemol.cluster);
        }

        // Null check and remove
        static void CollidersRemoveNull(RayfireRigid scr)
        {
            if (scr.physics.HasClusterColliders == true)
                for (int i = scr.physics.cc.Count - 1; i >= 0; i--)
                    if (scr.physics.cc[i] == null)
                        scr.physics.cc.RemoveAt (i);
        }
        
        // Check children for mesh or cluster root until all children will not be checked
        static void SetShardColliders (RayfireRigid scr, RFCluster cluster)
        {
            // Mesh collider
            if (scr.physics.ct == RFColliderType.Mesh)
            {
                for (int i = 0; i < cluster.shards.Count; i++)
                {
                    // Get mesh filter and collider TODO set collider by type
                    MeshCollider meshCol = cluster.shards[i].tm.GetComponent<MeshCollider>();
                    if (meshCol == null)
                    {
                        meshCol            = cluster.shards[i].mf.gameObject.AddComponent<MeshCollider>();
                        meshCol.sharedMesh = cluster.shards[i].mf.sharedMesh;
                    }
                    meshCol.convex = true;
   
                    // Set shard collider and collect
                    cluster.shards[i].col = meshCol;
                    scr.physics.cc.Add (meshCol);
                }
            }
                    
            // Box.Sphere collider
            else if (scr.physics.ct == RFColliderType.Box)
            {
                for (int i = 0; i < cluster.shards.Count; i++)
                {
                    // Set shard collider and collect
                    cluster.shards[i].col = cluster.shards[i].mf.gameObject.AddComponent<BoxCollider>();
                    scr.physics.cc.Add (cluster.shards[i].col);
                }
            }
            else if (scr.physics.ct == RFColliderType.Sphere)
            {
                for (int i = 0; i < cluster.shards.Count; i++)
                {
                    cluster.shards[i].col = cluster.shards[i].mf.gameObject.AddComponent<SphereCollider>();
                    scr.physics.cc.Add (cluster.shards[i].col);
                }
            }
        }
        
        // Check children for mesh or cluster root until all children will not be checked
        static void SetDeepShardColliders (RayfireRigid scr, RFCluster cluster)
        {
            // Set shard colliders
            SetShardColliders (scr, cluster);

            // Set child cluster colliders
            if (cluster.HasChildClusters == true)
                for (int i = 0; i < cluster.childClusters.Count; i++)
                    SetDeepShardColliders (scr, cluster.childClusters[i]);
        }
        
        /// /////////////////////////////////////////////////////////
        /// Cluster Colliders
        /// /////////////////////////////////////////////////////////   
        
        // Set cluster colliders by shards
        public static void CollectClusterColliders (RayfireRigid scr, RFCluster cluster)
        {
            // Reset original cluster colliders list
            if (scr.physics.cc == null)
                scr.physics.cc = new List<Collider>(cluster.shards.Count);
            else
                scr.physics.cc.Clear();
            
            // Collect all shards colliders
            CollectDeepColliders (scr, cluster);
        }
        
        // Check children for mesh or cluster root until all children will not be checked
        static void CollectDeepColliders (RayfireRigid scr, RFCluster cluster)
        {
            // Collect shards colliders
            for (int i = 0; i < cluster.shards.Count; i++)
                scr.physics.cc.Add (cluster.shards[i].col);

            // Set child cluster colliders
            if (scr.objTp == ObjectType.NestedCluster)
                if (cluster.HasChildClusters == true)
                    for (int i = 0; i < cluster.childClusters.Count; i++)
                        CollectDeepColliders (scr, cluster.childClusters[i]);
        }
        
        /// /////////////////////////////////////////////////////////
        /// Collider material
        /// /////////////////////////////////////////////////////////       
         
        // Set collider material
        public static void SetColliderMaterial(RayfireRigid scr)
        {
            // Set physics material if not defined by user
            if (scr.physics.ma == null)
                scr.physics.ma = scr.physics.PhysMaterial;
            
            // Set mesh collider material and stop
            if (scr.physics.mc != null)
            {
                scr.physics.mc.sharedMaterial = scr.physics.ma;
                return;
            }
            
            // Set cluster colliders material
            if (scr.physics.HasClusterColliders == true)
                for (int i = 0; i < scr.physics.cc.Count; i++)
                    scr.physics.cc[i].sharedMaterial = scr.physics.ma;
        }
        
        // Set shard collider material
        public static void SetColliderMaterial(RFPhysic physics, RFShard shard)
        {
            if (shard.col != null)
                shard.col.sharedMaterial = physics.ma;
        }
        
        /// /////////////////////////////////////////////////////////
        /// Collider properties
        /// /////////////////////////////////////////////////////////   
        
        // Set collider convex state
        public static void SetColliderConvex(RayfireRigid scr)
        {
            if (scr.physics.mc != null)
            {
                // Not Mesh collider
                if (scr.physics.mc is MeshCollider == false)
                    return;
                
                // Turn on convex for non kinematic
                MeshCollider mCol = (MeshCollider)scr.physics.mc;
                if (scr.physics.rb.isKinematic == false)
                    mCol.convex = true;
            }
        }
        
        // EDITOR clear colliders
        public static void DestroyColliders(RayfireRigid scr)
        {
            if (scr.physics.HasClusterColliders == true)
                for (int i = scr.physics.cc.Count - 1; i >= 0; i--)
                    if (scr.physics.cc[i] != null)
                        Object.DestroyImmediate (scr.physics.cc[i], true);
        }
        
        /// /////////////////////////////////////////////////////////
        /// RigidRoot
        /// /////////////////////////////////////////////////////////
        
        // Set rigidbody simType, mass, drag, solver iterations
        public static void SetPhysics(RayfireRigidRoot root)
        {
            // Set physics properties for rigidRoot shards
            SetPhysics (root.rigidRootShards, root.physics);
            
            // Set physics properties for meshRoot shards
            for (int i = 0; i < root.meshRootShards.Count; i++)
                SetPhysics (root.meshRootShards[i], root.meshRootShards[i].rigid.physics);
        }
        
        // Set shard Rigidbody and set physics properties. Uses for RigidRoot shards
        public static void SetPhysics(List<RFShard> shards, RFPhysic physic)
        {
            // Set phys props
            float density     = RayfireMan.inst.materialPresets.Density (physic.mt);
            float drag        = RayfireMan.inst.materialPresets.Drag (physic.mt);
            float dragAngular = RayfireMan.inst.materialPresets.AngularDrag (physic.mt);
            
            // Add Collider and Rigid body if has no Rigid component
            for (int i = 0; i < shards.Count; i++)
            {
                // Get rigidbody
                shards[i].rb = shards[i].tm.gameObject.GetComponent<Rigidbody>();
                
                // Set Rigid body
                if (shards[i].rb == null)
                    shards[i].rb = shards[i].tm.gameObject.AddComponent<Rigidbody>();
                
                // Set simulation
                SetSimulationType (shards[i].rb, shards[i].sm, ObjectType.Mesh, physic.gr, physic.si, physic.st);
                
                // Set density. After collider defined
                SetDensity (shards[i], physic, density);

                // Set drag properties
                SetDrag (shards[i], drag, dragAngular);
            }
        }
        
        // Set shard Rigidbody and set physics properties. Uses for RigidRoot -> MeshRoot shards
        public static void SetPhysics(RFShard shard, RFPhysic physic)
        {
            // Get rigidbody
            shard.rb = shard.tm.gameObject.GetComponent<Rigidbody>();
            
            // Set Rigid body
            if (shard.rb == null)
                shard.rb = shard.tm.gameObject.AddComponent<Rigidbody>();
            
            // Set simulation
            SetSimulationType (shard.rb, shard.sm, ObjectType.Mesh, physic.gr, physic.si, physic.st);
            
            // Set density. After collider defined
            SetDensity (shard, physic, RayfireMan.inst.materialPresets.Density (physic.mt));
            
            // Set drag properties
            SetDrag (shard, RayfireMan.inst.materialPresets.Drag (physic.mt), RayfireMan.inst.materialPresets.AngularDrag (physic.mt));
        }

        /// /////////////////////////////////////////////////////////
        /// Ignore colliders
        /// /////////////////////////////////////////////////////////
        
        // Pair structure
        struct RFIgnorePair
        {
            int a;
            int b;
            public RFIgnorePair(int A, int B)
            {
                a = A;
                b = B;
            }
        }
        
        // Set ignore list
        public static void SetIgnoreColliders(RFPhysic physics, List<RayfireRigid> rigids)
        {
            //float f1 = Time.realtimeSinceStartup;
         
            // Ignore colliders enabled
            if (physics.ine == true)
            {
                // Get ignore list if has no
                if (physics.HasIgnore == false)
                {
                    // Set bounds for Editor Setup
                    if (Application.isPlaying == false)
                    {
                        for (int i = 0; i < rigids.Count; i++)
                        {
                            if (rigids[i].mRnd == null)
                                rigids[i].mRnd = rigids[i].gameObject.GetComponent<MeshRenderer>();
                            if (rigids[i].mRnd != null)
                                rigids[i].lim.bound = rigids[i].mRnd.bounds;
                        }
                    }

                    // Collect bounds to check overlap
                    Bounds[] bounds = new Bounds[rigids.Count];
                    for (int i = 0; i < rigids.Count; i++)
                        bounds[i] = rigids[i].lim.bound;

                    // Get ignore list
                    physics.ign = Application.isPlaying == true 
                        ? GetIgnoreListFast (bounds) 
                        : GetIgnoreListShort (bounds);
                }
                                
                // Set physics ignore pairs. Runtime only
                if (Application.isPlaying == true)
                    IgnoreNeibCollision (rigids, physics.ign);
            }
            
            // Nullify if runtime
            if (Application.isPlaying == true)
                physics.ign = null;

            //Debug.Log (Time.realtimeSinceStartup - f1);
        }
        
        // Set ignore list
        public static void SetIgnoreColliders(RFPhysic physics, List<RFShard> shards)
        {
            // Ignore colliders enabled
            if (physics.ine == true)
            {
                // Get ignore list if has no
                if (physics.HasIgnore == false)
                    SetIgnoreListShards (physics, shards);
                
                // Set physics ignore pairs
                if (Application.isPlaying == true)
                    IgnoreNeibCollision (shards, physics.ign);
            }
            
            // Nullify if runtime
            if (Application.isPlaying == true)
                physics.ign = null;
        }
        
        // Ignore collision for overlapped shards
        public static void SetIgnoreListShards(RFPhysic physics, List<RFShard> shards)
        {
            // Collect bounds to check overlap
            Bounds[] bounds = new Bounds[shards.Count];
            for (int i = 0; i < shards.Count; i++)
                bounds[i] = shards[i].bnd;
            
            // Get ignore list
            physics.ign = Application.isPlaying == true
                ? GetIgnoreListFast (bounds)
                : GetIgnoreListShort (bounds);
        }
        
        // Ignore collision for overlapped shards
        public static List<int> GetIgnoreListFast(Bounds[] bounds)
        {
            // Get prune list
            List<int> pruneList = new List<int>();
            for (int s = 0; s < bounds.Length; s++)
            {
                for (int n = 0; n < bounds.Length; n++)
                {
                    if (s != n)
                    {
                        // Check bound intersection
                        if (bounds[s].Intersects (bounds[n]) == true)
                        {
                            pruneList.Add (s);
                            pruneList.Add (n);
                        }
                    }
                }
            }
            return pruneList;
        }
        
        // Ignore collision for overlapped shards
        public static List<int> GetIgnoreListShort(Bounds[] bounds)
        {
            RFIgnorePair          pair;
            HashSet<RFIgnorePair> ignorePairsHash = new HashSet<RFIgnorePair>();

            // Get prune list
            List<int> pruneList = new List<int>();
            for (int s = 0; s < bounds.Length; s++)
            {
                for (int n = 0; n < bounds.Length; n++)
                {
                    if (s != n)
                    {
                        // Check bound intersection
                        if (bounds[s].Intersects (bounds[n]) == true)
                        {
                            // Create pair
                            pair = new RFIgnorePair (s, n);

                            // Has no such pair yet
                            if (ignorePairsHash.Contains (pair) == false)
                            {
                                pruneList.Add (s);
                                pruneList.Add (n);

                                ignorePairsHash.Add (pair);
                                ignorePairsHash.Add (new RFIgnorePair (n, s));
                            }
                        }
                    }
                }
            }
            return pruneList;
        }
        
        // Ignore collision for overlapped shards
        public static void IgnoreNeibCollision(List<RayfireRigid> rigids, List<int> pr)
        {
            for (int s = 0; s < pr.Count / 2; s++)
                if (rigids[pr[s * 2 + 0]].physics.mc != null && rigids[pr[s * 2 + 1]].physics.mc != null)
                    Physics.IgnoreCollision (rigids[pr[s * 2 + 0]].physics.mc, rigids[pr[s * 2 + 1]].physics.mc, true);
        }
        
        // Ignore collision for overlapped shards
        public static void IgnoreNeibCollision(List<RFShard> shards, List<int> pr)
        {
            for (int s = 0; s < pr.Count / 2; s++)
                if (shards[pr[s * 2 + 0]].col != null && shards[pr[s * 2 + 1]].col != null)
                    Physics.IgnoreCollision (shards[pr[s * 2 + 0]].col, shards[pr[s * 2 + 1]].col, true);
        }
        
        /// /////////////////////////////////////////////////////////
        /// Getters
        /// /////////////////////////////////////////////////////////
        
        public bool HasIgnore { get { return ign != null && ign.Count > 0; } }
        public bool Destructible { get { return RayfireMan.inst.materialPresets.Destructible(mt); } }
        public int  Solidity     { get { return RayfireMan.inst.materialPresets.Solidity(mt); } }

        // Get Destructible state
        public bool HasClusterColliders
        {
            get
            {
                if (cc != null && cc.Count > 0)
                    return true;
                return false;
            }
        }
        
        // Get physic material
        public PhysicsMaterial PhysMaterial
        {
            get
            {
                // Return predefine material
                if (ma != null)
                    return ma;

                // Crete new material
                return RFMaterialPresets.PhysicMaterial(mt);
            }
        }
        
        // Bake getter properties
        public static void BakeProperties(RFPhysic physics)
        {
            // Set material solidity and destructible
            physics.solidity     = physics.Solidity;
            physics.destructible = physics.Destructible;
            
            // Set physics material if not defined by user
            if (physics.ma == null)
                physics.ma = physics.PhysMaterial;
        }
    }
}
