﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class DrugTile : MonoBehaviour
{
    [SerializeField] private TMP_Text nameLabelTMP; // label containing drug name
    [SerializeField] private ParticleSystem matchParticles;
    public string drugName
    {
        get { return nameLabelTMP.text; }
        set { nameLabelTMP.text = value; }
    }

    [SerializeField] public List<string> drugMatches; // list of drugs that this drug matches with
    [SerializeField] public SceneController controller; // controller managing gameplay

    public bool markedToDestroy = false; // when set, shrink this tile then destroy it

    // speeds to move and scale tile
    private const float moveSpeed = 2500;
    private const float scaleSpeed = 10;

    private void Update()
    {
        // tiles are spawned at scale 0 and grow to 1
        if (!markedToDestroy && (transform.localScale != Vector3.one))
        {
            transform.localScale = Vector3.MoveTowards(transform.localScale, Vector3.one, scaleSpeed * Time.deltaTime);
        }

        // when a tile is matched, shrink it
        if (markedToDestroy && (transform.localScale != Vector3.zero))
        {
            transform.localScale = Vector3.MoveTowards(transform.localScale, Vector3.zero, scaleSpeed * Time.deltaTime);
        }

        // destroy it when its scale hits 0
        if (markedToDestroy && (transform.localScale == Vector3.zero))
        {
            GameObject.Destroy(this.gameObject);
        }
    }

    private void OnEnable()
    {
        transform.localPosition = Vector3.zero;
    }

    // wrapper for MoveTowards to use as a coroutine
    // edited from https://stackoverflow.com/a/51166030
    IEnumerator MoveTowards(Transform objectToMove, Vector3 toPosition, float duration)
    {
        float counter = 0;

        while (counter < duration)
        {
            counter += Time.deltaTime;
            Vector3 currentPos = objectToMove.localPosition;

            float time = Vector3.Distance(currentPos, toPosition) / (duration - counter) * Time.deltaTime;

            objectToMove.localPosition = Vector3.MoveTowards(currentPos, toPosition, time);

            // Debug.Log(counter + " / " + duration);
            yield return null;
        }
        objectToMove.localPosition = Vector3.zero;
    }

    public void Tapped()
    {
        controller.CardTapped(this); // alert controller
    }

    // returns true if this drug matches another drugtile
    public bool CheckMatch(DrugTile other)
    {
        if (drugMatches.Contains(other.nameLabelTMP.text) && nameLabelTMP.text != other.nameLabelTMP.text)
        {
            return true;
        }
        else return false;
    }

    public void Select()
    {
        GetComponent<Image>().color = new Color(170F / 255F, 170F / 255F, 100F / 255F);
    }

    public void UnSelect()
    {
        controller.SetColor(this);
    }

    IEnumerator Wait(RectTransform newCell)
    {
        controller.MatchSFX.Play();
        yield return new WaitForEndOfFrame();
        this.controller.tilesOnScreen.Remove(this); //remove this card from the list of active cards
        markedToDestroy = true;
        yield return 0;
    }

    private void ChangeParent(RectTransform newParentCell)
    {
        // transform.parent = newParentCell;
        transform.SetParent(newParentCell);
        StartCoroutine(MoveTowards(transform, Vector3.zero, SlideController.SLIDE_TIME));
    }

    // slides a tile in given SlideController.Direction
    // returns true if the tile slides into a match, false otherwise
    public bool Slide(SlideController.Direction dir)
    {
        GridPosition myPos = transform.parent.gameObject.GetComponent<GridPosition>();
        GridPosition newPos = myPos.GetNext(dir);
        if (newPos)
        {
            RectTransform newCell = newPos.GetComponent<RectTransform>();
            if (newCell.childCount < 1) // if there isn't a tile in the next cell
            {
                ChangeParent(newCell);
            }
            else if (newCell.GetComponentInChildren<DrugTile>() && CheckMatch(newCell.GetComponentInChildren<DrugTile>())) // if there is a tile and they match
            {
                Instantiate(matchParticles, newCell.position - new Vector3(0, 0, 20), Quaternion.identity);
                ChangeParent(newCell);
                foreach (Transform child in newCell)
                {
                    controller.tilesOnScreen.Remove(child.GetComponent<DrugTile>()); //remove this tile from the list of active tiles
                    child.GetComponent<DrugTile>().markedToDestroy = true;
                }
                StartCoroutine(Wait(newCell));
                return true;
            }
        }
        return false;
    }

    // returns true if the tile can slide in the given direction, false otherwise
    public bool CanSlide(SlideController.Direction dir)
    {
        GridPosition myPos = transform.parent.gameObject.GetComponent<GridPosition>();
        GridPosition newPos = myPos.GetNext(dir);
        if (newPos)
        {
            RectTransform newCell = newPos.GetComponent<RectTransform>();
            if (newCell.childCount < 1) // if there isn't a tile in the next cell
            {
                return true;
            }
            else if (newCell.GetComponentInChildren<DrugTile>() && CheckMatch(newCell.GetComponentInChildren<DrugTile>())) // if there is a tile and they match
            {
                return true;
            }
        }
        return false;
    }

    public void GenerateMatchParticles()
    {
        Instantiate(matchParticles, transform.position - new Vector3(0, 0, 20), Quaternion.identity);
    }
}
