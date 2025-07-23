using Unity.VisualScripting;
using UnityEngine;

public class ChipPileTransport : ChipPile {
    [SerializeField] float time = 0.4f;
    [SerializeField] ChipPile destinationPile;

    private float speed;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() {
        speed = Vector3.Distance(transform.position, destinationPile.transform.position) / time;
    }

    // Update is called once per frame
    void Update() {
        transform.position = Vector3.MoveTowards(transform.position, destinationPile.transform.position, speed * Time.deltaTime);

    }

    void OnTriggerEnter(Collider collider) {
        ChipPile pile = collider.GetComponent<ChipPile>(); 
        if(pile != null && pile == destinationPile) {
            pile.Merge(this);
        }
    }

    public void Init(int amount, ChipPile destination) {
        destinationPile = destination;
        this.amount = amount;
    }



    // public void SendTo(int amount, ChipPile other) {
    //     if(amount > this.amount) {
    //         Debug.LogWarning($"{name}tried to send more chips than it had");
    //         return;
    //     }
        
    //     this.amount -= amount;
    // }

    // public void Merge(ChipPile pile) {

    // }
}
