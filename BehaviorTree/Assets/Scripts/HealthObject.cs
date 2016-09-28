using UnityEngine;
using System.Collections;


public class HealthObject : MonoBehaviour, Interactable {
    public void Interact(SurvivalPawn aUser) {
        aUser.TakeDamage(new DamageInfo(-10f));

        Destroy(gameObject);
    }
}
