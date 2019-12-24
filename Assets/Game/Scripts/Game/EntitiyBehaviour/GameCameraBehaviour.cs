using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cinemachine;
using Island.Game.System;
using Island.Game.World;
using UnityEngine;
using UnityEngine.Rendering;

namespace Island.Game.EntityBehaviour
{
    /// <summary>
    /// 相机控制器
    /// </summary>
    [RequireComponent(typeof(CinemachineVirtualCamera), typeof(Entity))]
    class GameCameraBehaviour : EntityBehaviour
    {
        private CinemachineVirtualCamera _virtualCamera;
        private CinemachineFramingTransposer _cameraBody;

        public float rotateSpeed = 10.0f;
        public float smooth = 0.1f;

        private float _zRotateSpeed;
        private float _zRotateSpeedVelocity;
        private float _xRotateSpeed;
        private float _xRotateSpeedVelocity;

        public float minDistance = 10;
        public float maxDistance = 20;

        public float minXRot = 30;
        public float maxXRot = 60;

        protected override void Init()
        {
            
        }

        private void Awake()
        {
            _virtualCamera = GetComponent<CinemachineVirtualCamera>();
            _cameraBody = (CinemachineFramingTransposer)_virtualCamera.GetCinemachineComponent(CinemachineCore.Stage.Body);
        }

        void EntityLoad(DataTag dataTag)
        {
            entity.HasUpdation = false;
            var yRot = dataTag.TryGet("yRot", 45.0f);
            var xRot = dataTag.TryGet("xRot", minXRot);
            transform.rotation = Quaternion.Euler(xRot, yRot, transform.rotation.z);

            UpdateCameraDistance();
        }

        void EntitySave(DataTag dataTag)
        {
            dataTag.Set("yRot", transform.rotation.eulerAngles.y);
            dataTag.Set("xRot", transform.rotation.eulerAngles.x);
        }

        void EntityUpdate()
        {
        }

        void UpdateCameraDistance()
        {
            var xRot = transform.rotation.eulerAngles.x;
            _cameraBody.m_CameraDistance = (xRot - minXRot) / (maxXRot - minXRot) * (maxDistance - minDistance) + minDistance;
        }

        void LateUpdate()
        {
            if (Input.GetKey(KeyCode.Q))
            {
                _zRotateSpeed = Mathf.SmoothDamp(_zRotateSpeed, rotateSpeed, ref _zRotateSpeedVelocity, smooth);
            }
            else if (Input.GetKey(KeyCode.E))
            {
                _zRotateSpeed = Mathf.SmoothDamp(_zRotateSpeed, -rotateSpeed, ref _zRotateSpeedVelocity, smooth);
            }
            else
            {
                _zRotateSpeed = Mathf.SmoothDamp(_zRotateSpeed, 0, ref _zRotateSpeedVelocity, smooth);
            }

            if (Input.mouseScrollDelta.y > Mathf.Epsilon)
            {
                _xRotateSpeed = Mathf.SmoothDamp(_xRotateSpeed, -rotateSpeed * 10, ref _xRotateSpeedVelocity, smooth);
            }
            else if (Input.mouseScrollDelta.y < -Mathf.Epsilon)
            {
                _xRotateSpeed = Mathf.SmoothDamp(_xRotateSpeed, rotateSpeed * 10, ref _xRotateSpeedVelocity, smooth);
            }
            else
            {
                _xRotateSpeed = Mathf.SmoothDamp(_xRotateSpeed, 0, ref _xRotateSpeedVelocity, smooth);
            }

            transform.RotateAround(_virtualCamera.Follow.position, Vector3.up, _zRotateSpeed * Time.deltaTime);

            transform.hasChanged = false;
            transform.Rotate(Vector3.right, _xRotateSpeed * Time.deltaTime);

            var xRot = transform.rotation.eulerAngles.x;

            if (xRot > maxXRot)
            {
                transform.rotation *= Quaternion.AngleAxis(maxXRot - xRot, Vector3.right);
                xRot = maxXRot;
            }
            else if (xRot < minXRot)
            {
                transform.rotation *= Quaternion.AngleAxis(minXRot - xRot, Vector3.right);
                xRot = minXRot;
            }

            if (!transform.hasChanged)
                return;

            UpdateCameraDistance();
        }
    }
}
