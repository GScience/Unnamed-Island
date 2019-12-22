
using Island.Game.EntityBehaviour;
using Island.Game.World;
using Island.UI;
using Island.UI.Pannels;
using Spine.Unity;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Island.Game.EntityBehaviour
{
    /// <summary>
    /// 玩家实体
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    class Player : MonoBehaviour
    {
        public SkeletonAnimation skeletonAnim;
        private CharacterController _controller;
        private Entity _entity;

        private List<Entity> _selectableEntityList = new List<Entity>();
        private Collider[] _selectedEntities = new Collider[20];

        private float _gravity = 9.8f;

        public float gracityScale = 1;
        public float speed = 1;

        public float interactSize = 1.0f;

        public List<Tuple<KeyCode, Action>> _playerAction = new List<Tuple<KeyCode, Action>>();

#if UNITY_EDITOR
        public bool autoWalk;
#endif
        private Entity _selectedEntity;
        private float _gravitySpeed;

        private PlayerInteractionPannel _playerInteractionPannel;
        
        void Awake()
        {
            _controller = GetComponent<CharacterController>();
            _entity = GetComponent<Entity>();
        }

        void Start()
        {
            _playerInteractionPannel = Pannel.Show("PlayerInteractionPannel").GetComponent<PlayerInteractionPannel>();
            _entity.HasUpdation = true;
        }

        private void OnDrawGizmos()
        {
            Gizmos.DrawWireSphere(transform.position, interactSize);
        }
        void OnEnable()
        {
            _controller.enabled = true;
        }

        void OnDisable()
        {
            _controller.enabled = false;
        }

        void EntityLoad(DataTag dataTag)
        {
            _entity.HasUpdation = true;
        }

        void EntitySave(DataTag dataTag)
        {
        }

        void EntityUpdate()
        {
            UpdateAction();
            UpdateSelectedEntity();

            if (!_controller.isGrounded)
            {
                _gravitySpeed += _gravity * gracityScale * Time.deltaTime;
                _controller.Move(Vector3.down * _gravitySpeed * Time.deltaTime);
            }
            else
                _gravitySpeed = 0;

            var cameraRot = Camera.main.transform.rotation;
            var cameraTotY = cameraRot.eulerAngles.y;

            var rotateMatrix = Matrix4x4.Rotate(Quaternion.AngleAxis(cameraTotY, Vector3.up));
            var velocityDirection = Vector3.zero;

            if (Input.GetKey(KeyCode.A))
            {
                if (skeletonAnim.transform.localScale.x > 0)
                    skeletonAnim.transform.localScale = new Vector3(
                        skeletonAnim.transform.localScale.x * -1,
                        skeletonAnim.transform.localScale.y,
                        skeletonAnim.transform.localScale.z);

                velocityDirection += Vector3.left;
            }

            if (Input.GetKey(KeyCode.D))
            {
                if (skeletonAnim.transform.localScale.x < 0)
                    skeletonAnim.transform.localScale = new Vector3(
                        skeletonAnim.transform.localScale.x * -1,
                        skeletonAnim.transform.localScale.y,
                        skeletonAnim.transform.localScale.z);

                velocityDirection += Vector3.right;
            }

            if (Input.GetKey(KeyCode.W)
#if UNITY_EDITOR
                || autoWalk
#endif
                )
            {
                velocityDirection += Vector3.forward;
            }

            if (Input.GetKey(KeyCode.S))
            {
                velocityDirection += Vector3.back;
            }

            if (velocityDirection != Vector3.zero
#if UNITY_EDITOR
                && !autoWalk
#endif
                ) 
                skeletonAnim.AnimationName = "Move";
            else
                skeletonAnim.AnimationName = "Relax";

            _controller.Move((Vector3)(rotateMatrix * velocityDirection.normalized) * Time.deltaTime * speed);
        }

        public void BindInteraction(KeyCode key, string tip, Action action)
        {
            _playerInteractionPannel.BindInteraction(key, tip);
            _playerAction.Add(new Tuple<KeyCode, Action>(key, action));
        }

        private void ClearInteraction()
        {
            _playerAction.Clear();
            _playerInteractionPannel.ClearInteraction();
        }

        private void UpdateAction()
        {
            if (_selectedEntity == null)
                ClearInteraction();

            foreach (var playerAction in _playerAction)
                if (Input.GetKeyDown(playerAction.Item1))
                    playerAction.Item2?.Invoke();
        }

        private void UpdateSelectedEntity()
        {
            var overlayResultCount = Physics.OverlapSphereNonAlloc(
                transform.position,
                interactSize, 
                _selectedEntities, 
                1 << Entity.SelectableLayer);

            _selectableEntityList.Clear();

            for (int i = 0; i < overlayResultCount; ++i)
            {
                var collider = _selectedEntities[i];
                var entity = collider.GetComponent<Entity>();
                if (entity != null && entity.IsSelectable)
                    _selectableEntityList.Add(entity);
            }

            // 没有选择任何物体
            if (_selectableEntityList.Count <= 0)
            {
                if (_selectedEntity != null)
                {
                    _selectedEntity.SendMessage("OnUnselected", this);
                    _selectedEntity = null;
                }
            }
            else
            {
                // 选择了物体
                _selectableEntityList.Sort(
                     (Entity entity1, Entity entity2) =>
                    {
                        var distance1 = Vector3.Distance(entity1.transform.position, transform.position);
                        var distance2 = Vector3.Distance(entity2.transform.position, transform.position);

                        if (distance1 > distance2)
                            return 1;
                        else if (distance1 < distance2)
                            return -1;
                        else
                            return 0;
                    });

                var newSelectedEntity = _selectableEntityList[0];

                if (_selectedEntity != newSelectedEntity)
                {
                    if (_selectedEntity != null)
                    {
                        _selectedEntity.SendMessage("OnUnselected", this);
                        ClearInteraction();
                    }

                    _selectedEntity = newSelectedEntity;

                    if (_selectedEntity != null)
                        _selectedEntity.SendMessage("OnSelected", this);
                }
            }
        }
    }
}