using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialInfo : MonoBehaviour
{
    public GameObject JumpInfo;
    public GameObject HoldJumpInfo;
    public GameObject RollInfo;
    public GameObject InvertGravityInfo;

    void Start()
    {
        StartCoroutine(MethodenAbfolge());
    }

    IEnumerator MethodenAbfolge()
    {
        yield return StartCoroutine(ShowJumpInfo());
        yield return StartCoroutine(ShowHoldJumpInfo());
        yield return StartCoroutine(ShowRollInfo());
        yield return StartCoroutine(ShowInvertGravityInfo());
    }

    IEnumerator ShowJumpInfo()
    {
        yield return new WaitForSeconds(2);
        JumpInfo.SetActive(true);
        yield return new WaitForSeconds(9);
        JumpInfo.SetActive(false);
        yield return new WaitForSeconds(1f);
    }

    IEnumerator ShowHoldJumpInfo()
    {
        HoldJumpInfo.SetActive(true);
        yield return new WaitForSeconds(8);
        HoldJumpInfo.SetActive(false);
        yield return new WaitForSeconds(1f);
    }

    IEnumerator ShowRollInfo()
    {
        RollInfo.SetActive(true);
        yield return new WaitForSeconds(8);
        RollInfo.SetActive(false);
        yield return new WaitForSeconds(6.5f);
    }

    IEnumerator ShowInvertGravityInfo()
    {
        InvertGravityInfo.SetActive(true);
        yield return new WaitForSeconds(2.7f);
        InvertGravityInfo.SetActive(false);
    }
}
