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

        // Store and set our scroll sensitivity to only affect zooming
        float zoomSens = scrollSensitivity;
        scrollSensitivity = 0f;
        base.OnScroll(data);
        Vector2 delta = data.scrollDelta;

        Vector2 contentSize = content.rect.size;

        // Calculate size of actual scroll content vs its viewport size
        // for how much to scale the content per zoom step
        // and for the minimum size it can be scaled to
        float relativeSize = Mathf.Max(viewportSize.x, viewportSize.y) / Mathf.Max(contentSize.x, contentSize.y);

        content.localScale += relativeSize * zoomSens * new Vector3(delta.y, delta.y, delta.y);
        content.localScale = new Vector3(
            Mathf.Clamp(content.localScale.x, relativeSize, 1.5f),
            Mathf.Clamp(content.localScale.y, relativeSize, 1.5f),
            Mathf.Clamp(content.localScale.z, relativeSize, 1.5f));

        // Set our sensitivity back
        scrollSensitivity = zoomSens;
    }
}
