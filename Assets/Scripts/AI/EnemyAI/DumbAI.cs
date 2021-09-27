using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//Tasks

//Edge detection, if the enemy is going to reach the end of a platform (by check if raycast on it's edges) then turn the other direction
//Attacking, if the enemy detects a player (via physics or checking manual) then follow player until edge is reached
//Wandering & idle, enemy will choose to wander and idle periodically randomly (for now)

[RequireComponent(typeof(RaycastCollider2D))]
public class DumbAI : MonoBehaviour
{
    private enum BehaviourStates {IDLE,PATROL,ATTACK}
    private enum Direction { LEFT, RIGHT};
    public LayerMask CollisionMask;

    public float MovementSpeed;
    public Transform Target;

    public LayerMask targetCollisionMask;
    public float AttackDistance;
    public float SpotDistance;
    public float BulletVelocity;
    private Direction CurrentDirection = Direction.RIGHT;
    private BehaviourStates CurrentState = BehaviourStates.PATROL;
    private RaycastCollider2D raycastCollider;
    private SpriteRenderer sprite;
    private AIWeapon weapon;
    private void Start() {
        weapon = this.GetComponent<AIWeapon>();
        raycastCollider = this.GetComponent<RaycastCollider2D>();
        sprite = this.GetComponent<SpriteRenderer>();
    }


    public float IdleInterval = 1.0f;
    public float IdleLength = 1.0f;

    public float IdleRandomRange = 2f;

    private float currentTimeLength = 0;

    private float stateTimer;

    public void Update() {

        if (Vector3.Distance(Player.instance.transform.position, this.transform.position) < SpotDistance) {
            Vector3 playerPos = Player.instance.transform.position;
            Vector3 dir = playerPos - this.transform.position;
            float distance = dir.magnitude;
            //Normalize
            dir = dir / distance;

            if(!Physics2D.Raycast(this.transform.position, dir, distance, targetCollisionMask)) {
                Target = Player.instance.transform;
            }


        } else {
            Target = null;
        }

        if (Target != null) {
            CurrentState = BehaviourStates.ATTACK;
        } else {


            stateTimer += Time.deltaTime;
            if (CurrentState == BehaviourStates.IDLE) {
                if (stateTimer > currentTimeLength) {
                    stateTimer = 0;
                    CurrentState = BehaviourStates.PATROL;

                    currentTimeLength += IdleInterval + (IdleRandomRange * Random.value);
                    //Random direction upon awake

                    if (Random.value > 0.5f) {
                        SwitchDirection(Direction.RIGHT);
                    } else {
                        SwitchDirection(Direction.LEFT);
                    }

                }
            } else if(CurrentState == BehaviourStates.PATROL){

                if (stateTimer > currentTimeLength) {
                    stateTimer = 0;
                    currentTimeLength += IdleLength + (IdleRandomRange * Random.value);
                    CurrentState = BehaviourStates.IDLE;
                }
            } else {
                CurrentState = BehaviourStates.PATROL;
            }

        }

        switch (CurrentState) {
            case BehaviourStates.IDLE:
                break;
            case BehaviourStates.PATROL:
                UpdateMove();
                break;
            case BehaviourStates.ATTACK:
                MoveTowardsTarget(Target.transform.position);
                break;
        }


    }

    public void UpdateMove() {
        Vector3 dir = CurrentDirection == Direction.LEFT ? Vector2.left : Vector2.right;

        if (!CheckEdge(CurrentDirection == Direction.LEFT ? raycastCollider.raycastPositions.trueBottomLeftEdge : raycastCollider.raycastPositions.trueBottomRightEdge) || (CurrentDirection == Direction.LEFT ? raycastCollider.isCollidingLeft : raycastCollider.isCollidingRight)) {
            dir = -dir;
            //Flip direction
            CurrentDirection = (CurrentDirection == Direction.LEFT ? Direction.RIGHT : Direction.LEFT);
            Vector3 curScale = this.transform.localScale;
            curScale.x *= -1;
            this.transform.localScale = curScale;
        }


        this.transform.position += dir * MovementSpeed * Time.deltaTime;
    }


    public void MoveTowardsTarget(Vector3 pos) {

        if (Vector2.Distance(this.transform.position, pos) < AttackDistance * 0.95f) {
            weapon.Shoot((pos - this.transform.position).normalized, BulletVelocity);
            return;
        }

        //To our right
        if(pos.x > this.transform.position.x + raycastCollider.Width) {
            SwitchDirection(Direction.RIGHT);
        } else if(pos.x < this.transform.position.x) {
            SwitchDirection(Direction.LEFT);
        } else {
            //We are on our target we will not move

            //A better behaviour would probably be get to a side of the player instead of being on the player, or attack the player via melee
            return;
        }

        //Move if we can move to the side of us
        if (CheckEdge(CurrentDirection == Direction.LEFT ? raycastCollider.raycastPositions.trueBottomLeftEdge : raycastCollider.raycastPositions.trueBottomRightEdge) || (CurrentDirection == Direction.LEFT ? raycastCollider.isCollidingLeft : raycastCollider.isCollidingRight)) {


            Vector3 dir;

            Vector3 curScale = this.transform.localScale;

            if (Direction.LEFT == CurrentDirection) {
                dir = Vector3.left;
            } else {
                dir = Vector3.right;
            }


            this.transform.localScale = curScale;

            this.transform.position += dir * MovementSpeed * Time.deltaTime;

            //If we can move, do so

        }

    }


    private void SwitchDirection(Direction d) {
        CurrentDirection = d;
        Vector3 curScale = this.transform.localScale;
        if (Direction.LEFT == CurrentDirection) {
            curScale.x = Mathf.Abs(curScale.x) * -1;
        } else {
            curScale.x = Mathf.Abs(curScale.x);
        }
        this.transform.localScale = curScale;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="origin"></param>
    /// <returns>Has an platform to its left or not</returns>
    private bool CheckEdge(Vector3 origin) {
        return Physics2D.Raycast(origin, Vector3.down, 0.125f, CollisionMask);
    }

}
