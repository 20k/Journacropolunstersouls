using System;
using UnityEngine;
using System.Collections;

namespace JamesCamera.TestOverheadView
{
    public class MainCharacterAnimation : MonoBehaviour
    {

        public Transform leftLeg;
        public Transform rightLeg;
        public Transform reference;
        public Transform basePosition;
        public Transform headForBob;
        public float walkCycleMs = 1000f;
        public float walkSweepAngle = (float)Math.PI / 8f;
        public TestOverheadView overheadView;
        /// <summary>
        /// for walking
        /// </summary>
        public float headBobDist = 0.1f;
        public float headBobIdleDist = 0.02f;
        public float headBobIdleTimeSeconds = 2.5f;

        private float yRot = 0;
        private float walkFrac = 0;
        private float interLegDistance = 0;
        private float heightOffset = 0;
        private Vector3 headOffset;
        private float headBobFrac = 0;

        // Use this for initialization
        void Start()
        {
            interLegDistance = (leftLeg.position - rightLeg.position).magnitude / 2f;
            heightOffset = basePosition.position.y - leftLeg.position.y;
            headOffset = headForBob.position - basePosition.position;
        }

        void doHeadBob()
        {
            float sinWalkFrac = Mathf.Sin(walkFrac * 2f * Mathf.PI)/2f;

            headBobFrac += Time.deltaTime / headBobIdleTimeSeconds;

            float extraBob = headBobIdleDist * Mathf.Sin(headBobFrac * 2f * Mathf.PI) / 2;

            headForBob.position = basePosition.position + new Vector3(0, headOffset.y + headBobDist * sinWalkFrac + extraBob, 0);
        }

        // Update is called once per frame
        void Update()
        {
            Vector2 input = overheadView.GetInput();

            Vector3 Euler = reference.rotation.eulerAngles;

            yRot = Euler.y;

            float lwalk = (float)Math.Sin(walkFrac * 2f * (float)Math.PI);

            float lrot = walkSweepAngle * lwalk;
            float rrot = walkSweepAngle * -lwalk;

            lrot += Mathf.PI / 2f;
            rrot += Mathf.PI / 2f;

            Quaternion lquat = Quaternion.Euler(lrot * Mathf.Rad2Deg, 0, 0);
            Quaternion rquat = Quaternion.Euler(rrot * Mathf.Rad2Deg, 0, 0);

            Quaternion globalYrot = Quaternion.Euler(0, yRot, 0);

            lquat = globalYrot * lquat;
            rquat = globalYrot * rquat;

            leftLeg.rotation = lquat;
            rightLeg.rotation = rquat;

            Vector2 pos2d = new Vector2(interLegDistance, 0);

            float l = pos2d.magnitude;
            float r = Vector2.Angle(new Vector2(1, 0), pos2d);

            r += -yRot * Mathf.Deg2Rad;

            pos2d.x = l * Mathf.Cos(r);
            pos2d.y = l * Mathf.Sin(r);

            Vector3 lpos;
            lpos.x = pos2d.x;
            lpos.y = -heightOffset;
            lpos.z = pos2d.y;
            leftLeg.position = lpos + basePosition.position;

            Vector3 rpos;
            rpos.x = -pos2d.x;
            rpos.y = -heightOffset;
            rpos.z = -pos2d.y;
            rightLeg.position = rpos + basePosition.position;

            if(input.magnitude > Mathf.Epsilon)
                walkFrac += Time.deltaTime / (walkCycleMs / 1000f);

            doHeadBob();

            walkFrac %= 1f;
        }
    }

}