using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class EnemyMovement : MonoBehaviour {

	[SerializeField]
	Transform playerInputSpace = default;

	[SerializeField, Range(0f, 100f)]
	float maxSpeed = 10f;

	[SerializeField, Range(0f, 100f)]
	float maxAcceleration = 10f, maxAirAcceleration = 1f;

	[SerializeField, Range(0f, 10f)]
	float jumpHeight = 2f;

	[SerializeField, Range(0, 5)]
	int maxAirJumps = 0;

	[SerializeField, Range(0, 90)]
	float maxGroundAngle = 25f, maxStairsAngle = 50f;

	[SerializeField, Range(0f, 100f)]
	float maxSnapSpeed = 100f;

	[SerializeField, Min(0f)]
	float probeDistance = 1f;

	[SerializeField]
	LayerMask probeMask = -1, stairsMask = -1;

	Rigidbody body;

	Vector3 velocity, desiredVelocity;

	Vector3 upAxis, rightAxis, forwardAxis;

	bool desiredJump;

	Vector3 contactNormal, steepNormal;

    Vector2 playerInput = new Vector2();

    int groundContactCount, steepContactCount;

	bool OnGround => groundContactCount > 0;

	bool OnSteep => steepContactCount > 0;

	int jumpPhase;

	float minGroundDotProduct, minStairsDotProduct;

	int stepsSinceLastGrounded, stepsSinceLastJump;

	private bool needsStraightened = false;

	public Transform playerTransform;
	public float enemySpeed = 1.0f;
	public float hitCount = 0;
	public float hitCapacity = 4;

	void OnValidate () {
		minGroundDotProduct = Mathf.Cos(maxGroundAngle * Mathf.Deg2Rad);
		minStairsDotProduct = Mathf.Cos(maxStairsAngle * Mathf.Deg2Rad);
	}

	void Awake () {
		body = GetComponent<Rigidbody>();
		body.useGravity = false;
		
		// playerInputSpace = GameLoop.Instance.orbitCamera.transform;
		// playerTransform = GameLoop.Instance.characterTransform;

		playerInputSpace = GameObject.Find("Orbit Camera").transform;
		playerTransform = GameObject.Find("Character").transform;
		
		OnValidate();
	}

    public void OnMovement(InputValue input)
    {
        playerInput = (Vector2)input.Get();
    }

    public void OnJump(InputValue input)
    {
        if (input.isPressed)
            desiredJump = true;
    }
    
	void Update () {
		
		
        // transform.gameObject.GetComponent<Rigidbody>().velocity += (directionToPlayer * enemySpeed * Time.deltaTime);
		StandUpStraight();

        // Handled on OnJump
        // desiredJump |= Input.GetButtonDown("Jump");
    }

	void FixedUpdate () {
		Vector3 gravity = CustomGravity.GetGravity(body.position, out upAxis);
		UpdateState();
		AdjustVelocity();

		if (desiredJump) {
			desiredJump = false;
			Jump(gravity);
		}

		velocity += gravity * Time.deltaTime;

		body.velocity = velocity;
		ClearState();
		
		if (Math.Abs(velocity.x) < .2)
		{
			needsStraightened = false;
		}
		else
		{
			needsStraightened = true;
		}


        var directionToPlayer = playerTransform.position - transform.position;
        Debug.DrawRay(transform.position, directionToPlayer.normalized);

        if (playerInputSpace)
        {
            rightAxis = ProjectDirectionOnPlane(playerInputSpace.right, upAxis);
            forwardAxis =
                ProjectDirectionOnPlane(playerInputSpace.forward, upAxis);
        }
        else
        {
            rightAxis = ProjectDirectionOnPlane(Vector3.right, upAxis);
            forwardAxis = ProjectDirectionOnPlane(Vector3.forward, upAxis);
        }

        // desiredVelocity =  new Vector3(directionToPlayer.x, directionToPlayer.y, directionToPlayer.y) * maxSpeed;
        transform.position += (directionToPlayer * enemySpeed * Time.deltaTime);

        // playerInput = new Vector2();
    }

	void ClearState () {
		groundContactCount = steepContactCount = 0;
		contactNormal = steepNormal = Vector3.zero;
	}

	void UpdateState () {
		stepsSinceLastGrounded += 1;
		stepsSinceLastJump += 1;
		velocity = body.velocity;
		if (OnGround || SnapToGround() || CheckSteepContacts()) {
			stepsSinceLastGrounded = 0;
			if (stepsSinceLastJump > 1) {
				jumpPhase = 0;
			}
			if (groundContactCount > 1) {
				contactNormal.Normalize();
			}
		}
		else {
			contactNormal = upAxis;
		}
	}
	
	/*
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        //Gizmos.DrawLine(transform.position, targetDirection * 50);
        Gizmos.DrawLine(transform.position, contactNormal);
        //Gizmos.DrawLine(transform.position, playerInput);
        //Vector3 yRotation = contactNormal * playerInput.y;

        Gizmos.color = Color.green;
        Vector3 yRotation = transform.position + velocity;
        Gizmos.DrawLine(transform.position, yRotation);
        //Gizmos.DrawLine(transform.position, velocity);
        //Gizmos.DrawLine(transform.position, transform.position + velocity);
        // Gizmos.DrawLine(new Vector3(0, 0, 0), transform.position);
    }
    */

    Vector3 myContactNormal;

    private void StandUpStraight()
	{
		Vector3 moveDirection = new Vector3(playerInput.x, 0, playerInput.y);

        var targetDirection = contactNormal;
        
        Vector3 yRotation = transform.position + velocity;
        yRotation += contactNormal;
        
        transform.forward = yRotation.normalized;
        //transform.up = contactNormal;
        

        var velocityDirection = Quaternion.LookRotation(velocity);
		var velocityVector = velocityDirection.eulerAngles;
	}

	bool SnapToGround () {
		if (stepsSinceLastGrounded > 1 || stepsSinceLastJump <= 2) {
			return false;
		}
		float speed = velocity.magnitude;
		if (speed > maxSnapSpeed) {
			return false;
		}
		if (!Physics.Raycast(
			body.position, -upAxis, out RaycastHit hit,
			probeDistance, probeMask
		)) {
			return false;
		}

		float upDot = Vector3.Dot(upAxis, hit.normal);
		if (upDot < GetMinDot(hit.collider.gameObject.layer)) {
			return false;
		}

		groundContactCount = 1;
		contactNormal = hit.normal;
		float dot = Vector3.Dot(velocity, hit.normal);
		if (dot > 0f) {
			velocity = (velocity - hit.normal * dot).normalized * speed;
		}
		return true;
	}

	bool CheckSteepContacts () {
		if (steepContactCount > 1) {
			steepNormal.Normalize();
			float upDot = Vector3.Dot(upAxis, steepNormal);
			if (upDot >= minGroundDotProduct) {
				steepContactCount = 0;
				groundContactCount = 1;
				contactNormal = steepNormal;
				return true;
			}
		}
		return false;
	}

	void AdjustVelocity () {
		Vector3 xAxis = ProjectDirectionOnPlane(rightAxis, contactNormal);
		Vector3 zAxis = ProjectDirectionOnPlane(forwardAxis, contactNormal);

		float currentX = Vector3.Dot(velocity, xAxis);
		float currentZ = Vector3.Dot(velocity, zAxis);

		float acceleration = OnGround ? maxAcceleration : maxAirAcceleration;
		float maxSpeedChange = acceleration * Time.deltaTime;

		float newX =
			Mathf.MoveTowards(currentX, desiredVelocity.x, maxSpeedChange);
		float newZ =
			Mathf.MoveTowards(currentZ, desiredVelocity.z, maxSpeedChange);

		velocity += xAxis * (newX - currentX) + zAxis * (newZ - currentZ);
        
        if (needsStraightened) StandUpStraight();
	}

	void Jump (Vector3 gravity) {
		Vector3 jumpDirection;
		if (OnGround) {
			jumpDirection = contactNormal;
		}
		else if (OnSteep) {
			jumpDirection = steepNormal;
			jumpPhase = 0;
		}
		else if (maxAirJumps > 0 && jumpPhase <= maxAirJumps) {
			if (jumpPhase == 0) {
				jumpPhase = 1;
			}
			jumpDirection = contactNormal;
		}
		else {
			return;
		}

		stepsSinceLastJump = 0;
		jumpPhase += 1;
		float jumpSpeed = Mathf.Sqrt(2f * gravity.magnitude * jumpHeight);
		jumpDirection = (jumpDirection + upAxis).normalized;
		float alignedSpeed = Vector3.Dot(velocity, jumpDirection);
		if (alignedSpeed > 0f) {
			jumpSpeed = Mathf.Max(jumpSpeed - alignedSpeed, 0f);
		}
		velocity += jumpDirection * jumpSpeed;
	}

	void OnCollisionEnter (Collision collision) {
		if (collision.gameObject.CompareTag("Bullet"))
		{
			OnHitByBullet();
		}
		EvaluateCollision(collision);
	}

	void OnCollisionStay (Collision collision) {
		EvaluateCollision(collision);
	}

	void EvaluateCollision (Collision collision) {
		float minDot = GetMinDot(collision.gameObject.layer);
		for (int i = 0; i < collision.contactCount; i++) {
			Vector3 normal = collision.GetContact(i).normal;
			float upDot = Vector3.Dot(upAxis, normal);
			if (upDot >= minDot) {
				groundContactCount += 1;
				contactNormal += normal;
			}
			else if (upDot > -0.01f) {
				steepContactCount += 1;
				steepNormal += normal;
			}
		}
	}

	void OnHitByBullet()
	{
		hitCount += 1;
		if (hitCount >= hitCapacity)
		{
			GameObject.Find("Game Loop").GetComponent<GameLoop>().AddTowel();
			Die();
		}
	}

	void Die()
	{
		Destroy(gameObject);
	}

	Vector3 ProjectDirectionOnPlane (Vector3 direction, Vector3 normal) {
		return (direction - normal * Vector3.Dot(direction, normal)).normalized;
	}

	float GetMinDot (int layer) {
		return (stairsMask & (1 << layer)) == 0 ?
			minGroundDotProduct : minStairsDotProduct;
	}
}
