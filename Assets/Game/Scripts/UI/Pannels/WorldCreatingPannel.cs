using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Island.UI.Pannels
{
    [RequireComponent(typeof(Pannel))]
    public class WorldCreatingPannel : MonoBehaviour
    {
        private Pannel _pannel;

        public Text stageText;

        private void Awake()
        {
            _pannel = GetComponent<Pannel>();
        }

        private void Start()
        {
            _pannel.canvas.sortingOrder = (int)UILayer.WorldCreatingPannel;
        }

        public void SetStage(string stage)
        {
            stageText.text = stage;
        }
    }
}
