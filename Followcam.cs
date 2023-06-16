using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

namespace D86_CE
{
    public class Followcam : MonoBehaviour
    {
        #region Variables
        private Transform carTarget;

        public float followCameraZoom = 5;

        private float x = 0.0f;
        private float y = 0.0f;
        #endregion

        #region Methods
        private void Update()
        {
            if (carTarget && !GetComponent<Freecam>().playersListOpen)
            {
                x += Input.GetAxisRaw("Mouse X") * Main.freeCamSens.Value * Time.deltaTime;
                y += Input.GetAxisRaw("Mouse Y") * Main.freeCamSens.Value * Time.deltaTime;
                followCameraZoom -= Input.mouseScrollDelta.y;
                followCameraZoom = Mathf.Clamp(followCameraZoom, 5, 65);
                y = Mathf.Clamp(y, -90, 90);
            }
        }

        private void LateUpdate()
        {
            if (carTarget)
            {
                Vector3 dir = new Vector3(0, 0, followCameraZoom);
                Quaternion rotation = Quaternion.Euler(y, x, 0);
                transform.position = carTarget.position + rotation * dir;
                transform.LookAt(carTarget.position);
            }
        }

        public void SetTargetCar(Transform target)
        {
            carTarget = target;
        }
        #endregion
    }
}
