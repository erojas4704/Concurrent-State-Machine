using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CSM;
using CSM.States;

public class DeathCube : MonoBehaviour
{
    public float speed = 10f;
    public Transform[] waypoints;
    private int waypointIndex = 0;

    // Update is called once per frame
    void Update()
    {
        Vector3 target = waypoints[waypointIndex].position;
        transform.position = Vector3.MoveTowards(transform.position, target, Time.deltaTime * speed);
        float distance = Vector3.Distance(transform.position, target);
        if (distance < 0.2f)
            waypointIndex = (waypointIndex + 1) % waypoints.Length;

        transform.Rotate(0, 90 * Time.deltaTime * speed * .25f, 0, Space.World);
    }

    void OnCollisionEnter(Collision col)
    {
        Collider other = col.collider;
        if (other.gameObject.tag == "Player")
        {
            other.gameObject.GetComponent<Actor>().EnterState<Dead>();
        }

    }
}
