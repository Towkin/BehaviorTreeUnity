using UnityEngine;
using System.Collections;

public class Football : MonoBehaviour, IDamagable {

    Vector3 mStartPosition;

    [SerializeField]
    int mRedScore = 0;
    [SerializeField]
    int mBlueScore = 0;

    [SerializeField]
    GameObject mBlueGoal;

    [SerializeField]
    GameObject mRedGoal;

    void Start() {
        mStartPosition = transform.position;
    }

    void OnTriggerEnter(Collider other) {
        if(other.gameObject == mBlueGoal) {
            mRedScore++;

            transform.position = mStartPosition;
            GetComponent<Rigidbody>().velocity = Vector3.zero;
        } else if(other.gameObject == mRedGoal) {
            mBlueScore++;

            transform.position = mStartPosition;
            GetComponent<Rigidbody>().velocity = Vector3.zero;
        }
    }
    
    public void TakeDamage(DamageInfo aInfo) {
        if (GetComponent<Rigidbody>()) {
            GetComponent<Rigidbody>().AddForceAtPosition(aInfo.Force * aInfo.Direction, aInfo.Dealer.transform.position);
        }
    }

    void OnGUI() {
        Vector2 RectSize = new Vector2(100, 20);

        GUI.Box(new Rect(new Vector2(Screen.width / 2 - RectSize.x / 2, 0), RectSize), "Blue: " + mBlueScore.ToString() + " - Red: " + mRedScore.ToString());
    }
}