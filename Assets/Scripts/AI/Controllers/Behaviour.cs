﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Panda;

public class Behaviour : MonoBehaviour {

    private MovingObject EnemyObject;
    private Vector2 ShootingTarget;
    private Vector2 MovementTarget;
    private float minDistanceNotToIgnore = 3f;
    private int SwarmSize = 2;

    private List<Vector2> movementPath;

    #region MOVEMENT METHODS ------------------------------------------------------------------------------------------------------------------------------
    [Task]
    public void BuildPath() { // Builds a path to be taken towards the MovementTarget
        MovingObject currentObject = gameObject.GetComponent<MovingObject>();

        if (MovementTarget != null)
            movementPath = FindPath.run(currentObject, MovementTarget);

        Task.current.Succeed();
    }

    [Task]
    public void Move() { // Goes to the next position in the Movmement Path
        MovingObject obj = gameObject.GetComponent<MovingObject>();

        if (MovementTarget != null && !MovementTarget.Equals((Vector2) obj.transform.position)) {
            MoveToPosition.run(obj, MovementTarget, movementPath);
            Task.current.Succeed();
            return;
        }

        Task.current.Fail();
    }

    [Task]
    public void MoveToEnemy() { // Changes MovementTarget to a ClosestEnemy
        MovingObject obj = gameObject.GetComponent<MovingObject>();

        List<MovingObject> players = GameManager.Instance.players.Cast<MovingObject>().ToList();
        EnemyObject = FindClosestTarget.closestTarget(obj, players);
        if (EnemyObject != null) {
            MovementTarget = EnemyObject.transform.position;
            Task.current.Succeed();
            return;
        }

        Task.current.Fail();
    }

    [Task]
    public void MoveToHidingSpot() { // Changes MovementTarget to a HidingSpot
        MovingObject obj = gameObject.GetComponent<MovingObject>();

        List<MovingObject> players = GameManager.Instance.players.Cast<MovingObject>().ToList();
        EnemyObject = FindClosestTarget.closestTarget(obj, players);
        MovementTarget = FindClosestHidingSpot.run(obj, EnemyObject);

        if (MovementTarget.Equals((Vector2) obj.transform.position)) {
            Task.current.Fail();
            return;
        }

        Task.current.Succeed();
    }



    #endregion MOVEMENT METHODS -------------------------------------------------------------------------------------------------------------------------


    #region SWARM METHODS -------------------------------------------------------------------------------------------------------------------------

    [Task]
    public void SwarmMoveToEnemy()
    { // Changes MovementTarget to ClosestEnemy or Enemy with lowest health.
        MovingObject currentObject = gameObject.GetComponent<MovingObject>();
        List<MovingObject> targets = new List<MovingObject>();
        for (int i = 0; i < GameManager.Instance.players.Count; i++)
            targets.Add(GameManager.Instance.players[i]);

        MovingObject target = FindLowestHealthTarget.LowestTarget(currentObject, targets);
        MovingObject potentialTarget = FindClosestTarget.closestTarget(currentObject, targets);
        if (target != null && potentialTarget != null)
        {
            float distance = Vector3.Distance(currentObject.transform.position, potentialTarget.transform.position);
            if (distance <= minDistanceNotToIgnore)
                MovementTarget = potentialTarget.transform.position;
            else
                MovementTarget = target.transform.position;

            Task.current.Succeed();
            return;
        }

        Task.current.Fail();
    }

    [Task]
    private void InRange() // Checks if inrange to shootfaster
    {
        MovingObject currentObject = gameObject.GetComponent<MovingObject>();
        int count = 0;
        for (int i = 0; i < GameManager.Instance.enemies.Count; i++)
            if (Vector3.Distance(currentObject.transform.position, GameManager.Instance.enemies[i].transform.position) < range)
                count++;
        if (count > SwarmSize)
            currentObject.TimeBetweenShotsMain = 0.2f;
        else
            currentObject.TimeBetweenShotsMain = 0.6f;
        Task.current.Succeed();
    }

    #endregion SWARM METHODS -------------------------------------------------------------------------------------------------------------------------

    [Task]
    public void ShootTarget() {
        bool mainFire = !(Random.value > 0.7);

        MovingObject obj = gameObject.GetComponent<MovingObject>();

        ShootPosition.run(obj, ShootingTarget, mainFire);

        Task.current.Succeed();
    }
}