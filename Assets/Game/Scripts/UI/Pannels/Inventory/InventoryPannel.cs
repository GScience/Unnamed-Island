using Island.Game.EntityBehaviour;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Island.UI.Pannels.Inventory
{
    [RequireComponent(typeof(Pannel))]
    public abstract class InventoryPannel : MonoBehaviour
    {
        public static InventoryPannel CurrendClosableInventoryPannel { get; private set; }

        protected Pannel _pannel;

        public InventoryBehaviour inventory;
        public bool closable = true;

        void Awake()
        {
            _pannel = GetComponent<Pannel>();
        }

        private void Start()
        {
            if (CurrendClosableInventoryPannel != null)
                UnityEngine.Debug.LogError("An none closable pannel has already opened");

            if (closable)
                CurrendClosableInventoryPannel = this;

            Init();
        }

        private void OnDisable()
        {
            CurrendClosableInventoryPannel = null;
        }

        protected virtual void Update()
        {
            if (closable && Input.GetKey(KeyCode.Escape))
                _pannel.Close();
        }

        protected abstract void Init();
    }
}
