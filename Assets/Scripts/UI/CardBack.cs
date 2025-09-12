using UnityEngine;

public class CardBack : MonoBehaviour
{
    public GameObject cardBack;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (CardDisplay.cardTurnedBack == false)
        {
            cardBack.SetActive(false);
        }
        else
        {
            cardBack.SetActive(true);
        }
    }
}
