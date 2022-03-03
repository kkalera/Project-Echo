using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Echo {
    [RequireComponent(typeof(HingeJoint))]
    public class WinchManager : MonoBehaviour
    {
        [SerializeField] SoCraneSpecs _craneSpecs;
        [SerializeField] Crane _crane;
        HingeJoint joint;

        private void Start()
        {
            joint = GetComponent<HingeJoint>();
        }

        void Update()
        {  
            MoveWinch(_crane.winchSpeed);
            ManageWinchLimit();
        } 
        public void MoveWinch(float value)
        {
            // Adjust the value since the value provided is the speed in m/s
            // The motor target velocity is in degree/s
            // Every pulley has a diameter of 1 meter.
            // This means that for every rotation, 3.14m of cable is added
            // So 1 degree = 0.00872m of cable released of 1m/s of cable = 114.68 degree/s
            float diameter = 1;
            float degreeToM = 360 / (Mathf.PI * diameter);
            value *= degreeToM * _craneSpecs.winchMaxSpeed;

            // Get 2 motors (one side turns clockwise while the other side turns counter-clockwise
            JointMotor motor = joint.motor;
            
            float timeDelta = Time.deltaTime + 0.02f;
            float accel = _craneSpecs.winchAcceleration * timeDelta * degreeToM;
            float deltaV = Mathf.Abs(value - motor.targetVelocity);
            if (accel > deltaV) accel = deltaV;

            if (motor.targetVelocity < value) motor.targetVelocity += accel;
            if (motor.targetVelocity > value) motor.targetVelocity -= accel;

            motor.targetVelocity = Mathf.Clamp(motor.targetVelocity, -_craneSpecs.winchMaxSpeed*degreeToM, _craneSpecs.winchMaxSpeed*degreeToM);
            joint.motor = motor;
        }
        private void ManageWinchLimit()
        {
            if((_crane.spreader.Position.y > 25 && _crane.winchSpeed > 0) ||
                (_crane.spreader.Position.y < 0 && _crane.winchSpeed < 0))
            {
                var motor = joint.motor;
                motor.targetVelocity = 0;
                joint.motor = motor;
            }
        }
    }
}