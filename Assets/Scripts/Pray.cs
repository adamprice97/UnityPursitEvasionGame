using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;

public class Pray : MonoBehaviour
{

    public float range = 14f;
    public List<HunterAgent> currentCollisions = new List <HunterAgent> ();
    public float speed = 300f;
    Rigidbody m_Rb;
    EnvironmentParameters m_ResetParams;

    void Start() {
        m_Rb = gameObject.GetComponent<Rigidbody>();
        m_ResetParams = Academy.Instance.EnvironmentParameters;
        var krange = m_ResetParams.GetWithDefault("killRange", 2f);   
        BoxCollider collider = this.GetComponent(typeof(BoxCollider)) as BoxCollider;
        collider.size =  new Vector3(krange, 1, krange);

        speed  = m_ResetParams.GetWithDefault("praySpeed", 800);
    }

    //Track hunters in range
    void OnTriggerEnter(Collider collision) {
        var agent = collision.gameObject.GetComponents<HunterAgent>();
        foreach (HunterAgent a in agent) {
            if (a != null && !currentCollisions.Contains(a))
            currentCollisions.Add(a);
        }
    }
    void OnTriggerExit (Collider collision) {
        var agent = collision.gameObject.GetComponent<HunterAgent>();
        currentCollisions.Remove(agent);
    }

    public void Respawn() {

        m_Rb.velocity = Vector3.zero;
        m_Rb.angularVelocity = Vector3.zero;

        currentCollisions = new List <HunterAgent>();
        gameObject.transform.localPosition = new Vector3((1 - 2 * Random.value) * 1, 0.5f, (1 - 2 * Random.value) * 1);

        //Get curriculum update from the academy
        var krange = m_ResetParams.GetWithDefault("killRange", 2f);   
        BoxCollider collider = this.GetComponent(typeof(BoxCollider)) as BoxCollider;
        collider.size =  new Vector3(krange, 1, krange);

        speed  = m_ResetParams.GetWithDefault("praySpeed", 800);
    }

    private void FixedUpdate()
    {
        if (currentCollisions.Count > 2 ) { //All agents are in range
            //var environment = gameObject.transform.parent.gameObject;
            //var agents = environment.GetComponentsInChildren<Agent>();
            foreach (var a in currentCollisions) {
                a.AddReward(5f);
                a.EndEpisode();
            }     
            Respawn();
        }
        if (speed > 0f)
        {
            Move();
        }

        if (Mathf.Abs(DistanceToCentre(transform.position)) > range) {
            var environment = gameObject.transform.parent.gameObject;
            var agents = environment.GetComponentsInChildren<HunterAgent>();
            foreach (var a in agents) {
                a.AddReward(-3f);
                a.EndEpisode();
            }    
            Respawn();
        } 
    }

    private void Move()
    {
        var environment = gameObject.transform.parent.gameObject;
        var agents = environment.GetComponentsInChildren<HunterAgent>();
        Vector3 moveDirection = new Vector3(0f,0f,0f);
        for (int i = 0; i < 2; i++)  { 
            moveDirection += transform.position - agents[i].bodyObject.transform.position;
        } //Face away from hunters and apply force
        transform.rotation = Quaternion.LookRotation(moveDirection, Vector3.up);
        Vector3 moveVector = speed * moveDirection.normalized;
        if (m_Rb.velocity.magnitude < 0.1) {
            m_Rb.AddForce(moveVector); //kick start movement
        }
        m_Rb.AddForce(moveVector);
    }

    private float DistanceToCentre(Vector3 location) {
        var vec = location;
        vec.y = 0;
        return vec.magnitude;
    }

}
