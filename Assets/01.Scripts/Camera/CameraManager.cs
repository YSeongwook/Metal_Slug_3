using Cinemachine;
using UnityEngine;

namespace _01.Scripts.Camera
{
    public class CameraManager : Singleton<CameraManager>
    {
        [Header("Zone 1")]
        public CinemachineVirtualCamera vcamZ1A;
        public CinemachineVirtualCamera vcamZ1B;
        public CinemachineVirtualCamera vcamZ1C;
        public CinemachineVirtualCamera vcamZ1D;
        public CinemachineVirtualCamera vcamZ1E;

        [Header("Zone 2")]
        public CinemachineVirtualCamera vcamZ2A;
        public CinemachineVirtualCamera vcamZ2B;
        public CinemachineVirtualCamera vcamZ2C;
        public CinemachineVirtualCamera vcamZ2D;
        public CinemachineVirtualCamera vcamZ2E;

        [Header("Zone 3")]
        public CinemachineVirtualCamera vcamZ3A;
        public CinemachineVirtualCamera vcamZ3B;

        private void Start()
        {
            DontDestroyOnLoad(gameObject);
        }

        #region Editor Switches

        private void SwitchZ1AtoZ1B()
        {
            SwitchCameras(vcamZ1A, vcamZ1B);
        }

        private void SwitchZ1BtoZ1C()
        {
            SwitchCameras(vcamZ1B, vcamZ1C);
        }

        private void SwitchZ1CtoZ1D()
        {
            SwitchCameras(vcamZ1C, vcamZ1D);
        }

        private void SwitchZ1DtoZ1E()
        {
            SwitchCameras(vcamZ1D, vcamZ1E);
        }

        private void SwitchZ1EtoZ2A()
        {
            SwitchCameras(vcamZ1E, vcamZ2A);
        }
        public void SwitchZ2AtoZ2B()
        {
            SwitchCameras(vcamZ2A, vcamZ2B);
        }

        private void SwitchZ2BtoZ2C()
        {
            SwitchCameras(vcamZ2B, vcamZ2C);
        }
        public void SwitchZ2CtoZ2D()
        {
            SwitchCameras(vcamZ2C, vcamZ2D);
        }

        private void SwitchZ2DtoZ2E()
        {
            SwitchCameras(vcamZ2D, vcamZ2E);
        }
        public void SwitchZ2CtoZ3A()
        {
            SwitchCameras(vcamZ2C, vcamZ3A);
        }
        public void SwitchZ2EtoZ3A()
        {
            SwitchCameras(vcamZ2E, vcamZ3A);
        }
        public void SwitchZ3AtoZ3B()
        {
            SwitchCameras(vcamZ3A, vcamZ3B);
        }
        public void SwitchZ2AtoZ3A()
        {
            SwitchCameras(vcamZ2A, vcamZ3A);
        }
        #endregion

        private void SwitchCameras(CinemachineVirtualCamera cam1, CinemachineVirtualCamera cam2)
        {
            if (cam1 != null) cam1.gameObject.SetActive(false);
            if (cam2 != null) cam2.gameObject.SetActive(true);
        }

        private void SwitchToZ3ACamera()
        {
            // 현재 활성화된 카메라 찾기
            CinemachineVirtualCamera currentActiveCamera = FindActiveCamera();

            // 현재 활성화된 카메라를 비활성화하고, Z3A 카메라를 활성화
            if (currentActiveCamera != null) currentActiveCamera.gameObject.SetActive(false);

            vcamZ3A.gameObject.SetActive(true);
        }

        private CinemachineVirtualCamera FindActiveCamera()
        {
            // Zone 1 카메라 검사
            if (vcamZ1A.gameObject.activeSelf) return vcamZ1A;
            if (vcamZ1B.gameObject.activeSelf) return vcamZ1B;
            if (vcamZ1C.gameObject.activeSelf) return vcamZ1C;
            if (vcamZ1D.gameObject.activeSelf) return vcamZ1D;
            if (vcamZ1E.gameObject.activeSelf) return vcamZ1E;

            // Zone 2 카메라 검사
            if (vcamZ2A.gameObject.activeSelf) return vcamZ2A;
            if (vcamZ2B.gameObject.activeSelf) return vcamZ2B;
            if (vcamZ2C.gameObject.activeSelf) return vcamZ2C;
            // if (vcamZ2D.gameObject.activeSelf) return vcamZ2D;
            // if (vcamZ2E.gameObject.activeSelf) return vcamZ2E;

            // Zone 3 카메라 검사
            if (vcamZ3A.gameObject.activeSelf) return vcamZ3A;

            // 활성화된 카메라가 없으면 null 반환
            return null;
        }

        #region Mission 1 Switches
        public void AfterCrabTower()
        {
            Instance.SwitchZ1AtoZ1B();
        }

        public void AfterSunkBoat()
        {
            Instance.SwitchZ1BtoZ1C();
        }

        public void AfterSunkSignPost()
        {
            Instance.SwitchZ1CtoZ1D();
        }

        public void AfterLocusts()
        {
            Instance.SwitchZ1DtoZ1E();
        }

        public void AfterBoatSignPost()
        {
            Instance.SwitchZ1EtoZ2A();
        }

        public void AfterFirstVan()
        {
            Instance.SwitchZ2AtoZ2B();
        }

        public void AfterSecondVan()
        {
            Instance.SwitchZ2BtoZ2C();
        }

        public void AfterBossSpawn()
        {
            // Instance.SwitchZ2CtoZ3A();
            Instance.SwitchToZ3ACamera();
        }

        #endregion

        #region Mission 2 Switches
        public void AfterFirstHeli()
        {
            Instance.SwitchZ1EtoZ2A();
        }
        public void AfterSecondHeli()
        {
            Instance.SwitchZ2DtoZ2E();
        }
        public void AfterWarp()
        {
            Instance.SwitchZ2AtoZ3A();
        }
        #endregion
    }
}
