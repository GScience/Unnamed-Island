﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Island.Game.Render;
using Island.Game.System;
using Island.Game.World;
using UnityEngine;

namespace Island.Game.EntityBehaviour
{
    [RequireComponent(typeof(InventoryBehaviour), typeof(Entity))]
    public class ChestBehaviour : EntityBehaviour
    {
        private SpriteRenderer _spriteRenderer;
        private InventoryBehaviour _inventoryBehaviour;

        protected override void EntityLoad(DataTag dataTag)
        {
            
        }

        protected override void EntitySave(DataTag dataTag)
        {
            
        }

        protected override void Init()
        {
            var spriteObject = new GameObject("sprite");
            spriteObject.transform.parent = transform;
            _spriteRenderer = spriteObject.AddComponent<SpriteRenderer>();
            _spriteRenderer.sprite = GameManager.EntitySpriteDatabase.Get("chest");
            spriteObject.AddComponent<Billboard>();

            _inventoryBehaviour = GetComponent<InventoryBehaviour>();
            _inventoryBehaviour.InventorySize = 20;
            _inventoryBehaviour.inventoryPannel = "ChestPannel";

            Entity.SetCollider(Vector3.up * 0.5f, 0.6f);
            Entity.IsSelectable = true;
        }

        protected override void OnSelected(PlayerBehaviour player)
        {
            player.BindInteraction(KeyCode.O, "打开",
            new Action(() =>
            {
                SendMessage("OnPlayerOpen", player);
            }));
        }

        void OnPlayerOpen(PlayerBehaviour player)
        {
            _inventoryBehaviour.ShowUI();
        }
    }
}
