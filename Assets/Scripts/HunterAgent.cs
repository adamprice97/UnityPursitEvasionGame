using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
public class HunterAgent : Agent
{
    public GameObject target;
    public GameObject bodyObject;
    public GameObject ally1;
    public GameObject ally2;
    Rigidbody m_Rb;
    Vector3 m_LookDir;
    public float strength = 10f;
    public float range = 15f;
    EnvironmentParameters m_ResetParams;

    public override void Initialize() {
        m_Rb = gameObject.GetComponent<Rigidbody>();
        m_LookDir = Vector3.zero;
    }

    public override void CollectObservations(VectorSensor sensor) {      
        sensor.AddObservation(bodyObject.transform.localPosition.x);
        sensor.AddObservation(bodyObject.transform.localPosition.z);
        sensor.AddObservation(ally1.transform.localPosition.x - bodyObject.transform.localPosition.x);
        sensor.AddObservation(ally1.transform.localPosition.z - bodyObject.transform.localPosition.z);
        sensor.AddObservation(ally2.transform.localPosition.x - bodyObject.transform.localPosition.x);
        sensor.AddObservation(ally2.transform.localPosition.z - bodyObject.transform.localPosition.z);
        sensor.AddObservation(target.transform.localPosition.x - bodyObject.transform.localPosition.x);
        sensor.AddObservation(target.transform.localPosition.z - bodyObject.transform.localPosition.z);
        sensor.AddObservation(target.transform.rotation);
    }

    public override void OnActionReceived(float[] vectorAction) {
        for (var i = 0; i < vectorAction.Length; i++) {
            vectorAction[i] = Mathf.Clamp(vectorAction[i], -1f, 1f);
        }
        var x = vectorAction[0];
        var z = vectorAction[1];
        m_Rb.AddForce(new Vector3(x, 0, z) * strength);

        AddReward(-0.001f);

        m_LookDir = new Vector3(x, 0, z);

    }

    public override void OnEpisodeBegin() {

        float angle =  Random.Range(0, Mathf.PI*2);
        gameObject.transform.localPosition = new Vector3(Mathf.Sin(angle)*range,0.5f,Mathf.Cos(angle)*range);

        m_Rb.velocity = default(Vector3);
    }
    void FixedUpdate() {

        if (Mathf.Abs(DistanceToCentre(transform.position)) > range + 1) { 
            AddReward(-3f);
            HunterAgent hunt1 = ally1.GetComponent(typeof(HunterAgent)) as HunterAgent;
            hunt1.EndEpisode();
            HunterAgent hunt2 = ally2.GetComponent(typeof(HunterAgent)) as HunterAgent;
            hunt2.EndEpisode();
            EndEpisode();
            return;
        }

        if (this.StepCount > 1000) {
            EndEpisode();
            Pray p = target.GetComponent(typeof(Pray)) as Pray;
            p.Respawn();
        }
    }

    public override void Heuristic(float[] actionsOut) {
        actionsOut[0] = Input.GetAxis("Horizontal");
        actionsOut[1] = Input.GetAxis("Vertical");
    }

    void Update() {
        if (m_LookDir.magnitude > float.Epsilon) {
            bodyObject.transform.rotation = Quaternion.Lerp(bodyObject.transform.rotation,
                Quaternion.LookRotation(m_LookDir),
                Time.deltaTime * 10f);
        }
    }

    private float DistanceToCentre(Vector3 location) {
        var vec = location;
        vec.y = 0;
        return vec.magnitude;
    }

}
