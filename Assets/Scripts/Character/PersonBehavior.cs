﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum PersonMoveMode { Path, Wander }

public class PersonBehavior : MonoBehaviour
{
	System.Action<PersonBehavior> endPathCallback;
	NavMeshAgent agent;
	PersonPath path;
	PersonMoveMode moveMode;
	bool active;

	int curNode;
	Transform myTransform;
	float start;

	void Awake ()
	{
		agent = GetComponent<NavMeshAgent> ();
		myTransform = GetComponent<Transform> ();
	}

	void LateUpdate ()
	{
		if ( !active )
			return;

		if ( moveMode == PersonMoveMode.Path )
		{
			if ( agent.hasPath && agent.remainingDistance <= agent.stoppingDistance )
			{
				if ( curNode < path.points.Length - 1 )
				{
					curNode++;
					agent.SetDestination ( path.points [ curNode ].position );

				} else
				{
					agent.Stop ();
					active = false;
					if ( endPathCallback != null )
						endPathCallback ( this );
				}
			}
		} else
		{
			Vector3 euler = transform.eulerAngles;
			if ( Time.timeScale != 0 )
				euler.y += 0.25f - Mathf.PerlinNoise ( start + Time.time, start + Time.time ) / 2;

			NavMeshHit navHit;
			float rayDist = 2f;
			bool didHit = agent.Raycast ( myTransform.position + myTransform.forward * rayDist, out navHit );
			if ( didHit )
			{
				Vector3 normal = new Vector3 ( navHit.normal.x, 0, navHit.normal.z ).normalized;
				Debug.DrawRay ( myTransform.position + Vector3.up, navHit.normal, Color.red );
				Vector3 targetEuler = Quaternion.LookRotation ( normal, Vector3.up ).eulerAngles;
				euler.y = Mathf.Lerp ( euler.y, targetEuler.y, 1f - navHit.distance / rayDist );
			}

			myTransform.eulerAngles = euler;
			agent.Move ( myTransform.forward * agent.speed * Time.deltaTime );
		}
	}

	public void UsePath (PersonPath _path, System.Action<PersonBehavior> callback)
	{
		endPathCallback = callback;
		curNode = 1;
		moveMode = PersonMoveMode.Path;
		path = _path;
		agent.SetDestination ( path.points [ 1 ].position );
		active = true;
	}

	public void Wander ()
	{
		agent.autoBraking = false;

		start = Random.value * 1000f;
		float y = Random.value * 360f;
		Vector3 euler = myTransform.eulerAngles;
		euler.y = y;
		myTransform.eulerAngles = euler;

		moveMode = PersonMoveMode.Wander;
		active = true;
	}
}