using System;
using CSM;
using UnityEngine;

namespace Playground.States.Player
{
    [Serializable]
    public partial class PlayerStats : Stats
    {
        [SerializeField] private float speed;
        [SerializeField] private float acceleration;
        [SerializeField] private float airAcceleration;
        [SerializeField] public float sprintSpeed;
        [SerializeField] private float attackSpeed;
        [SerializeField] private float drag;
        [SerializeField] private float friction;
        [SerializeField] private float ladderClimbSpeed;
        [SerializeField] private float jumpHangTime = 0.55f;
        [SerializeField] private float coyoteTime = 0.015f;
        [SerializeField] private float jumpForce = 7.5f;
        [SerializeField] private float turnSpeed = 10f;
    }
}