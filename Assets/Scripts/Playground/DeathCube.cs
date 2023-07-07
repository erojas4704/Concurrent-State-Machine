using UnityEngine;
using System;
using System.Linq;
using Playground.States;
using CSM;

namespace Playground
{
    public class DeathCube : MonoBehaviour
    {
        public bool reverse = false;
        public float speed = 10f;
        public Transform[] waypoints;
        private int waypointIndex = 0;

        void Start()
        {
            Transform closestWaypoint = waypoints.Aggregate((curr, next) =>
            {
                return Vector3.Distance(transform.position, curr.position) < Vector3.Distance(transform.position, next.position) ? curr : next;
            });
            waypointIndex = Array.IndexOf(waypoints, closestWaypoint);
        }
        // Update is called once per frame
        void Update()
        {
            Vector3 target = waypoints[reverse ? waypoints.Length - 1 - waypointIndex : waypointIndex].position;
            transform.position = Vector3.MoveTowards(transform.position, target, Time.deltaTime * speed);
            float distance = Vector3.Distance(transform.position, target);
            if (distance < 0.2f)
                waypointIndex = (waypointIndex + 1) % waypoints.Length;

            transform.Rotate(0, 90 * Time.deltaTime * speed * .25f, 0, Space.World);
        }

        void OnCollisionEnter(Collision col)
        {
            Collider other = col.collider;
            if (other.gameObject.CompareTag("Player"))
            {
                //TODO SHOULD ACCESS AN INTERFACE OR COMPONENT FOR DEALING DAMAGE. 
                //SHOULD NOT REFERENCE THE CSM WHATSOEVER.
                other.gameObject.GetComponent<Actor>().EnterState<Dead>();
            }
        }
    }
}