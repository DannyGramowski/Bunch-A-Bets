using System.Linq;
using TMPro;
using UnityEngine;

public class ChipPile : MonoBehaviour {
    [SerializeField] protected int amount = 1000;
    [SerializeField] bool spawn = false;
    [SerializeField] ChipPileTransport transportPrefab;
    [SerializeField] TextMeshProUGUI amountText;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() {
        
    }

    // Update is called once per frame
    void Update() {
        amountText.text = amount.ToString();

        if(Input.GetKeyDown(KeyCode.T) && spawn) {
            ChipPile other = FindObjectsOfType<ChipPile>().Where((ChipPile pile) => pile != this).First();

            if(other == null) {
                Debug.LogError("no other chip pile found");
            }

            SendTo(100, other);
        }
    }

    public void SendTo(int amount, ChipPile other) {
        if(amount > this.amount) {
            Debug.LogWarning($"{name}tried to send more chips than it had");
            return;
        }

        this.amount -= amount;
        ChipPileTransport transport = Instantiate(transportPrefab, transform.position, transform.rotation);
        transport.Init(amount, other);
    }

    public void Merge(ChipPileTransport pile) {
        amount += pile.amount;
        Destroy(pile.gameObject);
    }
}
