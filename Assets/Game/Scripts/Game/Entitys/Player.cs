
using Island.Game.Entitys;
using Spine.Unity;
using System;
using UnityEngine;

namespace Island.Game.Entitys
{
    /// <summary>
    /// 玩家实体
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    class Player : Entity
    {
        public SkeletonAnimation skeletonAnim;
        private CharacterController _controller;

        private float _gravity = 9.8f;

        public float gracityScale = 1;
        public float speed = 1;

        public float interactSize = 1.0f;

#if UNITY_EDITOR
        public bool autoWalk;
#endif
        private Entity _selectedEntity;

        private float _gravitySpeed;

        void Awake()
        {
            _controller = GetComponent<CharacterController>();
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

        protected override void LoadFromEntityData()
        {
            base.LoadFromEntityData();
            HasUpdation = true;
        }
        protected override void UpdateEntityState()
        {
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

            if (Input.GetKey(KeyCode.A))
            {
                if (skeletonAnim.transform.localScale.x > 0)
                    skeletonAnim.transform.localScale = new Vector3(
                        skeletonAnim.transform.localScale.x * -1,
                        skeletonAnim.transform.localScale.y,
                        skeletonAnim.transform.localScale.z);

                _controller.Move((Vector3) (rotateMatrix * Vector3.left) * Time.deltaTime * speed);
                skeletonAnim.AnimationName = "Move";
            }

            if (Input.GetKey(KeyCode.D))
            {
                if (skeletonAnim.transform.localScale.x < 0)
                    skeletonAnim.transform.localScale = new Vector3(
                        skeletonAnim.transform.localScale.x * -1,
                        skeletonAnim.transform.localScale.y,
                        skeletonAnim.transform.localScale.z);

                _controller.Move((Vector3) (rotateMatrix * Vector3.right) * Time.deltaTime * speed);
                skeletonAnim.AnimationName = "Move";
            }

            if (Input.GetKey(KeyCode.W)
#if UNITY_EDITOR
                || autoWalk
#endif
                )
            {
                _controller.Move((Vector3) (rotateMatrix * Vector3.forward) * Time.deltaTime * speed);
                skeletonAnim.AnimationName = "Move";
            }

            if (Input.GetKey(KeyCode.S))
            {
                _controller.Move((Vector3) (rotateMatrix * Vector3.back) * Time.deltaTime * speed);
                skeletonAnim.AnimationName = "Move";
            }

            if (!Input.GetKey(KeyCode.A) &&
                !Input.GetKey(KeyCode.D) &&
                !Input.GetKey(KeyCode.W) &&
                !Input.GetKey(KeyCode.S)
#if UNITY_EDITOR
                && !autoWalk
#endif
                )
                skeletonAnim.AnimationName = "Relax";
        }

        private void UpdateSelectedEntity()
        {
            var overlapResult = Physics.OverlapSphere(
                transform.position,
                interactSize, 1 << Layer);

            // 没有选择任何物体
            if (overlapResult.Length <= 1)
            {
                if (_selectedEntity != null)
                {
                    _selectedEntity.IsSelected = false;
                    _selectedEntity = null;
                }
            }
            else
            {
                // 选择了物体
                GameObject newSelectedObj = null;
                Array.Sort(
                    overlapResult, 
                    (Collider collider1, Collider collider2) =>
                    {
                        var distance1 = Vector3.Distance(collider1.transform.position, transform.position);
                        var distance2 = Vector3.Distance(collider2.transform.position, transform.position);

                        if (distance1 > distance2)
                            return 1;
                        else if (distance1 < distance2)
                            return -1;
                        else
                            return 0;
                    });

                foreach (var obj in overlapResult)
                    if (obj.gameObject != gameObject)
                    {
                        newSelectedObj = obj.gameObject;
                        break;
                    }
                var newSelectedEntity = newSelectedObj?.GetComponent<Entity>();

                if (newSelectedEntity == null)
                    return;

                if (_selectedEntity != newSelectedObj)
                {
                    if (_selectedEntity != null)
                        _selectedEntity.IsSelected = false;

                    _selectedEntity = newSelectedEntity;

                    if (_selectedEntity != null)
                        _selectedEntity.IsSelected = true;
                }
            } 
        }
    }
}