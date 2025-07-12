using ImprovedTimers;
using UnityEngine;
using UnityEngine.AI;

// public class DropBallStrategy : IActionStrategy
// {
//     public bool CanPerform => true;
//     public bool Complete   { get; private set; }
//     
//     private readonly NavMeshAgent agent;
//     private readonly AnimationController animations;
//     private readonly GameObject ball;
//     private readonly float dropRange;
//     
//     private CountdownTimer dropAnimationTimer;
//     
//     public DropBallStrategy(NavMeshAgent agent, AnimationController animations, GameObject ball, Transform objectGrabPoint, float dropRange = 2f) {
//         this.agent = agent;
//         this.animations = animations;
//         this.ball = ball;
//         this.dropRange = dropRange;
//
//         dropAnimationTimer = new CountdownTimer(2.4f);
//         dropAnimationTimer.OnTimerStart += () => {
//             animations.Eat();
//             Complete = false;
//         };
//         dropAnimationTimer.OnTimerStop += () => {
//             animations.Locomotion();
//             if (ball.TryGetComponent(out GrabbableObject grabbableObject)) { // Animation Event beim Aufheben und Droppen
//                 grabbableObject.Grab(objectGrabPoint);
//             }
//             Complete = true;
//         };
//     }
//
//     public void Start() {
//         dropAnimationTimer.Start();
//         agent.stoppingDistance = dropRange - 0.2f;
//         agent.SetDestination(ball.transform.position);
//     }
//     
//     public void Update(float deltaTime) {
//         dropAnimationTimer.Tick(deltaTime);
//     }
//     
//     public void Stop() {
//         dropAnimationTimer.Stop();
//         animations.Locomotion();
//         Complete = true;
//     }
// }