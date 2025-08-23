using System;
using UnityEngine;
using UnityEngine.UI;

public class ChangeButtonColor : MonoBehaviour
{
    [SerializeField]
    private Image buttonImage;
    
    [SerializeField]
    private Color activeColor;
    [SerializeField]
    private Color inactiveColor;

    private void Update()
    {
        buttonImage.color = MusicManager.Instance.isLooping ? activeColor : inactiveColor;
    }
}
