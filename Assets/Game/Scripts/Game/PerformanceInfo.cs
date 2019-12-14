using Island.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.UI;

namespace Island
{
    /// <summary>
    /// 显示性能信息
    /// </summary>
    [RequireComponent(typeof(Text))]
    [ExecuteAlways]
    class PerformanceInfo : MonoBehaviour
    {
        private Text _text;
        private float _time;
        private int _totalRenderFrameCount;
        private int _updateFrameCount;
        private int _probablyProblem = 0;

        private float _fps;
        private float _ups;

        private bool _hasPerformanceProblem;

        public float maxFpsChangePercent = 0.2f;
        public float maxUpsChangePercent = 0.4f;

        private float _deltaTime = 0;

        void Awake()
        {
            _text = GetComponent<Text>();
            _text.color = Color.white;
        }

        private void Start()
        {
            _text.canvas.sortingOrder = (int) UILayer.DebugInfo;
        }
        void Update()
        {
            UpdateFps();

            _deltaTime += Time.deltaTime;

            if (_deltaTime < 1)
                return;

            _deltaTime = 0;

            _text.text =
                $"FPS = {(int)_fps}\n" +
                $"Update per second = {(int)_ups}\n" +
                $"Memory = {Profiler.GetTotalAllocatedMemoryLong() / 1024 / 1024}MB/{Profiler.GetTotalReservedMemoryLong() / 1024 / 1024}MB\n" +
                $"Free Memory = {Profiler.GetTotalUnusedReservedMemoryLong() / 1024 / 1024}MB\n" +
                $"Probably performance problem found = {_probablyProblem}\n";

            if (_hasPerformanceProblem)
            {
                _text.color = Color.yellow;
                _text.text += "May has performance problem!";
            }
        }

        void UpdateFps()
        {
            _time += Time.unscaledDeltaTime;
            ++_updateFrameCount;

            var currentRenderFrameCount = Time.renderedFrameCount;

            if (_time >= 0.1f && currentRenderFrameCount - _totalRenderFrameCount >= 1 && _updateFrameCount > 1)
            {
                var fps = (currentRenderFrameCount - _totalRenderFrameCount) / _time;
                var ups = _updateFrameCount / _time;

                _time = 0;
                _updateFrameCount = 0;
                _totalRenderFrameCount = currentRenderFrameCount;

                if ((_fps - fps) / _fps > maxFpsChangePercent || (_ups - ups) / _ups > maxUpsChangePercent)
                {
                    _hasPerformanceProblem = true;
                    ++_probablyProblem;
                }

                _ups = ups;
                _fps = fps;
            }
        }
    }
}
