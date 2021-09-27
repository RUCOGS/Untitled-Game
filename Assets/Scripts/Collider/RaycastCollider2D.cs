using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public class RaycastCollider2D : MonoBehaviour {
    [HideInInspector]
    public RaycastPositions raycastPositions;
    private Collider2D PlayerCollider;

    public float SkinThickness;
    public int WidthRaycastsCount = 8;
    public int HeightRaycastsCount = 8;

    public LayerMask collisionMask;
    public LayerMask onewayColliderMask;
    private float WidthRaycastSpacing;
    private float HeightRaycastSpacing;
    
    
    public bool PassthroughOneways = false;

    //private bool UseJobs = false;

    public float Width { get; private set; }
    public float Height { get; private set; }

    public struct RaycastPositions {
        public Vector2 bottomLeft, bottomRight, topLeft, topRight;

        //Optional use
        public Vector2 trueBottomLeftEdge, trueBottomRightEdge;
    }

    void Start() {
        PlayerCollider = GetComponent<Collider2D>();
        UpdateRaycastPositions();

    }

    /// <summary>
    /// Used to get the bounds for the raycasting controller
    /// </summary>
    public void UpdateRaycastPositions() {
        raycastPositions = new RaycastPositions();
        Bounds bounds = PlayerCollider.bounds;

        raycastPositions.trueBottomLeftEdge = new Vector2(bounds.min.x, bounds.min.y);
        raycastPositions.trueBottomRightEdge = new Vector2(bounds.max.x, bounds.min.y);


        bounds.Expand(SkinThickness * -2);
        bounds.center = this.transform.position;

        raycastPositions.bottomLeft = new Vector2(bounds.min.x, bounds.min.y);
        raycastPositions.bottomRight = new Vector2(bounds.max.x, bounds.min.y);
        raycastPositions.topLeft = new Vector2(bounds.min.x, bounds.max.y);
        raycastPositions.topRight = new Vector2(bounds.max.x, bounds.max.y);

        Width = Vector2.Distance(raycastPositions.bottomLeft, raycastPositions.bottomRight);
        Height = Vector2.Distance(raycastPositions.bottomLeft, raycastPositions.topLeft);

        WidthRaycastSpacing = Width / (WidthRaycastsCount - 1);
        HeightRaycastSpacing = Height / (HeightRaycastsCount - 1);
    }

    public bool isCollidingTop { get; private set; }
    public bool isCollidingBottom { get; private set; }
    public bool isGrounded { get; private set; }
    public bool isCollidingLeft { get; private set; }
    public bool isCollidingRight { get; private set; }

    private CollisionProperties collisionBottom;
    private CollisionProperties collisionTop;
    private CollisionProperties collisionLeft;
    private CollisionProperties collisionRight;


    /// <summary>
    /// Used to update collision booleans
    /// </summary>
    public void UpdateCollisions() {
        UpdateRaycastPositions();

        //Top collision detection and rebounds

        collisionTop = CheckCollision(Vector2.up, raycastPositions.topLeft, raycastPositions.topRight, WidthRaycastsCount, WidthRaycastSpacing, SkinThickness);
        collisionBottom = CheckCollision(Vector2.down, raycastPositions.bottomLeft, raycastPositions.bottomRight, WidthRaycastsCount, WidthRaycastSpacing, SkinThickness);
        collisionLeft = CheckCollision(Vector2.left, raycastPositions.bottomLeft, raycastPositions.topLeft, HeightRaycastsCount, HeightRaycastSpacing, SkinThickness);
        collisionRight = CheckCollision(Vector2.right, raycastPositions.bottomRight, raycastPositions.topRight, HeightRaycastsCount, HeightRaycastSpacing, SkinThickness);



        isGrounded = CheckCollision(Vector2.down, raycastPositions.bottomLeft, raycastPositions.bottomRight, WidthRaycastsCount, WidthRaycastSpacing, SkinThickness + 0.075f).collided;

        isCollidingTop = collisionTop.collided;
        isCollidingBottom = collisionBottom.collided;
        isCollidingLeft = collisionLeft.collided;
        isCollidingRight = collisionRight.collided;
    }

    public void UpdateUnstucks() {
        if (isCollidingTop) Unstuck(collisionTop.hit, new Vector3(0, -1));
        if (isCollidingBottom) Unstuck(collisionBottom.hit, new Vector3(0, 1));
        if (isCollidingLeft) Unstuck(collisionLeft.hit, new Vector3(1, 0));
        if (isCollidingRight) Unstuck(collisionRight.hit, new Vector3(-1, 0));
    }


    /// <summary>
    /// Shoots multiple rays out to check collisions along one line with a direction.
    /// </summary>
    /// <param name="direction"></param>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <param name="amount"></param>
    /// <param name="spacing"></param>
    /// <returns>Data of what happen with collision</returns>
    private CollisionProperties CheckCollision(Vector2 direction, Vector2 a, Vector2 b, int amount, float spacing, float rayLength) {
        Vector2 spacingDir = (b - a).normalized;
        CollisionProperties properties= new CollisionProperties();
        properties.collided = false;

        for (int i = 0; i < amount; i++) {
            RaycastHit2D hit = Physics2D.Raycast(a + (spacingDir * spacing * i), direction, rayLength, collisionMask);
            if (hit.collider != null && (!(hit.collider.gameObject.tag == "Oneway Platform" && direction != Vector2.down) && !(hit.collider.gameObject.tag == "Oneway Platform" && PassthroughOneways))) {

                //Debug.DrawRay(a + (spacingDir * spacing * i), direction * rayLength, Color.green);
                properties.collided = true;
                properties.hit = hit;
                return properties;
            } else {
                //Debug.DrawRay(a + (spacingDir * spacing * i), direction * rayLength, Color.red);
            }
        }

        return properties;
    }


    private void Unstuck(RaycastHit2D hit2D, Vector3 direction) {
        if (hit2D.distance < SkinThickness) {
            float offset = SkinThickness - hit2D.distance;
            this.transform.position += offset * direction;
        }
    }

    private struct CollisionProperties{
        public bool collided;
        public RaycastHit2D hit;

    }



    //JOBS BASED COLLISION DETECTION
    /// <summary>
    /// !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!! WARNING DOES NOT WORK BECAUSE OF RAYCAST2D NOT BEING ABLE TO BE USED OVER THREADS D: 
    /// </summary>
    /*
    NativeArray<CollisionJobDataIn> j_collisionData;
    NativeArray<bool> j_hits;
    NativeArray<CollisionProperties> j_hitproperties;
    CollisionJob j_currentJob;
    JobHandle j_currentJobHandler;
    bool j_started = false;
    bool j_disposed = true;

    struct CollisionJobDataIn {

        public Vector2 spacingDir;
        public Vector2 rayDir;
        public Vector2 startPoint;
        public float rayLength;
        public float spacing;
        public int collisionMask;
    }

    struct CollisionJob : IJobParallelFor {

        [ReadOnly]
        public NativeArray<CollisionJobDataIn> dataArray;

        public NativeArray<bool> hits;
        public NativeArray<CollisionProperties> properties;

        [ReadOnly]
        public float widthAmount;
        [ReadOnly]
        public float heightAmount;

        public void Execute(int i) {

            CollisionJobDataIn data = dataArray[getJobDataIndex(i)];

            RaycastHit2D hit = Physics2D.Raycast(data.startPoint + (data.spacingDir * data.spacing * i), data.rayDir, data.rayLength, data.collisionMask);
            if (hit.collider != null) {
                Debug.DrawRay(data.startPoint + (data.spacingDir * data.spacing * i), data.rayDir * data.rayLength, Color.green);
                CollisionProperties properties = new CollisionProperties();
                properties.collided = true;
                properties.hit = hit;

                this.hits[i] = true;
                this.properties[i] = properties;
            } else {
                this.hits[i] = false;
                Debug.DrawRay(data.startPoint + (data.spacingDir * data.spacing * i), data.rayDir * data.rayLength, Color.red);
            }
        }


        private int getJobDataIndex(int i) {

            if(i >= 0 && i < widthAmount) {
                return 0; //Top
            }else if(i >= widthAmount && i < (widthAmount * 2)) {
                return 1; //Bottom
            } else if( i >= (widthAmount * 2) && i < (widthAmount * 2) + heightAmount) {
                return 2; //Left
            } else if(i >= (widthAmount * 2) + heightAmount && i < (widthAmount * 2) + (heightAmount * 2)) {
                return 3; //Right
            }else if(i >= (widthAmount * 2) + (heightAmount * 2)) { // i < (widthAmount*3) + (heightamount*2)
                return 4; //Grounded
            }
            return -1;
        }
    }

    private CollisionJobDataIn jGenerateCollisionDataIn(Vector2 direction, Vector2 a, Vector2 b, int amount, float spacing, float rayLength) {
        Vector2 g_spacingDir = (b - a).normalized;

        return new CollisionJobDataIn() {
            spacingDir = g_spacingDir,
            rayDir = direction,
            startPoint = a,
            rayLength = rayLength,
            spacing = spacing,
            collisionMask = this.collisionMask
        };
    }


    private void jStartCollisionJobs() {
        UpdateRaycastPositions();


        //amount - 1

        //widthAmount
        //heightAmount

        //0 Top       = 0                             -> widthAmount
        //1 Bottom    = widthAmount                   -> widthAmount*2
        //2 Left      = widthAmount*2                 -> widthAmount*2 + heightAmount
        //3 Right      = widthAmount*2 + heightAmount  -> widthAmount*2 + heightAmount*2
        //4 IsGrounded = widthAmount*2 + heightAmount*2 -> widthAmount*3 + heightAmount*2
        j_started = true;

        if (!j_disposed) {
            disposeJob();
        }

        int totalAmount = (WidthRaycastsCount * 3) + (HeightRaycastsCount * 2);

        j_collisionData = new NativeArray<CollisionJobDataIn>(5, Allocator.TempJob);

        j_collisionData[0] = jGenerateCollisionDataIn(Vector2.up, raycastPositions.topLeft, raycastPositions.topRight, WidthRaycastsCount, WidthRaycastSpacing, SkinThickness);
        j_collisionData[1] = jGenerateCollisionDataIn(Vector2.down, raycastPositions.bottomLeft, raycastPositions.bottomRight, WidthRaycastsCount, WidthRaycastSpacing, SkinThickness);
        j_collisionData[2] = jGenerateCollisionDataIn(Vector2.left, raycastPositions.bottomLeft, raycastPositions.topLeft, HeightRaycastsCount, HeightRaycastSpacing, SkinThickness);
        j_collisionData[3] = jGenerateCollisionDataIn(Vector2.right, raycastPositions.bottomRight, raycastPositions.topRight, HeightRaycastsCount, HeightRaycastSpacing, SkinThickness);
        j_collisionData[4] = jGenerateCollisionDataIn(Vector2.down, raycastPositions.bottomLeft, raycastPositions.bottomRight, WidthRaycastsCount, WidthRaycastSpacing, SkinThickness + 0.05f);

        j_hits = new NativeArray<bool>(totalAmount, Allocator.TempJob);
        j_hitproperties = new NativeArray<CollisionProperties>(totalAmount, Allocator.TempJob);

        j_currentJob = new CollisionJob {
            dataArray = j_collisionData,
            hits = j_hits,
            properties = j_hitproperties
        };


        j_currentJobHandler = j_currentJob.Schedule(totalAmount, Mathf.Max(1, totalAmount / 2, 32));
        j_disposed = false;
    }


    private void jUpdateCollisionsJobs() {
        if (!j_started) {
            jStartCollisionJobs();
        }

        int widthAmount = WidthRaycastsCount;
        int heightAmount = WidthRaycastsCount;

        j_currentJobHandler.Complete();

        var topTest = jHasHit(0, widthAmount);
        var bottomTest = jHasHit(widthAmount, (widthAmount * 2));
        var leftTest = jHasHit((widthAmount * 2), (widthAmount * 2) + heightAmount);
        var rightTest = jHasHit((widthAmount * 2) + heightAmount, (widthAmount * 2) + (heightAmount * 2));

        isGrounded = jHasHit((widthAmount * 2) + (heightAmount * 2), (widthAmount * 3) + (heightAmount * 2)).collided;

        isCollidingTop = topTest.collided;
        isCollidingBottom = bottomTest.collided;
        isCollidingLeft = leftTest.collided;
        isCollidingRight = rightTest.collided;

        if(isCollidingTop) collisionTop = jGetHitProperty(topTest.index);
        if (isCollidingBottom) collisionBottom = jGetHitProperty(bottomTest.index);
        if (isCollidingLeft) collisionLeft = jGetHitProperty(leftTest.index);
        if (isCollidingRight) collisionRight = jGetHitProperty(rightTest.index);

        disposeJob();
    }


    private (bool collided, int index) jHasHit(int startIndex, int endIndex) {
        int i = startIndex;
        for(; i < endIndex; i++) {
            if (j_currentJob.hits[i]) {
                return (true, i);
            }
        }

        return (false, -1);
    }

    private CollisionProperties jGetHitProperty(int i) {
        return j_currentJob.properties[i];
    }

    private void disposeJob() {
        j_hits.Dispose();
        j_hitproperties.Dispose();
        j_collisionData.Dispose();
        j_disposed = true;
    }


    private void LateUpdate() {
        if (UseJobs) jStartCollisionJobs();
    }


    private void OnDestroy() {
        disposeJob();
    }
    // Update is called once per frame
    void Update()
    {
        if (UseJobs) {
            jUpdateCollisionsJobs();
        } else {
            UpdateCollisions();
        }
        UpdateUnstucks();
    }
    */

    void Update() {
        UpdateCollisions();
        UpdateUnstucks();
    }
}
