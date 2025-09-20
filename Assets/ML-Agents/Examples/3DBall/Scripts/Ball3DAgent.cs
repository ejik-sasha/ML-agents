using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using Random = UnityEngine.Random;

public class Ball3DAgent : Agent
{
    public Transform ball;
    public Transform head;           
    public float maxTilt = 10f;      
    public Rigidbody ballRb;
    private Vector3 initialHeadRot;

    public override void Initialize()
    {
        if (head != null) initialHeadRot = head.localEulerAngles;
    }

    public override void OnEpisodeBegin()
    {
        ballRb.linearVelocity = Vector3.zero;
        ballRb.angularVelocity = Vector3.zero;
        ball.localPosition = new Vector3(0f, 0.5f + Random.Range(-0.1f,0.1f), 0f);
        head.localEulerAngles = initialHeadRot + new Vector3(Random.Range(-5f,5f),0f,Random.Range(-5f,5f));
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        Vector3 relPos = head.InverseTransformPoint(ball.position);
        sensor.AddObservation(head.localRotation.eulerAngles / 180f - Vector3.one); 
        sensor.AddObservation(relPos);   
        sensor.AddObservation(ballRb.linearVelocity);  

    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        float tiltX = Mathf.Clamp(actions.ContinuousActions[0], -1f, 1f) * maxTilt;
        float tiltZ = Mathf.Clamp(actions.ContinuousActions[1], -1f, 1f) * maxTilt;

        head.localRotation = Quaternion.Euler(tiltX, head.localEulerAngles.y, tiltZ);

        if (BallOnHead())
        {
            AddReward(0.1f * Time.fixedDeltaTime * 50f); 
        }
        else
        {
            AddReward(-1f);
            EndEpisode();
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var cont = actionsOut.ContinuousActions;
        cont[0] = Input.GetAxis("Vertical");  
        cont[1] = Input.GetAxis("Horizontal");
    }

    private bool BallOnHead()
    {
        Vector3 localPos = head.InverseTransformPoint(ball.position);
        return localPos.y > 0.1f; 
    }
}