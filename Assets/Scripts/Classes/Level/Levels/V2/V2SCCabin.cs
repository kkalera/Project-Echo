using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class V2SCCabin : CraneLevel
{
    [Header("Height")]
    [SerializeField] [Range(1, 25)] float startSpreaderHeight = 25;
    [SerializeField] [Range(1, 25)] float endSpreaderHeight = 10;
    [SerializeField] [Range(-0.0001f, 1)] float heightDiscount = 0.01f;
    [Space(10)]
    [SerializeField] [Range(-10, -1)] private float spawnMin = -1f;
    [SerializeField] [Range(1, 35)] private float spawnMax = 1f;
    [Space(10)]
    [SerializeField] int _maxStep = 5000;
    [SerializeField] Transform targetIndicator;
    [SerializeField] TMPro.TextMeshPro _tmpro;


    private ICrane crane;
    private Vector3 _targetLocation;
    private readonly List<float> swings = new List<float>();


    public override Vector3 TargetLocation => _targetLocation;

    public override void ResetEnvironment(ICrane crane)
    {
        this.crane = crane;
    }

    public override void OnEpisodeBegin()
    {
        crane.CabinMovementDisabled = false;
        crane.SwingDisabled = false;
        crane.WinchMovementDisabled = true;

        spawnMin = Mathf.Max(spawnMin - heightDiscount, -10);
        spawnMax = Mathf.Min(spawnMax + heightDiscount, 35);

        startSpreaderHeight = Mathf.Clamp(startSpreaderHeight - heightDiscount, endSpreaderHeight, startSpreaderHeight);

        _targetLocation = new Vector3(0, startSpreaderHeight, Random.Range(spawnMin, spawnMax));
        targetIndicator.transform.localPosition = _targetLocation;

        Vector3 newCraneLocation = new Vector3(0, startSpreaderHeight, Random.Range(spawnMin, spawnMax));
        crane.ResetToPosition(newCraneLocation);

        Utils.ReportStat(startSpreaderHeight, "Spreader height");
        Utils.ReportStat(spawnMin, "SpawnMin");
        Utils.ReportStat(spawnMax, "Spawnmax");

    }

    public override RewardData Step(Collision col = null, Collider other = null)
    {
        // Define rewardData variable
        RewardData rd = new RewardData(-0.5f / _maxStep);

        // Add the swing reward
        rd.reward += GetSwingReward();

        bool endEpisode = Vector3.Distance(_targetLocation, crane.SpreaderPosition) < 1;

        // Add a reward for finishing the environment
        if (endEpisode) rd.reward += 1;

        // Calculate the distance to the target to determine wether or not to end the episode at the next step       
        rd.endEpisode = endEpisode;

        return rd;

    }
    private void Update()
    {
        // Calculate and save the amount of swing
        float swing = Mathf.Abs(crane.CabinPosition.z - crane.SpreaderPosition.z + 1);
        swings.Add(swing);
    }
    private float GetSwingReward()
    {
        if (swings.Count == 0 || crane.CabinVelocity.magnitude == 0) return 0;

        // Calculate the amount of average swing between steps.
        float totalSwing = 0;

        for (int i = 0; i < swings.Count; i++)
        {
            totalSwing += swings[i];
        }

        float avgSwing = totalSwing / swings.Count;
        swings.Clear();

        Utils.ReportStat(avgSwing, "Swing");

        // Calculate a reward based upon the average swing. 
        float maxSwing = Mathf.Abs(crane.CabinPosition.y - crane.SpreaderPosition.y);
        float swingNorm = Utils.Normalize(avgSwing, 0, maxSwing);
        float swingReward = Mathf.Pow(1 - swingNorm, 8) / _maxStep;
        swingReward = Mathf.Clamp(swingReward, 0, 1);

        return swingReward;
    }
}
