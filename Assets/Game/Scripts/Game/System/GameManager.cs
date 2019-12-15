using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Island.Game.Entitys;
using Island.Game.World;
using Island.UI;
using UnityEngine;

namespace Island.Game.System
{
    /// <summary>
    /// 游戏管理器
    /// 负责管理所有控制器
    /// </summary>
    [RequireComponent(typeof(DataManager), typeof(WorldManager), typeof(BlockTextureManager))]
    class GameManager : MonoBehaviour
    {
        public static DataManager DataManager { get; private set; }
        public static WorldManager WorldManager { get; private set; }
        public static BlockTextureManager BlockTextureManager { get; private set; }

        public static Player PlayerController { get; private set; }

        public static bool IsInitializing => WorldManager == null || WorldManager.IsInitializing;
        public Player playerController = null;

        private Pannel _gameLoadingPannel;

        void Awake()
        {
            PlayerController = playerController;
            DataManager = GetComponent<DataManager>();
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
        }
    }
}