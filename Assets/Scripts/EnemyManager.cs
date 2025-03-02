using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
	// Start is called once before the first execution of Update after the MonoBehaviour is created
	public Player player;
	public bool isMoldy;
	public Color MoldColor = new Color(0.75f, 0.75f, 0.75f, 1);
	public float moldTime;
	public DateTime lastMold;
	public float moldCooldown;

	List<EnemyAI> enemies;

	EnemySpawner spawner;


	void Start()
	{
		spawner = GetComponent<EnemySpawner>();
		spawner.SpawnAllObjects();
		enemies = GetComponentsInChildren<EnemyAI>().ToList();
		lastMold = DateTime.Now.AddSeconds(moldTime * -1);
	}

	// Update is called once per frame
	void Update()
	{
		if (isMoldy && (lastMold.AddSeconds(moldTime) <= DateTime.Now))
		{
			isMoldy = false;
			unmold();
		}
	
	



	}

	public void Mold()
	{
		
		if (lastMold.AddSeconds(moldCooldown) <= DateTime.Now)
		{
			player.canTakeDamage = false;
			foreach (EnemyAI enemy in enemies)
			{
				enemy.canTakeDamage = false;	
				enemy.changeStates(MoldColor);
			}
		}
	}
		

	public void unmold()
	{
		player.canTakeDamage = true;
		foreach (EnemyAI enemy in enemies)
		{
			enemy.changeStates(Color.white);
			enemy.canTakeDamage = true;
		}
	}

	

}
