using UnityEngine;

public class MBDeck : MonoBehaviour {
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] MBCard cardPrefab;
    [SerializeField] CardLayout layout;

    void Start() {
        
    }

    // Update is called once per frame
    void Update() {
        if(Input.GetKeyDown(KeyCode.Space)) {
            SendCard(layout);
        }
    }

    public void SendCard(CardLayout layout) {
        //draw a card and start an animation to slide it to the appropriate layout
        MBCard card = Instantiate(cardPrefab);
        layout.CardTo(card);
    } 
}
