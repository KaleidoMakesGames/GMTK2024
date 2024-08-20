using KMGMath;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HazardSpawner : MonoBehaviour
{
    public DifficultyCurve difficulty;

    public RobotController robot;
    // Update is called once per frame
    void Update()
    {
        if(!IntroCoordinator.introCompleted) {
            return;
        }
        float hazardsPerSecond = difficulty.hazardsPerSecondOverHeight.Evaluate(robot.transform.position.y / 1000.0f) * difficulty.maxHazardsPerSecond;
        Debug.Log(hazardsPerSecond);
        for (int i = 0; i < Statistics.SamplePoisson(Time.deltaTime * hazardsPerSecond); i++) {
            Spawn();
        }
    }

    public void Spawn() {
        var probs = difficulty.hazards.Select(x => x.probabilityVsHeight.Evaluate(robot.transform.position.y / 1000.0f));
        float totalProb = probs.Sum();
        if(totalProb == 0) {
            return;
        }
        var normProbs = probs.Select(x => x / totalProb).ToArray();
        float v = Random.value;
        float t = 0;
        for(int i = 0; i < difficulty.hazards.Count; i++) {
            t += normProbs[i];
            if(v < t) {
                OffscreenSpawner.SpawnRelative(difficulty.hazards[i].prefab.transform, Random.value, 10, difficulty.hazards[i].prefab.width);
                return;
            }
        }
    }
}
