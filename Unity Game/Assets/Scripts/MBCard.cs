using UnityEngine;

public class MBCard : MonoBehaviour
{
    [SerializeField] float moveTime = 1f;

    private Vector3 newLocalPosition;
    private float speed;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake() {
        newLocalPosition = transform.position;
    }
    void Start() {
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
}
