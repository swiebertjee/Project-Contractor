﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PingInter : MonoBehaviour {
	public bool active = true;

	public float distance = 20;
	public float speed = 50;

	private Transform player;
	private underwaterFX sonar;

	private Vector3 origin;
	public Vector3 getOrigin{get{ return origin; }}

	private float ping = 0;
	public float getPing{ get { return ping; } set { ping = value; } }

	private int maxPulses;
	private int pingHits = 0;
	private bool ACTIVATE = false;
	private bool pinging = false;

	private float dist = 0;

	void Awake () {
		// define origin on awake so data is available on start
		origin = transform.position;
	}

	void Start () {
		sonar = Camera.main.GetComponent<underwaterFX> ();
		player = GameObject.FindGameObjectWithTag ("Player").transform;
		maxPulses = sonar.SonarVals.maxPulses;
	}

	void FixedUpdate () {
		PingCheck ();
		PingUpdate ();
	}

	void PingCheck () {
//		if (pinged)
//			return;

		if (!active)
			return;
		
		dist = Vector3.Distance (origin, player.position);
		for (int i = 0; i < maxPulses; i++) {
			if (dist - 1 < sonar.aPulse [i] && dist + 1 > sonar.aPulse [i]) {
				if (!ACTIVATE) 
					ping = 0;

				ACTIVATE = true;
			}
		}
	}

	void PingUpdate () {
		if (ACTIVATE) {
			if (!active)
				return;
			
			ping += Time.deltaTime * speed;
			if (ping > distance) {
				ACTIVATE = false;
				ping = 0;
			}
		}
	}



	void P () {

		if (pinging) {
			if (!active)
				return;

			ping += Time.deltaTime * speed;
			if (ping > distance) {
				pinging = false;
				ping = 0;
			}

		}
	}



	void C () {

		if (!active)
			return;

		dist = Vector3.Distance (origin, player.position);

		for (int i = 0; i < maxPulses; i++) {
			
			if (dist < sonar.aPulse [i]) {
				
				ACTIVATE = true;
			}
		}

		if (ACTIVATE)
			pinging = true;

		ACTIVATE = false;

	}




}