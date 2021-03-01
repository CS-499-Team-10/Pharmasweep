﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class DrugTile : MonoBehaviour {
	// [SerializeField] private GameObject cardBack;
	// [SerializeField] public TextMesh nameLabel;
	public TMP_Text nameLabelTMP;

	[SerializeField] public List<string> drugMatches;
	[SerializeField] public SceneController controller;

	private int _id;
	public int id {
		get {return _id;}
	}
	
	// void Start(){
	// 	nameLabelTMP = GetComponent<TMP_Text>();
	// }

	public void SetCard(int id, Sprite image) {
		_id = id;
		GetComponent<SpriteRenderer>().sprite = image;
		// controller = 
	}

	public void Tapped() {
		Debug.Log("clicked " + nameLabelTMP);
		controller.CardRevealed(this);
	}

	public void Unreveal() {
		// cardBack.SetActive(true);
	}
}
