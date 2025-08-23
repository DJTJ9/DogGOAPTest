using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonHoverEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Vector3 normalScale = Vector3.one;
    public Vector3 hoverScale = new Vector3(1.2f, 1.2f, 1f);
    public float scaleSpeed = 10f;

    private Vector3 targetScale;

    void Start()
    {
        targetScale = normalScale;
        transform.localScale = normalScale;
    }

    void OnEnable()
    {
        targetScale = normalScale;
        transform.localScale = normalScale;
    }
    
    void OnDisable()
    {
        transform.localScale = normalScale;
        transform.localScale = normalScale;
    }
    
    void Update()
    {
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * scaleSpeed);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        targetScale = hoverScale;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        targetScale = normalScale;
    }
}
