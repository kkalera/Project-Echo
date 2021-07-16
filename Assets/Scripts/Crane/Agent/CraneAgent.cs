using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine.InputSystem;

public class CraneAgent : Agent, IAgent
{    
    [SerializeField] public CraneLevelManager levelManager;
    [SerializeField] InputAction inputActionX;
    [SerializeField] InputAction inputActionY;
    [SerializeField] InputAction inputActionZ;
    [SerializeField] bool autoPilot;
    [SerializeField] bool logData;

    private void Start()
    {        
        CheckLevelParameter();
        ICrane crane = GetComponentInChildren<ICrane>();
        levelManager.CurrentLevel.InitializeEnvironment(transform, crane);

        inputActionX.Enable();
        inputActionY.Enable();
        inputActionZ.Enable();
    }

    public override void OnEpisodeBegin()
    {
        CheckLevelParameter();
        levelManager.CurrentLevel.SetCraneRestrictions();        
        levelManager.CurrentLevel.ResetEnvironment();
    }

    private void CheckLevelParameter()
    {
        int level = (int)Academy.Instance.EnvironmentParameters.GetWithDefault("level_parameter", 0);
        levelManager.SetEnvironment(level);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        float inputX = actions.ContinuousActions[0];
        float inputY = actions.ContinuousActions[1];
        float inputZ = actions.ContinuousActions[2];

        levelManager.CurrentLevel.Crane.MoveCrane(inputX);
        levelManager.CurrentLevel.Crane.MoveWinch(inputY);
        levelManager.CurrentLevel.Crane.MoveCabin(inputZ);

        RewardData rewardData = levelManager.CurrentLevel.GetReward();
        SetReward(rewardData.reward);
        if (rewardData.endEpisode) EndEpisode();
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<float> continuousActions = actionsOut.ContinuousActions;
        continuousActions[0] = inputActionX.ReadValue<float>();
        continuousActions[1] = inputActionY.ReadValue<float>();
        continuousActions[2] = inputActionZ.ReadValue<float>();

        if (autoPilot)
        {
            Vector3 actions = AutoPilot.GetInputs(
                levelManager.CurrentLevel.TargetLocation,
                levelManager.CurrentLevel.Crane.SpreaderPosition,
                levelManager.CurrentLevel.Crane.SpreaderVelocity,
                0.4f);
            continuousActions[0] = actions.x;
            continuousActions[1] = actions.y;
            continuousActions[2] = actions.z;

            if (logData) DataLogger.WriteString("test.txt", "" + actions);
        }
                

    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(levelManager.CurrentLevel.Crane.CabinPosition);
        sensor.AddObservation(levelManager.CurrentLevel.Crane.CabinVelocity);
                              
        sensor.AddObservation(levelManager.CurrentLevel.Crane.CranePosition);
        sensor.AddObservation(levelManager.CurrentLevel.Crane.CraneVelocity);
                              
        sensor.AddObservation(levelManager.CurrentLevel.Crane.SpreaderPosition);
        sensor.AddObservation(levelManager.CurrentLevel.Crane.SpreaderVelocity);

        sensor.AddObservation(levelManager.CurrentLevel.TargetLocation);

        if (logData) DataLogger.WriteString("test.txt", "" + levelManager.CurrentLevel.Crane.CabinPosition + 
            ","+ levelManager.CurrentLevel.Crane.CabinVelocity +
            "," + levelManager.CurrentLevel.Crane.CranePosition +
            "," + levelManager.CurrentLevel.Crane.CraneVelocity +
            "," + levelManager.CurrentLevel.Crane.SpreaderPosition +
            "," + levelManager.CurrentLevel.Crane.SpreaderVelocity +
            "," + levelManager.CurrentLevel.TargetLocation
            );
    }



    public void OnCollisionEnter(Collision col)
    {
    }

    public void OnCollisionStay(Collision col)
    {
    }

    public void OnTriggerEnter(Collider other)
    {
    }

    public void OnTriggerExit(Collider other)
    {
    }

    public void OnTriggerStay(Collider other)
    {
    }
}