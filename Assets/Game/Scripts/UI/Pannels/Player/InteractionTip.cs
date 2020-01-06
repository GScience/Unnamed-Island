using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Island.UI.Pannels.PlayerTip
{
    public class InteractionTip : MonoBehaviour
    {
        public Text tipText;
        public Text keyText;

        public string Tip
        {
            set
            {
                tipText.text = value;
            }
        }
        public KeyCode Key
        {
            set
            {
                keyText.text = value.ToString();
            }
        }
    }
}
