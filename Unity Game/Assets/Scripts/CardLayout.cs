using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class CardLayout : MonoBehaviour {
    [SerializeField] List<MBCard> cards;
    [SerializeField] float cardDistance;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void CardTo(MBCard card) {
        Vector3 newCardPosition = new Vector3(cardDistance, 0, 0) * cards.Count;
        cards.Add(card);
        card.transform.parent = transform;
        card.MoveTo(newCardPosition);

    }
}
