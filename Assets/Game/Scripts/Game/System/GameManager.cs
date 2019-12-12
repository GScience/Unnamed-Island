using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Game.Controller;
using Game.World;
using UnityEngine;

namespace Game.System
{
    [RequireComponent(typeof(DataManager), typeof(WorldManager), typeof(BlockTextureManager))]
    class GameManager : MonoBehaviour
    {
        public static DataManager DataManager { get; private set; }
        public static WorldManager WorldManager { get; private set; }
        public static BlockTextureManager BlockTextureManager { get; private set; }

        public static PlayerController PlayerController { get; private set; }

        public PlayerController playerController;

        void Awake()
        {
            PlayerController = playerController;
            DataManager = GetComponent<DataManager>();
            WorldManager = GetComponent<WorldManager>();
            BlockTextureManager = GetComponent<BlockTextureManager>();
        }
    }
}