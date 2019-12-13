using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Island.UI
{
    /// <summary>
    /// Canvas的淡入淡出
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public class Fader : MonoBehaviour
    {
        public enum FaderState
        {
            FadingIn, FadingOut, FadedIn, FadedOut, Null
        }

        public FaderState faderState;
        public float fadingSpeed = 1;

        private CanvasGroup _canvasGroup;

        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
        }

        public UnityAction onFadeIn;
        public UnityAction onFadeOut;

        void Update()
        {
            switch (faderState)
            {
                case FaderState.FadingIn:
                    _canvasGroup.alpha += fadingSpeed * Time.deltaTime;
                    if (_canvasGroup.alpha >= 1)
                    {
                        faderState = FaderState.FadedIn;
                        onFadeIn?.Invoke();
                    }
                    break;
                case FaderState.FadingOut:
                    _canvasGroup.alpha -= fadingSpeed * Time.deltaTime;
                    if (_canvasGroup.alpha <= 0)
                    {
                        faderState = FaderState.FadedOut;
                        onFadeOut?.Invoke();
                    }
                    break;
                default:
                    break;
            }
        }

        public static Fader CreateGlobalFader()
        {
            var faderObj = new GameObject();
            var canvas = faderObj.AddComponent<Canvas>();
            var canvasGroup = faderObj.AddComponent<CanvasGroup>();
            var fader = faderObj.AddComponent<Fader>();

            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = (int)UILayer.GlobalFader;
            canvasGroup.alpha = 0;

            var fadeImageObj = new GameObject();
            fadeImageObj.transform.parent = canvas.transform;
            var fadeImage = fadeImageObj.AddComponent<Image>();

            var rectTransform = fadeImageObj.GetComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.anchoredPosition = Vector2.zero;
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, canvas.pixelRect.height);
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, canvas.pixelRect.width);
            fadeImage.color = Color.black;

            return fader;
        }
    }
}
