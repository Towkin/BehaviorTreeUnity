using UnityEngine;
using System.Collections;


public class HealthObject : MonoBehaviour, IInteractable {
    public void Interact(SurvivalPawn aUser) {
        aUser.TakeDamage(new DamageInfo(-10f));

        Destroy(gameObject);
    }
}
