using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KartPhysicsData : ScriptableObject
{
    /*TURN*/
    public float MaxTurn;
    public float TurnSpeed;
    public float RelaxTurn;
    public float TurnSpeedSlowdown;
    public float TurnLimiter;
    public float TurnLimiterNeg;
    /*ACCELERATION*/
    public float AccelerationSpeed;
    public float DeaccelerationSpeed;
    public float MaxEngineVelocity;
    /*FRICTION*/
    public Vector3 AngularSidewaysFriction;
    public float GeneralFriction;
    public float VeloTurnMultiplier;
    /*GRAVITY*/
    public float GravityPushback;
    public Vector3 GravityForce;
}
