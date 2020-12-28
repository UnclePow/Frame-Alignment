using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MyJoy : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public GameObject joyBG;
    public GameObject joy;
    private float radius;

    private void Start()
    {
        HideJoy();
        radius = joyBG.GetComponent<RectTransform>().sizeDelta.x / 2;
    }

    private void ShowJoy()
    {
        joyBG.SetActive(true);
        joy.SetActive(true);
    }

    private void HideJoy()
    {
        joyBG.SetActive(false);
        joy.SetActive(false);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {

    }

    Vector3 dir;
    Vector3 relativePos;
    int angle;
    public void OnDrag(PointerEventData eventData)
    {
        //joy.GetComponent<RectTransform>().position = Input.mousePosition;
        dir = Input.mousePosition - joyBG.GetComponent<RectTransform>().position;
        relativePos = Vector3.ClampMagnitude(dir, radius);
        joy.GetComponent<RectTransform>().localPosition = relativePos;
        angle = (int)Vector3.Angle(dir, new Vector3(1, 0, 0));
        if (dir.y < 0)
            angle = 360 - angle;
        angle = angle / 3;

        BattleData.Instance.UpdateMoveDir(angle);
        //Debug.Log(angle);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        joy.GetComponent<RectTransform>().localPosition = Vector3.zero;
        BattleData.Instance.UpdateMoveDir(121);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        ShowJoy();
        joyBG.GetComponent<RectTransform>().position = Input.mousePosition;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        HideJoy();
    }
}
