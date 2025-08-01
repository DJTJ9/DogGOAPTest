    using UnityEngine;

    public interface IDamagable
    {
        public void TakeDamage(float damage);
        public void Demolish();
        
        public Transform GetTargetPosition();
    }
