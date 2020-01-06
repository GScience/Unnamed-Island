using Island.UI.Pannels.PlayerTip;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Island.UI.Pannels.Player
{
    [RequireComponent(typeof(Pannel))]
    class PlayerInteractionPannel : MonoBehaviour
    {
        public GameObject interactionTipPerfabs = null;
        public Transform interactionTipParent = null;

        public void BindInteraction(KeyCode key, string tip)
        {
            var tipObj = Instantiate(interactionTipPerfabs);
            var interactionTip = tipObj.GetComponent<InteractionTip>();
            interactionTip.Tip = tip;
            interactionTip.Key = key;

            tipObj.transform.SetParent(interactionTipParent);
        }

        public void ClearInteraction()
        {
            foreach (var child in interactionTipParent.GetComponentsInChildren<RectTransform>())
            {
                if (child.gameObject != interactionTipParent.gameObject)
                    Destroy(child.gameObject);
            }
        }
    }
}
