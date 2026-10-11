using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Book))]
public class AutoFlip : MonoBehaviour
{
    public FlipMode Mode;
    public float PageFlipTime = 0.7f;
    public float TimeBetweenPages = 1;
    public float DelayBeforeStarting = 0;
    public bool AutoStartFlip = true;
    public Book ControledBook;
    public int AnimationFramesCount = 40;
    public bool IsFlipping { get { return isFlipping; } }

    private bool isFlipping;
    private bool previousInteractable;

    private void Awake()
    {
        if (!ControledBook) ControledBook = GetComponent<Book>();
    }

    private void OnEnable()
    {
        if (!ControledBook) ControledBook = GetComponent<Book>();
        ControledBook.OnFlip.AddListener(PageFlipped);
    }

    private void Start()
    {
        if (AutoStartFlip) StartFlipping();
    }

    private void OnDisable()
    {
        CancelFlip();
        if (ControledBook) ControledBook.OnFlip.RemoveListener(PageFlipped);
    }

    private void PageFlipped()
    {
        if (isFlipping && ControledBook) ControledBook.interactable = previousInteractable;
        isFlipping = false;
    }

    public void CancelFlip()
    {
        StopAllCoroutines();
        if (isFlipping && ControledBook)
        {
            ControledBook.CancelPageFlip();
            ControledBook.interactable = previousInteractable;
        }
        isFlipping = false;
    }

    public void StartFlipping()
    {
        if (!isActiveAndEnabled) return;
        CancelFlip();
        StartCoroutine(FlipToEnd());
    }

    public void FlipRightPage() { BeginFlip(FlipMode.RightToLeft); }
    public void FlipLeftPage() { BeginFlip(FlipMode.LeftToRight); }

    private void BeginFlip(FlipMode direction)
    {
        if (!isActiveAndEnabled || isFlipping || !ControledBook) return;
        if (ControledBook.TotalPageCount == 0) return;
        if (direction == FlipMode.RightToLeft && ControledBook.currentPage >= ControledBook.TotalPageCount) return;
        if (direction == FlipMode.LeftToRight && ControledBook.currentPage <= 0) return;
        isFlipping = true;
        previousInteractable = ControledBook.interactable;
        // The book's mouse-follow update must not override the animated page corner.
        ControledBook.interactable = false;
        StartCoroutine(AnimatePage(direction));
    }

    private IEnumerator AnimatePage(FlipMode direction)
    {
        float center = (ControledBook.EndBottomRight.x + ControledBook.EndBottomLeft.x) * 0.5f;
        float halfWidth = (ControledBook.EndBottomRight.x - ControledBook.EndBottomLeft.x) * 0.45f;
        float height = Mathf.Abs(ControledBook.EndBottomRight.y) * 0.9f;
        if (halfWidth <= Mathf.Epsilon)
        {
            PageFlipped();
            yield break;
        }

        bool forward = direction == FlipMode.RightToLeft;
        float startX = center + (forward ? halfWidth : -halfWidth);
        Vector3 start = new Vector3(startX, -height, 0f);
        if (forward) ControledBook.DragRightPageToPoint(start);
        else ControledBook.DragLeftPageToPoint(start);

        float duration = Mathf.Max(0.1f, PageFlipTime);
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / duration));
            float x = Mathf.Lerp(startX, center + (forward ? -halfWidth : halfWidth), t);
            float offset = x - center;
            float y = -height * offset * offset / (halfWidth * halfWidth);
            Vector3 corner = new Vector3(x, y, 0f);
            if (forward) ControledBook.UpdateBookRTLToPoint(corner);
            else ControledBook.UpdateBookLTRToPoint(corner);
            yield return null;
        }
        ControledBook.ReleasePage();
    }

    private IEnumerator FlipToEnd()
    {
        yield return new WaitForSecondsRealtime(Mathf.Max(0f, DelayBeforeStarting));
        while (ControledBook && (Mode == FlipMode.RightToLeft
            ? ControledBook.currentPage < ControledBook.TotalPageCount
            : ControledBook.currentPage > 0))
        {
            BeginFlip(Mode);
            yield return new WaitUntil(() => !isFlipping);
            yield return new WaitForSecondsRealtime(Mathf.Max(0f, TimeBetweenPages));
        }
    }
}