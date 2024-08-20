using KMGMath;
using KMGPhysics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroneAnimationController : MonoBehaviour
{
    public RobotAnimator robot;

    public Springs.DampedSpringSettings spring;

    public Vector2 hoverOffsetFromRobot;
    public float hoverRadius;
    public float meanSecondsPerChange;

    public float bobAmplitude;
    public float bobFrequency;

    public float leftWallBase;
    public float leftWallThickness;

    private Vector2 velocity;
    private Vector2 goalPoint;

    // Update is called once per frame
    void Update()
    {
        var shouldChange = Statistics.SamplePoisson(Time.deltaTime / meanSecondsPerChange) > 0;
        if(shouldChange) {
            goalPoint = Random.insideUnitCircle;
        }

        Vector2 g = goalPoint*hoverRadius + (Vector2)robot.transform.position + hoverOffsetFromRobot;


        if (IntroCoordinator.introCompleted) {
            g = new Vector2(goalPoint.x*leftWallThickness-leftWallBase, goalPoint.y * hoverRadius + robot.transform.position.y + hoverOffsetFromRobot.y);
        }

        g += Vector2.up * bobAmplitude * Mathf.Sin(Mathf.PI*2*Time.time * bobFrequency);
        Vector2 current = transform.position;
        Springs.ComputeMassSpringDamper(ref current, ref velocity, g, spring, Time.deltaTime);
        transform.position = current;
    }
}
