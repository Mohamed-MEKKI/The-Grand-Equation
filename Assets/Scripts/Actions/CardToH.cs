using NUnit.Framework;
using UnityEngine;

public class CardToHand : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    GameObject Hand;
    GameObject HandCard;

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        Hand = GameObject.Find("HandPanel");
        HandCard.transform.SetParent(Hand.transform);
        HandCard.transform.localScale = Vector3.one;
        HandCard.transform.localPosition = new Vector3(transform.position.x, transform.position.y, -40);
        HandCard.transform.eulerAngles = new Vector3(25, 0, 0);

    }
}
