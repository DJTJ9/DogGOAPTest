﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace RayFire
{
    [AddComponentMenu (RFLog.act_path)]
    [HelpURL (RFLog.act_link)]
    public class RayfireActivator : MonoBehaviour
    {
        /// <summary>
        /// Rayfire Activator activation type.
        /// </summary>
        public enum ActivationType
        {
            OnTriggerEnter = 0,
            OnTriggerExit  = 1,
            OnCollision    = 2
        }

        /// <summary>
        /// Rayfire Activator animation type.
        /// </summary>
        public enum AnimationType
        {
            ByGlobalPositionList = 0,
            ByStaticLine         = 1,
            ByDynamicLine        = 2,
            ByLocalPositionList  = 5
        }

        /// <summary>
        /// Rayfire Activator gizmo type.
        /// </summary>
        public enum GizmoType
        {
            Box            = 1, 
            Sphere         = 0,
            Collider       = 2,
            ParticleSystem = 5
        }
        
        // UI
        public bool           checkRigid     = true;
        public bool           checkRigidRoot = true;
        public GizmoType      gizmoType;
        public float          sphereRadius = 5f;
        public Vector3        boxSize      = new Vector3 (5f, 2f, 5f);
        public bool           showGizmo    = true;
        public ActivationType type;
        public float          spdMin;
        public float          delay;
        public bool           demolishCluster;
        public bool           apply;
        public ForceMode      mode     = ForceMode.Impulse;
        public Vector3        velocity = Vector3.down;
        public int            velVar   = 20;
        public Vector3        spin;
        public bool           coord;
        public bool           showAnimation;
        public float          duration       = 3f;
        public float          scaleAnimation = 1f;
        public AnimationType  positionAnimation;
        public LineRenderer   line;
        public List<Vector3>  positionList;

        public int    mask      = -1;
        public string tagFilter = "Untagged";
        static string untagged  = "Untagged";
        
        public Collider                     activatorCollider;
        public ParticleSystem               ps;
        public List<ParticleCollisionEvent> collisionEvents;
        
        // Non serialized
        [NonSerialized] bool    animating;
        [NonSerialized] float   pathRatio;
        [NonSerialized] float   lineLength;
        [NonSerialized] float[] checkpoints;
        [NonSerialized] float   delta;
        [NonSerialized] float   deltaRatioStep;
        [NonSerialized] float   distDeltaStep;
        [NonSerialized] float   distRatio;
        [NonSerialized] float   timePassed;
        [NonSerialized] int     activeSegment;
        [NonSerialized] Vector3 positionStart;
        [NonSerialized] Vector3 scaleStart;

        [NonSerialized]        bool    speedCorState;
        [NonSerialized] public float   spdNow;
        [NonSerialized]        Vector3 spdPos;

        // TODO list of objects to check or all colliders objects

        /// /////////////////////////////////////////////////////////
        /// Common
        /// /////////////////////////////////////////////////////////

        // Start is called before the first frame update
        void Awake()
        {
            // Check collider and triggers
            if (gizmoType != GizmoType.ParticleSystem)
                SetCollider();
            else
                SetParticleSystem();
            
            // Start speed cor
            StartSpeedCor();

            // TODO no target check
            
            positionStart = transform.position;
            scaleStart    = transform.localScale;
        }

        /// /////////////////////////////////////////////////////////
        /// Enable/Disable
        /// /////////////////////////////////////////////////////////
        
        // Disable
        void OnDisable()
        {
            // Set coroutines states
            speedCorState = false;
        }

        // Activation
        void OnEnable()
        {
            // Start cors
            if (Application.isPlaying == true && gameObject.activeSelf == true && speedCorState == false)
                StartSpeedCor();
        }
        
        /// /////////////////////////////////////////////////////////
        /// Collision
        /// /////////////////////////////////////////////////////////
        
        // Collider collision activation
        void OnCollisionEnter (Collision collision)
        {
            if (type == ActivationType.OnCollision && gizmoType != GizmoType.ParticleSystem)
                ActivationCheck (collision.collider);
        }
        
        // Particle collision activation
        void OnParticleCollision (GameObject other)
        {
            if (type == ActivationType.OnCollision && gizmoType == GizmoType.ParticleSystem)
            {
                int numCollisionEvents = ps.GetCollisionEvents (other, collisionEvents);
                for (int i = 0; i < numCollisionEvents; i++)
                    ActivationCheck (collisionEvents[i].colliderComponent as Collider);
            }
        }

        /// /////////////////////////////////////////////////////////
        /// Trigger
        /// /////////////////////////////////////////////////////////

        // Activate on collider enter
        void OnTriggerEnter (Collider coll)
        {
            if (type == ActivationType.OnTriggerEnter)
                    ActivationCheck (coll);
        }

        // Activate on collider exit
        void OnTriggerExit (Collider coll)
        {
            if (type == ActivationType.OnTriggerExit)
                ActivationCheck (coll);
        }

        /// /////////////////////////////////////////////////////////
        /// Set 
        /// /////////////////////////////////////////////////////////

        // Prepare collider and triggers
        void SetCollider()
        {
            // Sphere collider
            if (gizmoType == GizmoType.Sphere)
            {
                SphereCollider col = gameObject.AddComponent<SphereCollider>();
                col.isTrigger     = type != ActivationType.OnCollision;
                col.radius        = sphereRadius;
                activatorCollider = col;
            }

            // Box collider
            if (gizmoType == GizmoType.Box)
            {
                BoxCollider col = gameObject.AddComponent<BoxCollider>();
                col.isTrigger     = type != ActivationType.OnCollision;
                col.size          = boxSize;
                activatorCollider = col;
            }

            // Custom colliders
            if (gizmoType == GizmoType.Collider)
            {
                Collider[] colliders = GetComponents<Collider>();
                if (colliders.Length == 0)
                    RayfireMan.Log (RFLog.act_dbgn + name + RFLog.act_noCol, gameObject);
                if (type != ActivationType.OnCollision)
                    for (int i = 0; i < colliders.Length; i++)
                        colliders[i].isTrigger = type != ActivationType.OnCollision;
            }
        }

        // Prepare particle system
        void SetParticleSystem()
        {
            collisionEvents = new List<ParticleCollisionEvent>();
            ps              = GetComponent<ParticleSystem>();
            if (ps != null)
            {
                ParticleSystem.CollisionModule cm = ps.collision;
                cm.enabled                = true;
                cm.enableDynamicColliders = true;
                cm.sendCollisionMessages  = true;
            }

            if (type != ActivationType.OnCollision)
            {
                type = ActivationType.OnCollision;
                RayfireMan.Log (RFLog.act_dbgn + name + RFLog.act_onCol, gameObject);
            }
        }

        /// /////////////////////////////////////////////////////////
        /// Speed 
        /// /////////////////////////////////////////////////////////

        // Start speed coroutine
        public void StartSpeedCor()
        {
            if (Application.isPlaying && spdMin > 0 && speedCorState == false)
                StartCoroutine (SpeedCor ());
        }
        
        // Speed coroutine
        IEnumerator SpeedCor ()
        {
            // Stop if running 
            if (speedCorState == true)
                yield break;

            // Set running state
            speedCorState = true;

            // Initial pos
            spdPos = transform.position;
            
            while (spdMin > 0)
            {
                // Set speed
                spdNow = Vector3.Distance(transform.position, spdPos) / Time.deltaTime;
                spdPos = transform.position;
                yield return null;
            }

            spdNow        = 0;
            speedCorState = false;
        }
        
        /// /////////////////////////////////////////////////////////
        /// Activation
        /// /////////////////////////////////////////////////////////
        
        static bool IsInLayerMask(int layer, LayerMask mask) => (mask.value & (1 << layer)) != 0;
        
        // Check for activation
        void ActivationCheck (Collider coll)
        {
            // Speed check
            if (spdMin > 0)
                if (spdNow < spdMin)
                    return;
            
            // Check for tag
            if (tagFilter != untagged && coll.gameObject.CompareTag (tagFilter) == false)
                return;

            // Check layer
            if (mask == 0)
                return;
            if (mask != -1)
                if (IsInLayerMask(coll.gameObject.layer, mask) == false)
                    return;
            
            // Activate
            if (checkRigid == true)
                RigidListActivationCheck (coll);
            if (checkRigidRoot == true)
                RigidRootActivationCheck (coll);
        }
        
        /// /////////////////////////////////////////////////////////
        /// Rigid Activation
        /// /////////////////////////////////////////////////////////
        
        // Check for Rigid activation
        void RigidListActivationCheck(Collider coll)
        {
            // Get rigid from collider or rigid body
            RayfireRigid rigid = coll.attachedRigidbody == null 
                ? coll.GetComponent<RayfireRigid>() 
                : coll.attachedRigidbody.GetComponent<RayfireRigid>();
            
            // Has no rigid
            if (rigid == null)
                return;
           
            // Mesh Root rigid
            if (rigid.objTp == ObjectType.MeshRoot)
                return;

            // Activation
            if (rigid.act.act == true)
            {
                if (rigid.simTp == SimType.Inactive || rigid.simTp == SimType.Kinematic)
                {
                    if (delay <= 0)
                        Activate (rigid);
                    else
                        StartCoroutine (DelayedActivationCor (rigid));
                }
            }
            
            // Connected cluster one fragment detach
            if (rigid.objTp == ObjectType.ConnectedCluster && demolishCluster == true)
            {
                if (delay <= 0)
                {
                    RFDemolitionCluster.DemolishCluster (rigid, new[] {coll});

                    // Init particles
                    // RFParticles.InitDemolitionParticles(rigid);
                }
                else
                    StartCoroutine (DelayedClusterCor (rigid, coll));
            }
        }
        
        // Exclude from simulation and keep object in scene
        IEnumerator DelayedActivationCor (RayfireRigid rigid)
        {
            // Wait life time
            yield return new WaitForSeconds (delay);
            
            // Activate
            if (rigid != null)
                Activate(rigid);;
        }
        
        // Demolish cluster
        IEnumerator DelayedClusterCor (RayfireRigid rigid, Collider coll)
        {
            // Wait life time
            yield return new WaitForSeconds (delay);

            // Activate
            if (rigid != null && coll != null)
                RFDemolitionCluster.DemolishCluster (rigid, new[] {coll});
        }
        
        // ActivateRigid
        void Activate(RayfireRigid rigid)
        {
            // Activate
            rigid.Activate();

            // Add force
            AddForce (rigid.physics.rb);
        }
        
        /// /////////////////////////////////////////////////////////
        /// Rigid Root Activation
        /// /////////////////////////////////////////////////////////
        
        // Check for Rigid activation
        void RigidRootActivationCheck(Collider coll)
        {
            // TODO cache activated collider and skip them before get component in parent
            
            // TODO register all RigidRoots and their gameobjects in manager and check for them by coll.gameobject 
            
            // Has no rigid root as parent
            if (coll.transform.parent == null)
                return;
            
            // Get rigid root
            RayfireRigidRoot rigidRoot = null;
            if (coll.transform.parent != null)
                rigidRoot = coll.transform.parent.GetComponentInParent<RayfireRigidRoot>();
            
            // Has no rigid
            if (rigidRoot == null)
                return;
                
            // Activation
            if (rigidRoot.activation.act == true)
                if (rigidRoot.simTp == SimType.Inactive || rigidRoot.simTp == SimType.Kinematic)
                {
                    if (delay <= 0)
                        ActivateCollider(rigidRoot, coll);
                    else
                        StartCoroutine (DelayedActivationCor (rigidRoot, coll));
                }
        }
        
        // Exclude from simulation and keep object in scene
        IEnumerator DelayedActivationCor (RayfireRigidRoot rigidRoot, Collider coll)
        {
            // Wait life time
            yield return new WaitForSeconds (delay);

            // Activate
            if (rigidRoot != null)
                ActivateCollider(rigidRoot, coll);
        }
        
        // Activate shard by collider
        void ActivateCollider (RayfireRigidRoot rigidRoot, Collider coll)
        {
            for (int i = rigidRoot.inactiveShards.Count - 1; i >= 0; i--)
            {
                if ( rigidRoot.inactiveShards[i].col == coll)
                {
                    // Activate and remove if activated
                    if (RFActivation.ActivateShard (rigidRoot.inactiveShards[i], rigidRoot) == true)
                    {
                        // Add force
                        AddForce (rigidRoot.inactiveShards[i].rb);
                        
                        // Remove from list
                        rigidRoot.inactiveShards.RemoveAt (i);
                    }

                    // Break because collider matched shard
                    break;
                }
            }
        }

        /// /////////////////////////////////////////////////////////
        /// Force
        /// /////////////////////////////////////////////////////////

        // Add force to rigidbody
        void AddForce(Rigidbody rb)
        {
            if (apply == true)
            {
                // Velocity
                if (velocity != Vector3.zero)
                {
                    Vector3 vel = velocity;

                    // Add variation
                    if (velVar > 0)
                    {
                        vel.x += Random.Range (0f, velocity.x / 100f * velVar);
                        vel.y += Random.Range (0f, velocity.y / 100f * velVar);
                        vel.z += Random.Range (0f, velocity.z / 100f * velVar);
                    }

                    if (coord == false)
                        rb.AddForce (vel, mode);
                    else
                        rb.AddForce (transform.TransformDirection (vel), mode);
                }

                // Angular velocity
                if (spin != Vector3.zero)
                {
                    rb.AddTorque (spin, mode);
                }
            }
        }
        
        /// /////////////////////////////////////////////////////////
        /// Animation
        /// /////////////////////////////////////////////////////////

        // Trigger animation start
        public void StartAnimation()
        {
            // Already animating
            if (animating == true)
                return;

            // Set animation data
            SetAnimation();

            // Positions check
            if (positionList.Count < 2 && scaleAnimation == 1f)
            {
                RayfireMan.Log (RFLog.act_dbgn + name + RFLog.act_noPos, gameObject);
                return;
            }

            // Start animation
            StartCoroutine (AnimationCor());
        }

        // Set animation adata
        void SetAnimation()
        {
            // Set points
            if (ByLine == true)
                SetWorldPointsByLine();
            
            // Set points
            if (positionAnimation == AnimationType.ByLocalPositionList)
                SetWorldPointsByLocal();

            // Set ration checkpoints
            SetCheckPoints();
        }

        // Set points by line
        void SetWorldPointsByLine()
        {
            // Null check
            if (line == null)
            {
                RayfireMan.Log (RFLog.act_dbgn + name + RFLog.act_noLine, gameObject);
                return;
            }

            // Set points
            positionList = new List<Vector3>();
            for (int i = 0; i < line.positionCount; i++)
                positionList.Add (line.transform.TransformPoint (line.GetPosition (i)));

            // Add first point if looped
            if (line.loop == true)
                positionList.Add (positionList[0]);
        }
        
        // Set points by line
        void SetWorldPointsByLocal()
        {
            // Positions check
            if (positionList.Count < 2)
                return;

            // List of world positions with current position as start
            List<Vector3> worldPoints = new List<Vector3>(){transform.position};
            for (int i = 1; i < positionList.Count; i++)
                worldPoints.Add (transform.position + positionList[i]);
            
            // Set to position list
            positionList.Clear();
            positionList = worldPoints;
        }
        
        // Set ration checkpoints
        void SetCheckPoints()
        {
            // Positions check
            if (positionList.Count < 2)
                return;

            // Total and segments length
            lineLength = 0f;
            List<float> segmentsLength = new List<float>();
            if (positionList.Count >= 2)
            {
                for (int i = 0; i < positionList.Count - 1; i++)
                {
                    float length = Vector3.Distance (positionList[i], positionList[i + 1]);
                    segmentsLength.Add (length);
                    lineLength += length;
                }
            }

            // Get segments ration checkpoints
            float sum = 0f;
            checkpoints = new float[segmentsLength.Count + 1];
            for (int i = 0; i < segmentsLength.Count; i++)
            {
                float localRation = segmentsLength[i] / lineLength * 100f;
                checkpoints[i] = sum;
                sum += localRation;
            }

            checkpoints[segmentsLength.Count] = 100f;
        }

        //Animation over line coroutine
        IEnumerator AnimationCor()
        {
            // Stop
            if (animating == true)
                yield break;

            // Set state On
            animating = true;

            // Set starting position
            if (positionList.Count >= 2)
                transform.position = positionList[0];

            while (timePassed < duration)
            {
                // Stop
                if (animating == false)
                    yield break;

                // Update all info for dynamic line
                if (positionAnimation == AnimationType.ByDynamicLine)
                    SetAnimation();

                // Prepare info
                delta      =  Time.deltaTime;
                timePassed += delta;

                // Position animation
                if (positionList.Count >= 2)
                {
                    // Increase time and path ratio
                    deltaRatioStep =  delta / duration;
                    distDeltaStep  =  lineLength * deltaRatioStep;
                    distRatio      =  distDeltaStep / lineLength * 100f;
                    pathRatio      += distRatio;

                    // Get active line segment
                    activeSegment = GetSegment (pathRatio);
                    float   segmentRate = (checkpoints[activeSegment + 1] - pathRatio) / (checkpoints[activeSegment + 1] - checkpoints[activeSegment]);
                    Vector3 stepPos     = Vector3.Lerp (positionList[activeSegment + 1], positionList[activeSegment], segmentRate);
                    transform.position = stepPos;
                }

                // Scale animation
                if (scaleAnimation > 1f)
                {
                    float   scaleRate = timePassed / duration;
                    Vector3 maxScale  = new Vector3 (scaleAnimation, scaleAnimation, scaleAnimation);
                    Vector3 newScale  = Vector3.Lerp (scaleStart, maxScale, scaleRate);
                    transform.localScale = newScale;
                }

                // Wait
                yield return null;
            }

            // Reset data
            ResetData();
        }

        // Get active segment id
        int GetSegment (float ration)
        {
            if (checkpoints.Length > 2)
            {
                for (int i = 0; i < checkpoints.Length - 1; i++)
                    if (ration > checkpoints[i] && ration < checkpoints[i + 1])
                        return i;
                return checkpoints.Length - 2;
            }

            return 0;
        }

        // Reset animation info
        void ResetData()
        {
            animating      = false;
            pathRatio      = 0f;
            lineLength     = 0f;
            checkpoints    = null;
            delta          = 0f;
            deltaRatioStep = 0f;
            distDeltaStep  = 0f;
            distRatio      = 0f;
            timePassed     = 0f;
            activeSegment  = 0;
        }

        // Stop animation
        public void StopAnimation()
        {
            animating = false;
        }

        // Stop animation
        public void ResetAnimation()
        {
            // Reset info
            ResetData();

            // Reset position
            transform.position = positionStart;
        }

        // Add new position
        public void AddPosition (Vector3 newPos)
        {
            // Only for global and local
            if (ByLine == true)
            {
                RayfireMan.Log (RFLog.act_dbgn + name + RFLog.act_savePos, gameObject);
                return;
            }
            
            // Create list
            if (positionList == null)
                positionList = new List<Vector3>();

            // Same position
            if (positionList.Count > 0 && newPos == positionList[positionList.Count - 1])
            {
                RayfireMan.Log (RFLog.act_dbgn + name + RFLog.act_samePos, gameObject);
                return;
            }

            // Save global position
            if (positionAnimation == AnimationType.ByGlobalPositionList)
            {
                // Check for empty list or same position
                if (positionList.Count == 0 || newPos != positionList[positionList.Count - 1])
                    positionList.Add (newPos);
            }
            
            // Save global position
            if (positionAnimation == AnimationType.ByLocalPositionList)
            {
                // First position in world space to save other position in local space relative to first position
                if (positionList.Count == 0)
                    positionList.Add (newPos);
    
                // Other positions in local space relative to first
                else
                    positionList.Add (newPos - positionList[0]);
            }
        }

        /// /////////////////////////////////////////////////////////
        /// Gizmo
        /// /////////////////////////////////////////////////////////

        // Set gizmo
        public void SetGizmoType (GizmoType gizmo)
        {
            gizmoType = gizmo;
            
            // Set new collider
            if (Application.isPlaying == true)
            {
                // Destroy existing collider
                if (activatorCollider != null)
                    Destroy (activatorCollider);
                
                // Set new collider
                SetCollider();
            }
        }
        
        /// /////////////////////////////////////////////////////////
        /// Getters
        /// /////////////////////////////////////////////////////////

        public bool ByPositions
        {
            get {
                return positionAnimation == AnimationType.ByLocalPositionList ||
                       positionAnimation == AnimationType.ByGlobalPositionList;
            }
        }
        
        public bool ByLine
        {
            get {
                return positionAnimation == AnimationType.ByStaticLine ||
                       positionAnimation == AnimationType.ByDynamicLine;
            }
        }

        public float SpeedMin
        {
            get { return spdMin; }
            set
            {
                spdMin = value;
                if (Application.isPlaying == true)
                    StartSpeedCor();
            }
        }
    }
}