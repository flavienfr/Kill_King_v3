using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public Animator animator;

    public int maxHealth = 100;
    private int currentHealth;

    void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int Damage)
    {
        currentHealth -= Damage;

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        //animator.SetBool("IsDead", true);
        animator.SetTrigger("IsDead");

        GetComponent<Collider2D>().enabled = false;
        this.enabled = false;
    }
}
