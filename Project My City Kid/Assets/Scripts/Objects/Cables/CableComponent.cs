using UnityEngine;
using System;


public class CableComponent : MonoBehaviour
{
	#region Class members

	[SerializeField, Tooltip("The starting point of the cable, typically attached to an object.")]
	Transform startPoint;

	[SerializeField, Tooltip("The ending point of the cable, typically attached to another object.")]
	Transform endPoint;

	[SerializeField, Tooltip("The material used for rendering the cable.")]
	Material cableMaterial;

	// Cable config
	[SerializeField, Tooltip("The total length of the cable.")]
	float cableLength = 0.5f;

	[SerializeField, Tooltip("The total number of segments in the cable. Each segment is a portion of the cable.")]
	int totalSegments = 5;

	[SerializeField, Tooltip("Segments per unit length of the cable. Determines the granularity of the segments along the cable.")]
	float segmentsPerUnit = 2f;

	protected int segments = 0;

	[SerializeField, Tooltip("The thickness of the cable.")]
	float cableWidth = 0.1f;

	// Solver config
	[SerializeField, Tooltip("The number of iterations for the Verlet integration step. Affects how accurately the cable simulation is calculated.")]
	int verletIterations = 1;

	[SerializeField, Tooltip("The number of iterations for the constraint solver. More iterations improve accuracy but reduce performance.")]
	int solverIterations = 1;

	[SerializeField, Tooltip("Defines the stiffness of the cable. A higher value means the cable will resist stretching more.")]
	float stiffness = 1f;

	protected LineRenderer line;
	protected CableParticle[] points;

	#endregion


	#region Initial setup

	void Start()
	{
		InitCableParticles();
		InitLineRenderer();
	}

	/**
	 * Init cable particles
	 * 
	 * Creates the cable particles along the cable length
	 * and binds the start and end tips to their respective game objects.
	 */
	void InitCableParticles()
	{
		// Calculate segments to use
		if (totalSegments > 0)
			segments = totalSegments;
		else
			segments = Mathf.CeilToInt (cableLength * segmentsPerUnit);

		Vector3 cableDirection = (endPoint.position - startPoint.position).normalized;
		float initialSegmentLength = cableLength / segments;
		points = new CableParticle[segments + 1];

		// Foreach point
		for (int pointIdx = 0; pointIdx <= segments; pointIdx++) {
			// Initial position
			Vector3 initialPosition = transform.position + (cableDirection * (initialSegmentLength * pointIdx));
			points[pointIdx] = new CableParticle(initialPosition);
		}

		// Bind start and end particles with their respective gameobjects
		CableParticle start = points[0];
		CableParticle end = points[segments];
		start.Bind(startPoint.transform);
		end.Bind(endPoint.transform);
	}

	/**
	 * Initialized the line renderer
	 */
	void InitLineRenderer()
	{
		line = gameObject.AddComponent<LineRenderer>();

		// Anstelle von SetWidth:
		line.startWidth = cableWidth;  // Setzt die Breite am Anfang des Liners
		line.endWidth = cableWidth;    // Setzt die Breite am Ende des Liners

		// Anstelle von SetVertexCount:
		line.positionCount = segments + 1;  // Legt die Anzahl der Segmente fest

		line.material = cableMaterial;
		line.GetComponent<Renderer>().enabled = true;
	}

	#endregion


	#region Render Pass

	void Update()
	{
		RenderCable();
	}

	/**
	 * Render Cable
	 * 
	 * Update every particle position in the line renderer.
	 */
	void RenderCable()
	{
		for (int pointIdx = 0; pointIdx < segments + 1; pointIdx++) 
		{
			line.SetPosition(pointIdx, points [pointIdx].Position);
		}
	}

	#endregion


	#region Verlet integration & solver pass

	void FixedUpdate()
	{
		for (int verletIdx = 0; verletIdx < verletIterations; verletIdx++) 
		{
			VerletIntegrate();
			SolveConstraints();
		}
	}

	/**
	 * Verler integration pass
	 * 
	 * In this step every particle updates its position and speed.
	 */
	void VerletIntegrate()
	{
		Vector3 gravityDisplacement = Time.fixedDeltaTime * Time.fixedDeltaTime * Physics.gravity;
		foreach (CableParticle particle in points) 
		{
			particle.UpdateVerlet(gravityDisplacement);
		}
	}

	/**
	 * Constrains solver pass
	 * 
	 * In this step every constraint is addressed in sequence
	 */
	void SolveConstraints()
	{
		// For each solver iteration..
		for (int iterationIdx = 0; iterationIdx < solverIterations; iterationIdx++) 
		{
			SolveDistanceConstraint();
			SolveStiffnessConstraint();
		}
	}

	#endregion


	#region Solver Constraints

	/**
	 * Distance constraint for each segment / pair of particles
	 **/
	void SolveDistanceConstraint()
	{
		float segmentLength = cableLength / segments;
		for (int SegIdx = 0; SegIdx < segments; SegIdx++) 
		{
			CableParticle particleA = points[SegIdx];
			CableParticle particleB = points[SegIdx + 1];

			// Solve for this pair of particles
			SolveDistanceConstraint(particleA, particleB, segmentLength);
		}
	}
		
	/**
	 * Distance Constraint 
	 * 
	 * This is the main constrains that keeps the cable particles "tied" together.
	 */
	void SolveDistanceConstraint(CableParticle particleA, CableParticle particleB, float segmentLength)
	{
		// Find current vector between particles
		Vector3 delta = particleB.Position - particleA.Position;
		// 
		float currentDistance = delta.magnitude;
		float errorFactor = (currentDistance - segmentLength) / currentDistance;
		
		// Only move free particles to satisfy constraints
		if (particleA.IsFree() && particleB.IsFree()) 
		{
			particleA.Position += errorFactor * 0.5f * delta;
			particleB.Position -= errorFactor * 0.5f * delta;
		} 
		else if (particleA.IsFree()) 
		{
			particleA.Position += errorFactor * delta;
		} 
		else if (particleB.IsFree()) 
		{
			particleB.Position -= errorFactor * delta;
		}
	}

	/**
	 * Stiffness constraint
	 **/
	void SolveStiffnessConstraint()
	{
		float distance = (points[0].Position - points[segments].Position).magnitude;
		if (distance > cableLength) 
		{
			foreach (CableParticle particle in points) 
			{
				SolveStiffnessConstraint(particle, distance);
			}
		}	
	}

	/**
	 * TODO: I'll implement this constraint to reinforce cable stiffness 
	 * 
	 * As the system has more particles, the verlet integration aproach 
	 * may get way too loose cable simulation. This constraint is intended 
	 * to reinforce the cable stiffness.
	 * // throw new System.NotImplementedException ();
	 **/
	void SolveStiffnessConstraint(CableParticle cableParticle, float distance)
	{
	}

	#endregion
}
