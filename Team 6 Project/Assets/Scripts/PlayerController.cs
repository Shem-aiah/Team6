using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;

public class PlayerController : MonoBehaviour, IDamage, IHealth
{
    [Header("----- Components -----")]
    [SerializeField] LayerMask ignoreMask;
    [SerializeField] CharacterController pController;
    [Header("----- Player Stats -----")]
    [SerializeField] int HP;
    [SerializeField] int speed;
    int HPOriginal;
    bool isSprinting;
    [SerializeField] int sprint;
    private int jumpCount;
    [SerializeField] int jumpMax;
    [SerializeField] int jumpSpeed;
    [SerializeField] int gravity;
    [SerializeField] int interactionRange;
    [Header("----- Power Ups -----")]
    public List<ActivePowerup> activePowerups = new List<ActivePowerup>();
    [Header("----- Gun Stats -----")]
    bool isShooting;
    [SerializeField] int shootDamage;
    [SerializeField] float shootRate;
    [SerializeField] int shootDistance;
    [SerializeField] Animator crossbowAnimator;
    [SerializeField] GameObject crossbowPowerupParticles;
    [SerializeField] GameObject boostHitEffect;
    [SerializeField] GameObject arrowHitEffect;
    [Header("----- Shovel Stats -----")]
    bool hasShovel;
    bool isSwinging;
    [SerializeField] GameObject shovelModel;
    [SerializeField] List<ShovelStats> shovelList;
    [SerializeField] int shovelDMG;
    [SerializeField] int shovelDurability;
    [SerializeField] float swingRate;
    [SerializeField] float shovelKnockback;
    [SerializeField] float knockbackStunDuration;
    [SerializeField] int shovelDist;
    [SerializeField] private GameObject shovelHitParticles;
    [SerializeField] private Material goldenShovelMaterial;
    [SerializeField] private ShovelStats defaultShovel;
    private Material shovelOriginalMaterial;
    bool hasGoldenShovel = false;

    [SerializeField] public int maxCropInInventory;
    public int currentCropsInInventory;

    public int maxSeedsInInventory;
    public int currentSeedsInInventory;

    bool isPlayingSteps;

    private int selectedShovel = -1;                 
    private float lastSwingTime;

    Vector3 moveDirection;
    Vector3 playerVelocity;
    //shield implementation (delete if we are not using it)
    public bool isShielded;
    private Coroutine shieldCoroutine;
    //public GameObject ShieldOverlay;
    //end of shield

    [SerializeField] public GameObject flashlight;

    // Start is called before the first frame update
    void Start()
    {
        HPOriginal = HP;
        UpdatePlayerUI();
        currentCropsInInventory = 0;

        crossbowAnimator.SetBool("Fire", true);
        crossbowAnimator.SetFloat("ShootRate", 1 / shootRate * 3);
    }

    // Update is called once per frame
    void Update()
    {
        // Show the shooting distance on the scene screen
        Debug.DrawRay(Camera.main.transform.position, Camera.main.transform.forward * shootDistance, Color.red);
        Movement();
        Sprint();
        Interact();
        UpdateActivePowerups();

        crossbowPowerupParticles.SetActive(ContainsPowerup(PowerupType.Boost));

        if (!hasGoldenShovel && ContainsPowerup(PowerupType.GoldenShovel))
        {
            if (shovelList.Count == 0)
            {
                GetShovelStats(defaultShovel);
            }

            hasGoldenShovel = true;
            shovelOriginalMaterial = shovelModel.GetComponent<MeshRenderer>().material;
            shovelModel.GetComponent<MeshRenderer>().material = goldenShovelMaterial;
        }
        if (hasGoldenShovel && !ContainsPowerup(PowerupType.GoldenShovel))
        {
            hasGoldenShovel = false;
            shovelModel.GetComponent<MeshRenderer>().material = shovelOriginalMaterial;
            shovelOriginalMaterial = null;
        }
    }

    // Setter for jumpCount private variable
    void SetJumpCount(int jump)
    {
        jumpCount = jump;
    }

    // Getter for jumpCount private variable
    int GetJumpCount()
    { 
        return jumpCount; 
    }

    void Interact()
    {
        RaycastHit hit;
        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, interactionRange, ~ignoreMask))
        {
            IInteract interact = hit.collider.GetComponent<IInteract>();
            if (interact != null)
            {
                interact.Interact();
            }
        }
    }

    void Movement()
    {
        if(pController.isGrounded)
        {
            SetJumpCount(0);
            if (playerVelocity.y < -3.0f)
            {
                AudioManager.Instance.playerLandSound.PlayOnPlayer();
            }
            playerVelocity = Vector3.zero;
        }

        moveDirection = (transform.forward * Input.GetAxis("Vertical")) + (transform.right * Input.GetAxis("Horizontal"));

        float speedPowerupModifier = ContainsPowerup(PowerupType.Speed) ? 2 : 1;

        pController.Move(moveDirection * speed * speedPowerupModifier * Time.deltaTime);


        Jump();

        pController.Move(playerVelocity * Time.deltaTime);
        playerVelocity.y -= gravity * Time.deltaTime;

        if(Input.GetButton("Fire1") && !isShooting)
        {
            StartCoroutine(Shoot());
        }
        if (hasShovel && Input.GetKeyDown(KeyCode.F) && !isShooting)
        {
            StartCoroutine(SwingShovel());
        }

        if (pController.isGrounded && !isPlayingSteps && moveDirection.magnitude > 0.3f)
        {
            StartCoroutine(PlaySteps());
        }
    }
    
    void Jump()
    {
        if(Input.GetButtonDown("Jump") && GetJumpCount() < jumpMax)
        {
            SetJumpCount(GetJumpCount() + 1);
            playerVelocity.y = jumpSpeed;
            AudioManager.Instance.playerJumpSound.PlayOnPlayer();
        }
    }

    void Sprint()
    {
        if(Input.GetButtonDown("Sprint"))
        {
            speed *= sprint;
            isSprinting = true;
        }
        else if(Input.GetButtonUp("Sprint"))
        {
            speed /= sprint;
            isSprinting = false;
        }
    }

    IEnumerator Shoot()
    {
        isShooting = true;

        if (ContainsPowerup(PowerupType.Boost))
        {
            AudioManager.Instance.boostPowerupShootSound.PlayOnPlayer();
        }
        else
        {
            AudioManager.Instance.shootSound.PlayOnPlayer();
        }

        crossbowAnimator.SetBool("Fire", false);

        RaycastHit hit;
        if(Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, shootDistance, ~ignoreMask))
        {
            // Show what the player is shooting on the Debug Log
            Debug.Log(hit.collider.name);

            IDamage dmg = hit.collider.GetComponent<IDamage>();

            if (ContainsPowerup(PowerupType.Boost))
            {
                Instantiate(boostHitEffect, hit.point, Quaternion.identity);
            }
            else
            {
                Instantiate(arrowHitEffect, hit.point, Quaternion.identity);
            }

            // If damage is not null set damage amount to shoot damage
            if (dmg != null )
            {
                dmg.TakeDamage(shootDamage * (ContainsPowerup(PowerupType.Boost) ? 2 : 1));
            }
        }

        // Wait for duration of the shooting rate and set isShooting back to false
        yield return new WaitForSeconds(shootRate / (ContainsPowerup(PowerupType.Boost) ? 2 : 1));

        crossbowAnimator.SetBool("Fire", true);

        isShooting = false;
    }

    IEnumerator PlaySteps()
    {
        isPlayingSteps = true;

        AudioManager.Instance.footstepSound.PlayOnPlayer();

        if (isSprinting)
        {
            yield return new WaitForSeconds(0.3f);
        }
        else
        {
            yield return new WaitForSeconds(0.5f);
        }

        isPlayingSteps = false;
    }

    public void TakeDamage(int amount)
    {
        //shield implimentation
        if (isShielded)
        {
            
            return;
        }


        HP -= amount;
        UpdatePlayerUI();
        AudioManager.Instance.playerHurtSound.PlayOnPlayer();
        StartCoroutine(FlashDamage());

        // Player is killed
        if(HP <= 0)
        {
            GameManager.Instance.YouLose();
        }
    }

    public void HealUp()
    {
        if (HP < HPOriginal)
        {
            HP = HPOriginal;
            UpdatePlayerUI();
        }
        
    }

    public void HealAmount(int amount)
    {
        if (HP + amount > HPOriginal)
        {
            HP = HPOriginal;
        }
        else
        {
            HP = HP + amount;
        }
        UpdatePlayerUI();
    }

    public void UpdatePlayerUI()
    {
        GameManager.Instance.playerHPBar.fillAmount = (float)HP / HPOriginal;
    }

    IEnumerator FlashDamage()
    {
        GameManager.Instance.playerDamageScreen.SetActive(true);
        yield return new WaitForSeconds(0.1f);
        GameManager.Instance.playerDamageScreen.SetActive(false);
    }

    //shield implementation (delete if we decide not to use it)
    public void ActivateShield(float duration)
    {
        if (shieldCoroutine != null)
        {
            StopCoroutine(shieldCoroutine);
        }

        shieldCoroutine = StartCoroutine(shieldWithOverlay(duration));
    }

    private IEnumerator shieldWithOverlay(float duration)
    {
        isShielded = true;

        GameManager.Instance.StartCoroutine(GameManager.Instance.ApplyShieldEffect(duration));

        yield return new WaitForSeconds(duration);

        isShielded = false;

        shieldCoroutine = null;
    }
    //end of shield

    public void GetShovelStats(ShovelStats shovel)
    {
        shovelList.Add(shovel);                       
        selectedShovel = shovelList.Count - 1;       
        shovelDMG = shovel.shovelDMG;
        shovelDurability = shovel.durability;
        swingRate = shovel.swingRate;
        shovelKnockback = shovel.shovelKnockback;
        knockbackStunDuration = shovel.knockbackStunDuration;
        shovelDist = shovel.shovelDist;
        shovelModel.GetComponent<MeshFilter>().sharedMesh = shovel.shovelModel.GetComponent<MeshFilter>().sharedMesh;
        shovelModel.GetComponent<MeshRenderer>().sharedMaterial = shovel.shovelModel.GetComponent<MeshRenderer>().sharedMaterial;
        shovelModel.SetActive(true);

        hasShovel = true;
        GameManager.Instance.AddControlPopup("Shovel", "F");
        AudioManager.Instance.shovelPickupSound.Play();
        Debug.Log("Shovel equipped: " + shovel.name);
    }


    IEnumerator SwingShovel()
    {
        // Ensure player isn't already swinging
        isSwinging = true;

        // Check for hits within the shovel's distance
        Collider[] hitColliders = Physics.OverlapSphere(transform.position + transform.forward * shovelDist / 2, shovelDist / 2, ~ignoreMask);

        int enemiesHit = 0;
        foreach (Collider hitCollider in hitColliders)
        {
            if (!hitCollider.CompareTag("Enemy")) { continue; }
            enemiesHit++;

            Debug.Log($"Shovel hit: {hitCollider.name}");

            IDamage dmg = hitCollider.GetComponent<IDamage>();
            if (dmg != null)
            {
                dmg.TakeDamage(ContainsPowerup(PowerupType.GoldenShovel) ? 9999 : shovelDMG);
            }

            IKnockback knockback = hitCollider.GetComponent<IKnockback>();
            if (knockback != null)
            {
                Vector3 dir = transform.position - hitCollider.transform.position;
                knockback.Knockback(dir.normalized, shovelKnockback, knockbackStunDuration);
            }

            Instantiate(shovelHitParticles, hitCollider.transform.position, Quaternion.identity);
        }

        if (enemiesHit > 0)
        {
            if (!ContainsPowerup(PowerupType.GoldenShovel))
            {
                shovelDurability--;
            }
            AudioManager.Instance.shovelHitSound.PlayOnPlayer();
        }
        Debug.Log("Shovel durability: " + shovelDurability);

        if (shovelDurability <= 0)
        {
            BreakShovel();
        }

        yield return new WaitForSeconds(swingRate);
        isSwinging = false;
    }

    void BreakShovel()
    {
        hasShovel = false;
        shovelDMG = 0;
        shovelDist = 0;
        swingRate = 0;
        shovelModel.SetActive(false);

        shovelList.RemoveAt(shovelList.Count - 1);

        GameManager.Instance.RemoveControlPopup("Shovel");

        Debug.Log("Shovel broke!");
    }

    public void PickUpCrop()
    {
        if(currentCropsInInventory < maxCropInInventory)
        {
            currentCropsInInventory++;
            Debug.Log($"Crops in Inventory: " + currentCropsInInventory);
            GameManager.Instance.UpdateCropInventory(currentCropsInInventory);

        }


    }

    public void PlaceCrop()
    {
        if(currentSeedsInInventory > 0)
        {
            currentSeedsInInventory--;

            Debug.Log($"Seeds in Inventory: " + currentSeedsInInventory);
            GameManager.Instance.UpdateSeedInventory(currentSeedsInInventory);

        }

    }

    private void OnDrawGizmosSelected()
    {
        if (hasShovel)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, shovelDist);
        }
    }

    public enum PowerupType { Speed, Freeze, Boost, Shield, GoldenShovel }
    [System.Serializable]
    public class ActivePowerup
    {
        public PowerupType type;
        public float maxDuration;
        public float duration;
    }

    public bool ContainsPowerup(PowerupType type)
    {
        for (int i = 0; i < activePowerups.Count; i++)
        { 
            if (activePowerups[i].type == type)
            {
                return true;
            }
        }
        return false;
    }

    public void AddPowerup(PowerupType type, float duration)
    {
        for (int i = 0; i < activePowerups.Count; i++)
        {
            if (activePowerups[i].type == type)
            {
                activePowerups[i].duration = duration;
                activePowerups[i].maxDuration = duration;
                return;
            }
        }

        ActivePowerup newPowerup = new ActivePowerup();
        newPowerup.type = type;
        newPowerup.maxDuration = duration;
        newPowerup.duration = duration;

        activePowerups.Add(newPowerup);
    }

    private void UpdateActivePowerups()
    {
        for (int i = 0; i < activePowerups.Count; i++)
        {
            activePowerups[i].duration -= Time.deltaTime;
            if (activePowerups[i].duration <= 0)
            {
                activePowerups.Remove(activePowerups[i]);
            }
        }
    }
}
