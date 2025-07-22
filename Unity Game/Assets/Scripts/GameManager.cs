using UnityEngine;

public class GameManager : MonoBehaviour {
    public static GameManager Manager{ get {
        if(manager == null) {
            manager = FindObjectOfType<GameManager>();
        }
        return manager;
    }}
    private static GameManager manager = null;


    [SerializeField] MBPlayer[] players;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Program.Main();
    }

    // Update is called once per frame
    void Update() {
        
    }
}
