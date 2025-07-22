using Server;
using TMPro;
using UnityEngine;

public class MBCard : MonoBehaviour
{
    [SerializeField] float moveTime = 1f;
    [SerializeField] TextMeshProUGUI text;

    private Vector3 newLocalPosition;
    private float speed;
    private Card card;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() {
        newLocalPosition = transform.position;
        text = GetComponentInChildren<TextMeshProUGUI>();
    }

    // Update is called once per frame
    void Update() {
        if(Vector3.Distance(transform.localPosition, newLocalPosition) > 0.001f) {
            transform.localPosition = Vector3.MoveTowards(transform.localPosition, newLocalPosition, speed * Time.deltaTime);
        }
    }

    public void MoveTo(Vector3 newLocalPosition) {
        this.newLocalPosition = newLocalPosition;
        this.speed = Vector3.Distance(transform.localPosition, newLocalPosition) / moveTime;
    }
    
    public void SetCard(Card card) {
        this.card = card;
        text.text = card.Suit.ToString() + card.Value.ToString();
    }
}
