using Island.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Island.UI
{
    [RequireComponent(typeof(Canvas))]
    public sealed class Pannel : MonoBehaviour
    {
        public enum PannelAnimType
        {
            Animation, Fader
        }

        private Fader _fader;
        private Animation _animation;
        public Canvas Canvas { get; private set; }

        public AnimationClip showAnim;
        public AnimationClip closeAnim;

        public PannelAnimType pannelAnimType;
        private bool _isClosing;

        private void Awake()
        {
            if (pannelAnimType == PannelAnimType.Fader)
                _fader = GetComponent<Fader>();
            else
                _animation = GetComponent<Animation>();
            Canvas = GetComponent<Canvas>();
        }

        private void Start()
        {
            if (pannelAnimType == PannelAnimType.Fader)
                _fader.faderState = Fader.FaderState.FadingIn;
            else
            {
                _animation.AddClip(showAnim, "show");
                _animation.AddClip(closeAnim, "close");

                _animation.Play("show");
            }
        }

        public void Update()
        {
            if (pannelAnimType == PannelAnimType.Fader)
                return;

            if (_isClosing && !_animation.isPlaying)
                Destroy(gameObject);
        }
        public void Close()
        {
            if (pannelAnimType == PannelAnimType.Fader)
            {
                _fader.onFadeOut += () => Destroy(gameObject);
                _fader.faderState = Fader.FaderState.FadingOut;
            }
            else
            {
                if (_animation.isPlaying)
                    _animation.Stop();
                _animation.Play("close");
                _isClosing = true;
            }
        }

        public static Pannel Show(string name)
        {
            var perfab = PerfabsUtils.Create(name);
            var pannel = perfab.GetComponent<Pannel>();
            if (pannel == null)
            {
                Debug.LogError(name + " is not a pannel");
                Destroy(perfab);
            }

            return pannel;
        }
    }
}
