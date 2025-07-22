using Server;
using UnityEngine;

public class MBDeck : MonoBehaviour {
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] MBCard cardPrefab;
    [SerializeField] CardLayout centerLayout;


    void Start() {
        
    }

    // Update is called once per frame
    void Update() {
        // if(Input.GetKeyDown(KeyCode.Space)) {
        //     SendCard(centerLayout);
        // }
    }

    public void SendCard(CardLayout layout, Card card) {
        //draw a card and start an animation to slide it to the appropriate layout
        MBCard mbcard = Instantiate(cardPrefab);
        mbcard.SetCard(card);
        layout.CardTo(mbcard);

    } 
}
