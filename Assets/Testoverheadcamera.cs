using System;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

class JMaths
{
    static public float AngleDiff(float a1, float a2)
    {
        a1 %= 2 * Mathf.PI;
        a2 %= 2 * Mathf.PI;

        float diff = a1 - a2;

        float d1 = (a1 + 2f * (float)Math.PI) - a2;
        float d2 = (a1 - 2f * (float)Math.PI) - a2;

        if (Math.Abs(d1) < Math.Abs(diff))
            diff = d1;
        if (Math.Abs(d2) < Math.Abs(diff))
            diff = d2;

        return diff;
    }
}

namespace JamesCamera.TestOverheadView
{
    [Serializable]
    public class CameraRotater
    {
        public float XSensitivity = 2f;
        public float YSensitivity = 2f;
        public float zoomPerTick = 4f;
        public bool clampVerticalRotation = true;
        public float MinimumX = -90F;
        public float MaximumX = 90F;
        public bool smooth;
        public float smoothTime = 5f;
        public bool lockCursor = true;
        public float character90TurnTimeSeconds = 0.1f;

        private bool m_cursorIsLocked = true;

        [HideInInspector]
        public float cameraDist = 1;
        private float xAcc = -45;
        private float yAcc = 0;
        private float zoomAcc = 0;
        private float currentTurnAngleAcc = 0f;

        public void Init(Transform character, Transform camera)
        {
            Vector3 v1 = character.position;
            Vector3 v2 = camera.position;

            cameraDist = (v1 - v2).magnitude;
        }

        public void UpdateCameraLook(Transform character, Transform camera, Vector2 input)
        {
            Quaternion cQuat = Quaternion.Euler(xAcc, yAcc, 0f);

            Vector3 front = cQuat * Vector3.forward;

            Vector3 glob = character.position + front * (cameraDist + zoomAcc * -zoomPerTick);

            camera.position = glob;
        }

        public void UpdateCharacterRot(Transform character, Vector2 input)
        {
            ///YEAAAAH C#
            currentTurnAngleAcc %= 2f * (float)Math.PI;

            yAcc += CrossPlatformInputManager.GetAxisRaw("Mouse X") * XSensitivity;
            xAcc += CrossPlatformInputManager.GetAxisRaw("Mouse Y") * YSensitivity;

            zoomAcc += CrossPlatformInputManager.GetAxisRaw("Mouse ScrollWheel");

            Quaternion yQuat = Quaternion.Euler(0, yAcc, 0);


            float desiredAngle = (float)Math.Atan2(input.y, -input.x) - (float)Math.PI/2f;

            if (Math.Abs(input.y) < Mathf.Epsilon && Math.Abs(input.x) < Mathf.Epsilon)
            {
                desiredAngle = currentTurnAngleAcc;
            }

            float currentAngle = currentTurnAngleAcc;

            float diff = desiredAngle - currentAngle;

            float d1 = (desiredAngle + 2f * (float)Math.PI) - currentAngle;
            float d2 = (desiredAngle - 2f * (float)Math.PI) - currentAngle;

            if (Math.Abs(d1) < Math.Abs(diff))
                diff = d1;
            if (Math.Abs(d2) < Math.Abs(diff))
                diff = d2;

            float turnDir = 0;

            if (Math.Abs(diff) > Mathf.Epsilon)
                turnDir = diff / (float)Math.Abs(diff);

            if(Math.Abs(turnDir) > Math.Abs(diff))
                turnDir = turnDir * Math.Abs(diff);

            currentTurnAngleAcc += turnDir * Time.deltaTime * 0.5f * (float)Math.PI / character90TurnTimeSeconds;

            Quaternion inputQuat = Quaternion.Euler(0, currentTurnAngleAcc * Mathf.Rad2Deg, 0);

            character.localRotation = yQuat * inputQuat;

            UpdateCursorLock();
        }
        
        public void SetCursorLock(bool value)
        {
            lockCursor = value;
            if(!lockCursor)
            {//we force unlock the cursor if the user disable the cursor locking helper
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }

        public void UpdateCursorLock()
        {
            //if the user set "lockCursor" we check & properly lock the cursos
            if (lockCursor)
                InternalLockUpdate();
        }

        private void InternalLockUpdate()
        {
            if(Input.GetKeyUp(KeyCode.Escape))
            {
                m_cursorIsLocked = false;
            }
            else if(Input.GetMouseButtonUp(0))
            {
                m_cursorIsLocked = true;
            }

            if (m_cursorIsLocked)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            else if (!m_cursorIsLocked)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }

        Quaternion ClampRotationAroundXAxis(Quaternion q)
        {
            q.x /= q.w;
            q.y /= q.w;
            q.z /= q.w;
            q.w = 1.0f;

            float angleX = 2.0f * Mathf.Rad2Deg * Mathf.Atan (q.x);

            angleX = Mathf.Clamp (angleX, MinimumX, MaximumX);

            q.x = Mathf.Tan (0.5f * Mathf.Deg2Rad * angleX);

            return q;
        }

    }
}
