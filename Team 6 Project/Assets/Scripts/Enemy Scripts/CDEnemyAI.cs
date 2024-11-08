// CDEnemyAI.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class CDEnemyAI : MonoBehaviour, IDamage
{
    [SerializeField] Renderer model;
    [SerializeField] NavMeshAgent agent;

    [SerializeField] Transform headPosition;
    [SerializeField] int faceTargetSpeed;
    [SerializeField] int HP;
    [SerializeField] int attackDamage = 10;
    [SerializeField] float attackRate;

    private GameObject currentTargetCrop;
    private Color colourOriginal;
    private bool isAttacking;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        colourOriginal = model.material.color;
        GameManager.Instance.GameGoal(1);

        // Set initial target crop
        UpdateTargetCrop();
    }

    void Update()
    {
        if (currentTargetCrop == null)
        {
            UpdateTargetCrop();
        }

        if (currentTargetCrop != null)
        {
            Vector3 cropDirection = currentTargetCrop.transform.position - headPosition.position;
            agent.SetDestination(currentTargetCrop.transform.position);

            if (agent.remainingDistance <= agent.stoppingDistance)
            {
                faceTarget(cropDirection);
            }

            if (!isAttacking && Vector3.Distance(transform.position, currentTargetCrop.transform.position) <= agent.stoppingDistance)
            {
                StartCoroutine(Attack());
            }
        }
        else
        {
            agent.ResetPath(); // Stop moving if no target is available
        }
    }

    void UpdateTargetCrop()
    {
        currentTargetCrop = GameManager.Instance.GetNearestCrop(transform.position);
        if (currentTargetCrop != null)
        {
            //Debug.Log("New target crop assigned: " + currentTargetCrop.name);
        }
        else
        {
            //Debug.Log("No crops left to target.");
            // no More crops, you lose
            GameManager.Instance.YouLose();
        }
    }

    void faceTarget(Vector3 cropDirection)
    {
        Quaternion rot = Quaternion.LookRotation(cropDirection);
        transform.rotation = Quaternion.Lerp(transform.rotation, rot, Time.deltaTime * faceTargetSpeed);
    }

    public void TakeDamage(int amount)
    {
        HP -= amount;
        StartCoroutine(FlashRed());

        if (HP <= 0)
        {
            GameManager.Instance.GameGoal(-1);
            Destroy(gameObject);
        }
    }

    IEnumerator FlashRed()
    {
        model.material.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        model.material.color = colourOriginal;
    }

    IEnumerator Attack()
    {
        isAttacking = true;

        CropDamage cropDamage = currentTargetCrop.GetComponent<CropDamage>();
        if (cropDamage != null)
        {
            cropDamage.TakeDamage(attackDamage);
        }

        yield return new WaitForSeconds(attackRate);
        isAttacking = false;
    }
}
