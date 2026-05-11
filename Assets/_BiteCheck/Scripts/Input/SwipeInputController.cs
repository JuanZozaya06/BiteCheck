using System;
using UnityEngine;

namespace BiteCheck.Input
{
    public class SwipeInputController : MonoBehaviour
    {
        [SerializeField] private float minSwipeDistance = 120f;
        [SerializeField] private float maxVerticalRatio = 0.6f;
        [SerializeField] private bool debugLogs;

        private bool locked;
        private bool trackingSwipe;
        private bool swipeConsumed;
        private int activeTouchId = -1;
        private Vector2 startPosition;

        public event Action OnSwipeLeft;
        public event Action OnSwipeRight;

        private void Update()
        {
            if (locked)
            {
                return;
            }

            if (UnityEngine.Input.touchSupported && UnityEngine.Input.touchCount > 0)
            {
                HandleTouchInput();
                return;
            }

            HandleMouseInput();
        }

        public void LockInput()
        {
            locked = true;
            ResetInput();
        }

        public void UnlockInput()
        {
            locked = false;
            ResetInput();
        }

        public void ResetInput()
        {
            trackingSwipe = false;
            swipeConsumed = false;
            activeTouchId = -1;
            startPosition = Vector2.zero;
        }

        private void HandleMouseInput()
        {
            if (UnityEngine.Input.GetMouseButtonDown(0))
            {
                BeginSwipe(UnityEngine.Input.mousePosition);
            }
            else if (UnityEngine.Input.GetMouseButtonUp(0))
            {
                EndSwipe(UnityEngine.Input.mousePosition);
            }
        }

        private void HandleTouchInput()
        {
            for (int i = 0; i < UnityEngine.Input.touchCount; i++)
            {
                Touch touch = UnityEngine.Input.GetTouch(i);

                if (!trackingSwipe && touch.phase == TouchPhase.Began)
                {
                    activeTouchId = touch.fingerId;
                    BeginSwipe(touch.position);
                    return;
                }

                if (trackingSwipe && touch.fingerId == activeTouchId)
                {
                    if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                    {
                        EndSwipe(touch.position);
                    }

                    return;
                }
            }
        }

        private void BeginSwipe(Vector2 position)
        {
            trackingSwipe = true;
            swipeConsumed = false;
            startPosition = position;
        }

        private void EndSwipe(Vector2 endPosition)
        {
            if (!trackingSwipe || swipeConsumed)
            {
                ResetInput();
                return;
            }

            Vector2 delta = endPosition - startPosition;
            TryResolveSwipe(delta);
            ResetInput();
        }

        private void TryResolveSwipe(Vector2 delta)
        {
            float horizontalDistance = Mathf.Abs(delta.x);
            float verticalDistance = Mathf.Abs(delta.y);

            if (horizontalDistance < minSwipeDistance)
            {
                LogIgnoredSwipe("too short", delta);
                return;
            }

            if (verticalDistance > horizontalDistance * maxVerticalRatio)
            {
                LogIgnoredSwipe("too vertical", delta);
                return;
            }

            swipeConsumed = true;

            if (delta.x < 0f)
            {
                LogSwipe("left", delta);
                OnSwipeLeft?.Invoke();
            }
            else
            {
                LogSwipe("right", delta);
                OnSwipeRight?.Invoke();
            }
        }

        private void LogSwipe(string direction, Vector2 delta)
        {
            if (!debugLogs)
            {
                return;
            }

            Debug.Log($"Swipe {direction}: {delta}", this);
        }

        private void LogIgnoredSwipe(string reason, Vector2 delta)
        {
            if (!debugLogs)
            {
                return;
            }

            Debug.Log($"Ignored swipe ({reason}): {delta}", this);
        }
    }
}
