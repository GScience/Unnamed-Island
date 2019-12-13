using Island.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Island.Utils
{
    /// <summary>
    /// 与场景转换相关
    /// </summary>
    public static class SceneUtils
    {
        public static void SwitchScene(string sceneName)
        {
            var globalFader = Fader.CreateGlobalFader();

            UnityEngine.Object.DontDestroyOnLoad(globalFader);

            globalFader.faderState = Fader.FaderState.FadingIn;

            globalFader.onFadeIn += () =>
            {
                SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
                globalFader.faderState = Fader.FaderState.FadingOut;
            };

            globalFader.onFadeOut += () =>
            {
                UnityEngine.Object.Destroy(globalFader.gameObject);
            };
        }
    }
}
