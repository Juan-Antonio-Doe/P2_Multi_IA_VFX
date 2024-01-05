using Nrjwolf.Tools.AttachAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour {

	[field: Header("Autoattach properties")]
	[field: SerializeField, FindObjectOfType, ReadOnlyField] private EnemiesManager enemyManager { get; set; }
}