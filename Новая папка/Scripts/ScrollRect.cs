using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(ScrollRectsss))]
public class ScrollRectsss : MonoBehaviour
{
    [Header("Ограничения прокрутки")]
    [Tooltip("Максимальное смещение сверху в пикселях")]
    public float maxTopOffset = 100f; 

    private ScrollRect scrollRect;
    private RectTransform content;
    private bool isDragging = false;

    private void Awake()
    {
        scrollRect = GetComponent<ScrollRect>();
        content = scrollRect.content;

       
        scrollRect.movementType = ScrollRect.MovementType.Elastic;
        scrollRect.elasticity = 0.1f;
    }

    private void Update()
    {
        if (isDragging)
        {
            ClampScrollPosition();
        }
    }

    public void OnBeginDrag()
    {
        isDragging = true;
    }

    public void OnEndDrag()
    {
        isDragging = false;
        ClampScrollPosition();
    }

    private void ClampScrollPosition()
    {
        
        Vector3 contentPos = content.anchoredPosition;

     
        float maxY = Mathf.Min(maxTopOffset, content.rect.height - scrollRect.viewport.rect.height);

        
        contentPos.y = Mathf.Clamp(contentPos.y, -maxY, maxTopOffset);

       
        content.anchoredPosition = contentPos;
    }
}