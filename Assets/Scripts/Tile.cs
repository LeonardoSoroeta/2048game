using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Tile : MonoBehaviour
{
    public int value;
    public float speed;
    public float speedMultiplier;
    [SerializeField] Text valueText;

    Animator animator;

    bool hasMerged;

    Image image;

    public void UpdateTileValue(int newValue)
    {
        value = newValue;
        valueText.text = value.ToString();

        int colorIndex = GetColorIndex(value);
        image = GetComponent<Image>();
        image.color = TileStyleHolder.instance.tileStyles[colorIndex].tileColor;
        valueText.color = TileStyleHolder.instance.tileStyles[colorIndex].textColor;
    }

    int GetColorIndex(int value)
    {
        int index = 0;
        
        while (value != 1)
        {
            index++;
            value /= 2;
        }

        index--;

        return index;
    }

    public void Double()
    {
        UpdateTileValue(value * 2);
        ScoreTracker.instance.IncreaseScore(value);
        animator.SetTrigger("Merge");
    }

    public void PlaySpawnAnimation()
    {
        animator.SetTrigger("Spawn");
    }

    void Awake ()
    {
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        if (transform.localPosition != Vector3.zero)
        {
            hasMerged = false;
            transform.localPosition = Vector3.MoveTowards(transform.localPosition, Vector3.zero, speed * speedMultiplier * Time.deltaTime);
        }
        else
        {
            if (hasMerged == false)
            {
                // If parent has two childs and this is the second one, then destroy the first one
                if (transform.parent.GetChild(0) != this.transform)
                {
                    Destroy(transform.parent.GetChild(0).gameObject);
                }
                hasMerged = true;
            }
        }
    }

}
