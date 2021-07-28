using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContainerDeck : CraneLevel
{    
    [SerializeField] private GameObject containerPrefab;
    [SerializeField] private GameObject targetPrefab;
    public bool enableSwing;
    private Transform container;
    private Transform target;
    private Transform environment;
    private Vector3 currentTarget;
    private ICrane _crane;
    public override ICrane Crane { get => _crane; set => _crane = value; }

    public override Vector3 TargetLocation => currentTarget;  

    public override void ClearEnvironment()
    {
        GameObject[] containers = GameObject.FindGameObjectsWithTag("container");
        foreach (GameObject c in containers)
        {
            Destroy(c);
        }
        
        Destroy(environment.Find("targetPlane").gameObject);
    }

    public override RewardData GetReward()
    {
        if (!_crane.ContainerGrabbed && target.position.z < 15f)
        {
            _crane.ReleaseContainer(environment);
            return new RewardData(1f, true);
        }
        return new RewardData();
    }

    public override void IncreaseDifficulty(){}

    public override void InitializeEnvironment(Transform environment, ICrane crane)
    {
        _crane = crane;
        this.environment = environment;

        GameObject containerobj = Instantiate(containerPrefab, this.environment);
        containerobj.tag = "container";
        container = containerobj.transform;

        GameObject targetobj = Instantiate(targetPrefab, this.environment);
        targetobj.name = "targetPlane";
        target = targetobj.transform;
        
    }

    private void Update()
    {
        if(!_crane.ContainerGrabbed && Vector3.Distance(_crane.SpreaderWorldPosition, container.position + new Vector3(0,2.85f,0)) < 0.3f)
        {
            _crane.GrabContainer(container);
            currentTarget = target.position + new Vector3(0, 2.85f,0);
        }

        if(_crane.ContainerGrabbed &&
            Mathf.Approximately(container.position.y+2.85f, currentTarget.y) &&
            Mathf.Abs(container.position.z - currentTarget.z) < 0.5f)
        {
            _crane.ReleaseContainer(environment);

            target.position = container.position - new Vector3(0, 0, 2.6f);

            GameObject containerobj = Instantiate(containerPrefab, this.environment);
            containerobj.tag = "container";
            container = containerobj.transform;
            container.transform.localPosition = new Vector3(0, 0, Random.Range(-4, 4));

            currentTarget = container.position + new Vector3(0, 2.85f, 0);

        }
    }

    public override void ResetEnvironment()
    {
        ClearEnvironment();
        InitializeEnvironment(environment, _crane);

        _crane.ReleaseContainer(environment);
        container.transform.rotation = Quaternion.Euler(Vector3.zero);
        container.transform.localPosition = new Vector3(0, 0, Random.Range(-4,4));
        target.transform.localPosition = new Vector3(0, 0, 40);
        _crane.ResetToPosition(new Vector3(0, 25, Random.Range(-25,40)));
        currentTarget = container.transform.position;
    }

    public override void SetCraneRestrictions()
    {        
        _crane.CraneMovementDisabled = true;
        _crane.CabinMovementDisabled = false;        
        _crane.WinchMovementDisabled = false;
        _crane.SwingDisabled = !enableSwing;        
        _crane.SetWinchLimits(0, 30);
    }
}