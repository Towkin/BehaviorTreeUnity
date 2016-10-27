using UnityEngine;
using System.Collections;

public class Football : MonoBehaviour, IDamagable {
    public void TakeDamage(DamageInfo aInfo) {
        if (GetComponent<Rigidbody>()) {
            GetComponent<Rigidbody>().AddForceAtPosition(aInfo.Force * aInfo.Direction, aInfo.Dealer.transform.position);
        }
    }
}