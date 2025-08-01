using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.Serialization;

public class IKHandler : MonoBehaviour
{
    [FormerlySerializedAs("ChainIKConstraint")]
    [SerializeField]
    private ChainIKConstraint chainIKConstraint;

    [FormerlySerializedAs("IKConstraint")]
    [SerializeField]
    private IKConstraintData iKConstraint;

    private Transform target;

    private Coroutine iKCoroutine;

    public void ReadyForAttack() {
        if (Physics.SphereCast(transform.position, 5f, Vector3.forward, out RaycastHit hit)) {
            if (hit.transform.TryGetComponent(out IDamagable damagable) && chainIKConstraint != null) {
                target = damagable.GetTargetPosition();
                chainIKConstraint.data.target.position = target.transform.position;
                if (iKCoroutine != null) {
                    StopCoroutine(iKCoroutine);
                }

                iKCoroutine = StartCoroutine(LerpChainIKWeight());
            }
        }
    }

    private IEnumerator LerpChainIKWeight() {
        float time = Time.deltaTime;
        chainIKConstraint.weight = 0;
        while (time < 1) {
            time += Time.deltaTime / iKConstraint.LerpTime;
            chainIKConstraint.weight = Mathf.Lerp(0, iKConstraint.MaxWeight, time);
            yield return null;
        }

        chainIKConstraint.weight = iKConstraint.MaxWeight;

        yield return new WaitForSeconds(iKConstraint.FullWeightDuration);
        time = 0;
        while (time < 1) {
            time -= Time.deltaTime / iKConstraint.LerpTime;
            chainIKConstraint.weight -= iKConstraint.MaxWeight * Time.deltaTime;
            yield return null;
        }

        chainIKConstraint.weight = 0;
    }

    [Serializable]
    private struct IKConstraintData
    {
        [field: SerializeField]
        public float LerpTime { get; private set; }

        [field: SerializeField]
        public float MaxWeight { get; private set; }

        [field: SerializeField]
        public float FullWeightDuration { get; private set; }
    }
}