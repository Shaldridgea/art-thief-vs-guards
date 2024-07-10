using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ZoomScrollRect : ScrollRect
{
    protected override void Start()
    {
        base.Start();
    }

    public override void OnScroll(PointerEventData data)
    {
        Vector2 viewportSize = viewport.rect.size;
        float zoomSens = scrollSensitivity;
        scrollSensitivity = 0f;
        base.OnScroll(data);
        Vector2 delta = data.scrollDelta;
        //delta *= 1f;
        Vector2 contentSize = content.rect.size;

        float relativeSize = viewportSize.magnitude / contentSize.magnitude;

        content.localScale += relativeSize * zoomSens * new Vector3(delta.y, delta.y, delta.y);
        content.localScale = new Vector3(
            Mathf.Clamp(content.localScale.x, relativeSize, 1.5f),
            Mathf.Clamp(content.localScale.y, relativeSize, 1.5f),
            Mathf.Clamp(content.localScale.z, relativeSize, 1.5f));
        scrollSensitivity = zoomSens;
    }
}
