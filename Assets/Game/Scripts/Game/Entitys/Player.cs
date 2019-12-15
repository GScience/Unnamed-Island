
using Island.Game.Entitys;
using Spine.Unity;
using UnityEngine;

namespace Island.Game.Entitys
{
    /// <summary>
    /// Íæ¼Ò¿ØÖÆÆ÷
    /// </summary>
    class Player : Entity
    {
        public SkeletonAnimation skeletonAnim;
        private CharacterController _controller;

        private float _gravity = 9.8f;

        public float gracityScale = 1;
        public float speed = 1;

#if UNITY_EDITOR
        public bool autoWalk;
#endif
        private float _gravitySpeed;

        void Awake()
        {
            _controller = GetComponent<CharacterController>();
        }

        void OnEnable()
        {
            _controller.enabled = true;
        }

        void OnDisable()
        {
            _controller.enabled = false;
        }

        protected override void UpdateMovement()
        {
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
    }
}