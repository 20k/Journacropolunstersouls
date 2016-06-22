using System;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

namespace JamesCamera.TestOverheadView
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(CapsuleCollider))]
    public class TestOverheadView : MonoBehaviour
    {
        [Serializable]
        public class MovementSettings
        {
            public float ForwardSpeed = 8.0f;   // Speed when walking forward
            public float RunMultiplier = 2.0f;   // Speed when sprinting
            public KeyCode RunKey = KeyCode.LeftShift;
            public float JumpForce = 30f;

            public AnimationCurve SlopeCurveModifier = new AnimationCurve(new Keyframe(-90.0f, 1.0f), new Keyframe(0.0f, 1.0f), new Keyframe(90.0f, 0.0f));
            [HideInInspector]
            public float CurrentTargetSpeed = 1f;

#if !MOBILE_INPUT
            private bool m_Running;
#endif

            public void UpdateDesiredTargetSpeed(Vector2 input)
            {
                if (input == Vector2.zero) return;

                CurrentTargetSpeed = ForwardSpeed;

#if !MOBILE_INPUT
                if (Input.GetKey(RunKey))
                {
                    CurrentTargetSpeed *= RunMultiplier;
                    m_Running = true;
                }
                else
                {
                    m_Running = false;
                }
#endif
            }

#if !MOBILE_INPUT
            public bool Running
            {
                get { return m_Running; }
            }
#endif
        }


        [Serializable]
        public class AdvancedSettings
        {
            public float groundCheckDistance = 0.01f; // distance for checking if the controller is grounded ( 0.01f seems to work best for this )
            public float stickToGroundHelperDistance = 0.5f; // stops the character
            public float slowDownRate = 20f; // rate at which the controller comes to a stop when there is no input
            public bool airControl; // can the user control the direction that is being moved in the air
            [Tooltip("set it to 0.1 or more if you get stuck in wall")]
            public float shellOffset; //reduce the radius by that ratio to avoid getting stuck in wall (a value of 0.1f is nice)
        }


        public Camera cam;
        public MainCharacterProceduralLegController legController;
        public MovementSettings movementSettings = new MovementSettings();
        public CameraRotater cameraOrbit = new CameraRotater();
        public AdvancedSettings advancedSettings = new AdvancedSettings();


        public AnimationCurve DodgeCurve;
        public float DodgeDistance = 1;
        public float DodgeTimeSeconds = 1;
        public float DodgeTimeRecovery = 1.5f;
        private float DodgeTime = 0; ///not a frac
        private bool IsDodging = false;
        private Vector3 DodgeStart = new Vector3(0, 0, 0);
        private Vector3 DodgeEnd = new Vector3(0, 0, 0);

        private Rigidbody m_RigidBody;
        private CapsuleCollider m_Capsule;
        private float m_YRotation;
        private Vector3 m_GroundContactNormal;
        private bool m_Jump, m_PreviouslyGrounded, m_Jumping, m_IsGrounded;
        private Vector2 input;


        public Vector3 Velocity
        {
            get { return m_RigidBody.velocity; }
        }

        public bool Grounded
        {
            get { return m_IsGrounded; }
        }

        public bool Jumping
        {
            get { return m_Jumping; }
        }

        public bool Running
        {
            get
            {
#if !MOBILE_INPUT
                return movementSettings.Running;
#else
	            return false;
#endif
            }
        }


        private void Start()
        {
            m_RigidBody = GetComponent<Rigidbody>();
            m_Capsule = GetComponent<CapsuleCollider>();
            cameraOrbit.Init(transform, cam.transform);
        }

        void Dodge(Vector2 input)
        {
            if (IsDodging)
                return;

            //Vector3 in3 = new Vector3(input.x, 0, input.y);

            Vector3 desiredMove = cam.transform.forward * input.y + cam.transform.right * input.x;
            desiredMove = Vector3.ProjectOnPlane(desiredMove, m_GroundContactNormal).normalized;

            IsDodging = true;
            DodgeTime = 0;
            DodgeStart = transform.position;
            DodgeEnd = DodgeStart + desiredMove.normalized * DodgeDistance;
        }

        void DodgeTick(float ftime)
        {
            if (!IsDodging)
                return;

            //Vector3 interp = DodgeStart * (1.f - DodgeFrac) + DodgeEnd * DodgeFrac;

            //0 = beginning, 1 = end
            float sampleCurve = 1f - DodgeCurve.Evaluate(DodgeTime/DodgeTimeSeconds);

            if(DodgeTime < DodgeTimeSeconds)
                transform.position = DodgeStart * sampleCurve + DodgeEnd * (1f - sampleCurve);

            DodgeTime += ftime;

            if (DodgeTime >= DodgeTimeSeconds + DodgeTimeRecovery)
                IsDodging = false;
        }

        private void Update()
        {
            if (CrossPlatformInputManager.GetButtonDown("Jump") && !m_Jump)
            {
                m_Jump = true;
            }

            GroundCheck();
            //Vector2 input = GetInput();
            input = GetInput() * Time.deltaTime / Time.fixedDeltaTime;

            float runval = movementSettings.Running ? 1f : 0f;

            legController.SetMoveDir(input, runval * movementSettings.RunMultiplier, movementSettings.Running);

            if ((Mathf.Abs(input.x) > float.Epsilon || Mathf.Abs(input.y) > float.Epsilon) && (advancedSettings.airControl || m_IsGrounded))
            {
                // always move along the camera forward as it is the direction that it being aimed at
                Vector3 desiredMove = cam.transform.forward * input.y + cam.transform.right * input.x;
                desiredMove = Vector3.ProjectOnPlane(desiredMove, m_GroundContactNormal).normalized;

                transform.position = transform.position + desiredMove.normalized * movementSettings.CurrentTargetSpeed * Time.deltaTime / Time.fixedDeltaTime;
                m_RigidBody.position = transform.position;
            }

            if (m_IsGrounded)
            {
                m_RigidBody.drag = 5f;

                if (m_Jump)
                {
                    m_RigidBody.drag = 0f;
                    m_RigidBody.velocity = new Vector3(m_RigidBody.velocity.x, 0f, m_RigidBody.velocity.z);
                    m_RigidBody.AddForce(new Vector3(0f, movementSettings.JumpForce, 0f), ForceMode.Impulse);
                    m_Jumping = true;
                }

                if (!m_Jumping && Mathf.Abs(input.x) < float.Epsilon && Mathf.Abs(input.y) < float.Epsilon && m_RigidBody.velocity.magnitude < 1f)
                {
                    m_RigidBody.Sleep();
                }
            }
            else
            {
                m_RigidBody.drag = 0f;
                if (m_PreviouslyGrounded && !m_Jumping)
                {
                    StickToGroundHelper();
                }
            }
            m_Jump = false;
            UpdateCharacter();

            if(Input.GetKeyDown(KeyCode.LeftControl))
            {
                Dodge(input);
            }

            DodgeTick(Time.deltaTime);
        }

        void LateUpdate()
        { 
            RotateView();
        }

        private float SlopeMultiplier()
        {
            float angle = Vector3.Angle(m_GroundContactNormal, Vector3.up);
            return movementSettings.SlopeCurveModifier.Evaluate(angle);
        }


        private void StickToGroundHelper()
        {
            RaycastHit hitInfo;
            if (Physics.SphereCast(transform.position, m_Capsule.radius * (1.0f - advancedSettings.shellOffset), Vector3.down, out hitInfo,
                                   ((m_Capsule.height / 2f) - m_Capsule.radius) +
                                   advancedSettings.stickToGroundHelperDistance, ~0, QueryTriggerInteraction.Ignore))
            {
                if (Mathf.Abs(Vector3.Angle(hitInfo.normal, Vector3.up)) < 85f)
                {
                    m_RigidBody.velocity = Vector3.ProjectOnPlane(m_RigidBody.velocity, hitInfo.normal);
                }
            }
        }


        public Vector2 GetInput()
        {
            Vector2 input = new Vector2
            {
                x = CrossPlatformInputManager.GetAxisRaw("Horizontal"),
                y = CrossPlatformInputManager.GetAxisRaw("Vertical")
            };
            movementSettings.UpdateDesiredTargetSpeed(input);
            return input;
        }


        private void RotateView()
        {
            //avoids the mouse looking if the game is effectively paused
            if (Mathf.Abs(Time.timeScale) < float.Epsilon) return;

            cameraOrbit.UpdateCameraLook(transform, cam.transform, input);
            cam.transform.LookAt(m_RigidBody.transform);
        }

        private void UpdateCharacter()
        {
            cameraOrbit.UpdateCharacterRot(transform, input);
        }

        /// sphere cast down just beyond the bottom of the capsule to see if the capsule is colliding round the bottom
        private void GroundCheck()
        {
            m_PreviouslyGrounded = m_IsGrounded;
            RaycastHit hitInfo;
            if (Physics.SphereCast(transform.position, m_Capsule.radius * (1.0f - advancedSettings.shellOffset), Vector3.down, out hitInfo,
                                   ((m_Capsule.height / 2f) - m_Capsule.radius) + advancedSettings.groundCheckDistance, ~0, QueryTriggerInteraction.Ignore))
            {
                m_IsGrounded = true;
                m_GroundContactNormal = hitInfo.normal;
            }
            else
            {
                m_IsGrounded = false;
                m_GroundContactNormal = Vector3.up;
            }
            if (!m_PreviouslyGrounded && m_IsGrounded && m_Jumping)
            {
                m_Jumping = false;
            }
        }
    }
}
