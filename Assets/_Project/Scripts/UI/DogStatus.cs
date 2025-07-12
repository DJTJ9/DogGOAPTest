using System;
using System.Collections;
using UnityEngine;
using ImprovedTimers;
using Unity.VisualScripting;
using UnityEngine.Serialization;
using UnityEngine.UI;
using DG.Tweening;

public class DogStatus : MonoBehaviour
{
    [SerializeField]
    private GameObject goapAgent;

    [SerializeField]
    private GameObject player;

    [SerializeField]
    private string sentence;

    [SerializeField]
    private float duration;
    
    [SerializeField]
    private GameObject textMeshAsset;

    [SerializeField]
    private RawImage bubble;

    public AnimationCurve bubbleAppearCurve;
    
    private GameObject m_TextObject;
    private CountdownTimer m_waitTimer;

    private IEnumerator Start() {
        yield return new WaitForSeconds(2);
        
        yield return bubble.transform.DOScale(Vector3.one, duration).SetEase(bubbleAppearCurve).WaitForCompletion();
        
        Debug.Log("Tween finished!");
        
        //yield return bubble.transform.DOScale(Vector3.one, duration).WaitForCompletion();

        //yield return bubble.transform.DOScale(Vector3.one, duration).WaitForCompletion();
    
    }

    private void Awake() {
        bubble.gameObject.SetActive(true);
        
        m_waitTimer = new CountdownTimer(duration);

        m_waitTimer.OnTimerStop += () => {
            bubble.gameObject.SetActive(false);
        };
    }

    private void LateUpdate() {
    bubble.transform.LookAt(player.transform);
    }

    
    public void CreateTextObject(string text, GameObject parent)
    {
        Vector3 pos = GetBoundsOffset(parent);
        m_TextObject = Instantiate(textMeshAsset, parent.transform, true);
        m_TextObject.GetComponent<TMPro.TextMeshPro>().text = text;

        m_TextObject.transform.localPosition = pos;
        m_TextObject.transform.rotation = GetTextLookRotation();
    }

    private Vector3 GetBoundsOffset(GameObject gameObject)
    {
        MeshFilter parentMesh = gameObject.GetComponent<MeshFilter>();
        if (parentMesh != null)
        {
            return GetBoundsOffset(parentMesh.mesh.bounds);
        }
        Collider parentCollider = gameObject.GetComponent<Collider>();
        if (parentCollider != null)
        {
            return GetBoundsOffset(parentCollider.bounds);
        }
        return Vector3.zero;
    }

    private Vector3 GetBoundsOffset(Bounds bounds)
    {
        return new Vector3(0.0f, bounds.max.y + 0.05f);
    }

    private Quaternion GetTextLookRotation()
    {
        Vector3 cameraForward = Camera.main.transform.forward;
        cameraForward.y = 0.0f;
        cameraForward.Normalize();
        return Quaternion.LookRotation(cameraForward);
    }
}