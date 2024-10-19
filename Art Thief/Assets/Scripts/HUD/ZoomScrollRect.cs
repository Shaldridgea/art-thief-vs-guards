using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ZoomScrollRect : ScrollRect
{
    private float relativeSize = 1f;

    const float MAX_ZOOM = 1.2f;

    private float zoomFactor = 0f;

    protected override void Start()
    {
        base.Start();
    }

    public override void OnScroll(PointerEventData data)
    {
        // Store and set our scroll sensitivity to only affect zooming
        float zoomSens = scrollSensitivity;
        scrollSensitivity = 0f;
        base.OnScroll(data);
        Vector2 delta = data.scrollDelta;
        
        zoomFactor = Mathf.Clamp(zoomFactor + zoomSens * delta.y, 0f, 1f);
        SetZoom(zoomFactor);

        // Set our sensitivity back
        scrollSensitivity = zoomSens;
    }

    private void CalculateRelativeZoomSize()
    {
        if (viewport == null || content == null)
            return;

        // Calculate size of actual scroll content vs its viewport size
        // for how much to scale the content per zoom step
        // and for the minimum size it can be scaled to
        Vector2 viewportSize = viewport.rect.size;
        Vector2 contentSize = content.rect.size;
        relativeSize = Mathf.Max(viewportSize.x, viewportSize.y) / Mathf.Max(contentSize.x, contentSize.y);
    }

    public void SetZoom(float newZoomFactor)
    {
        // Find the point on the content that correlates with the viewport's centre
        Vector2 viewportCentrePoint = content.InverseTransformPoint(
            viewport.position +
            (new Vector3(viewport.rect.width / 2f, -viewport.rect.height / 2f) * viewport.root.localScale.x));

        zoomFactor = Mathf.Clamp(newZoomFactor, 0f, 1f);
        CalculateRelativeZoomSize();

        Vector3 contentScale = content.localScale;
        Vector3 newScale = Vector3.one * Mathf.Lerp(relativeSize, MAX_ZOOM, zoomFactor);
        content.localScale = newScale;

        if (contentScale != newScale)
        {
            // Set the content position after changing scale to be in the same place as before
            Vector2 contentSize = content.rect.size;
            Vector2 normalisedCentre = viewportCentrePoint / contentSize;
            Vector2 relativeChange = contentSize * contentScale - contentSize * newScale;
            relativeChange *= normalisedCentre;

            content.anchoredPosition += relativeChange;
        }
    }
}
