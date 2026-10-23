using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Turn_Move : MonoBehaviour {
	[SerializeField] private float TurnX;
	[SerializeField] private float TurnY;
	[SerializeField] private float TurnZ;

	[SerializeField] private float MoveX;
	[SerializeField] private float MoveY;
	[SerializeField] private float MoveZ;

	[SerializeField] private bool World;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if (World == true) {
			transform.Rotate(TurnX * Time.deltaTime,TurnY * Time.deltaTime,TurnZ * Time.deltaTime, Space.World);
			transform.Translate(MoveX * Time.deltaTime, MoveY * Time.deltaTime, MoveZ * Time.deltaTime, Space.World);
		}else{
			transform.Rotate(TurnX * Time.deltaTime,TurnY * Time.deltaTime,TurnZ * Time.deltaTime, Space.Self);
			transform.Translate(MoveX * Time.deltaTime, MoveY * Time.deltaTime, MoveZ * Time.deltaTime, Space.Self);
		}
	}
}
