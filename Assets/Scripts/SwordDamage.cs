using UnityEngine;
using System.Collections.Generic;

public class SwordDamage : MonoBehaviour
{
    [Header("Damage Settings")]
    [SerializeField] private float damage = 35;
    private bool canDealDamage = false;

    private enemyHealth enemyHealthScript;

    private HashSet<GameObject> damagedObjects = new HashSet<GameObject>();
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void EnableDamage()
    {
        canDealDamage = true;
        damagedObjects.Clear();
        Debug.Log("EnableDamage");
    }

    public void DisableDamage()
    {
        canDealDamage = false;
        damagedObjects.Clear();
        Debug.Log("DisableDamage");
    }

    // Update is called once per frame
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Тригер работает");
        if (!canDealDamage)
            return;

        Debug.Log("Он поймал коллайдер");

        if(other.transform.root == transform.root)
        {
            Debug.Log("это сам игрок");
            return;
        }

        Debug.Log("Он сделал проверку root");
        if (other.TryGetComponent<enemyHealth>(out enemyHealth enemyHealthComponent))
        {
            Debug.Log("Он поймал тег именно врага!");
            {
                if(!damagedObjects.Contains(other.gameObject))
                {
                    damagedObjects.Add(other.gameObject);

                    enemyHealthComponent.TakeDamage(damage);
                }
            }
            
        }
    }
}
