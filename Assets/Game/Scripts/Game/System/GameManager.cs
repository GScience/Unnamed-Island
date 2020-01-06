using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Island.Game.EntityBehaviour;
using Island.Game.World;
using Island.UI;
using UnityEngine;

namespace Island.Game.System
{
    /// <summary>
    /// 游戏管理器
    /// 负责管理所有控制器
    /// </summary>
    [RequireComponent(typeof(ProxyManager), typeof(WorldManager), typeof(BlockTextureManager))]
    class GameManager : MonoBehaviour
    {
        public static ProxyManager ProxyManager { get; private set; }
        public static WorldManager WorldManager { get; private set; }
        public static BlockTextureManager BlockTextureManager { get; private set; }
        public static SpriteDatabase EnvElementSpriteDatabase { get; private set; }
        public static SpriteDatabase ItemSpriteDatabase { get; private set; }

        public static PlayerBehaviour Player { get; private set; }

        public static bool IsInitializing => WorldManager == null || WorldManager.IsInitializing;
        public PlayerBehaviour player = null;

        public SpriteDatabase envElementSpriteDatabase;
        public SpriteDatabase itemSpriteDatabase;

        private Pannel _gameLoadingPannel;
        private Pannel _debugPannel;

        void Awake()
        {
            Player = player;
            EnvElementSpriteDatabase = envElementSpriteDatabase;
            ItemSpriteDatabase = itemSpriteDatabase;

            ProxyManager = GetComponent<ProxyManager>();
            WorldManager = GetComponent<WorldManager>();
            BlockTextureManager = GetComponent<BlockTextureManager>();
        }

        private void Update()
        {
            if (_gameLoadingPannel == null && IsInitializing)
                _gameLoadingPannel = Pannel.Show("gameLoading");

            if (!IsInitializing && _gameLoadingPannel != null)
            {
                _gameLoadingPannel.Close();
                _gameLoadingPannel = null;
            }

            if (Input.GetKeyDown(KeyCode.F1))
            {
                if (_debugPannel == null)
                    _debugPannel = Pannel.Show("DebugPannel");
                else
                    _debugPannel.Close();
            }
        }
    }
}