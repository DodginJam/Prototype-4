using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // Related to Player
    private Rigidbody PlayerRigidBody
    { get; set; }
    private GameObject FocalPoint
    { get; set; }
    [field: SerializeField] public float PlayerSpeed
    { get; private set; }
    private float VerticalMovement
    { get; set; }
    private bool IsGrounded
    { get; set; }
    private float DefaultMass
    { get; set; }
    public int PlayerLives
    { get; set; } = 5;
    private float FallHeightLimit
    { get; set; } = -10.0f;
    public bool GameOver
    { get; set; } = false;
    private Vector3 StartPosition
    { get; set; }

    // Related to Dash Ability.
    private bool IsDashing
    { get; set; } = false;
    private bool DashInputPressed
    { get; set; } = false;
    private float DashForce
    { get; set; } = 300.0f;
    private float DashCooldownTimer
    { get; set; } = 2.5f;
    private float MassModifierDash
    { get; set; } = 2.5f;
    private float DashReturnMassToNormalTimer
    { get; set; } = 0.5f;
    [field: SerializeField] public Light DashingLightObject
    { get; set; }


    // Related to PowerUps Indicators.
    private GameObject PowerUpIndicatorParent
    { get; set; }
    public Vector3 PowerUpIndicatorOffsetPosition
    { get; private set; }
    [field: SerializeField] public GameObject PushBackIndicator
    { get; private set; }
    [field: SerializeField] public GameObject ProjectileIndicator
    { get; private set; }
    [field: SerializeField] public GameObject SmashAttackIndicator
    { get; private set; }

    // Related to PowerUp GamePlay state - PushBack
    private bool HasPowerUpPushBack
    { get; set; }
    [field: SerializeField] public GameObject PushBackObject
    { get; set; }
    public Vector3 PushBackObjectOffset 
    { get; set; } = new Vector3(0, -0.45f, 0);
    private float PushBackForceMultiplier
    { get; set; } = 500.0f;
    private int PushBackCoruntinesRunning
    { get; set; } = 0;
    bool FlashLightCoruntinesRunning
    { get; set; } = false;

    // Related to PowerUp GamePlay state - Projectile
    private bool HasPowerUpProjectiles
    { get; set; }
    [field: SerializeField] public GameObject ProjectilePrefab
    { get; private set; }
    public float ProjectileSpeed
    { get; set; } = 20.0f;
    private bool HasProjectileInputPressed
    { get; set; }
    private int ProjectileCoruntinesRunning
    { get; set; } = 0;
    private bool ProjectileOnCooldown
    { get; set; } = false;
    private float ProjectileOnCooldownTimer
    { get; set; } = 0.5f;

    // Related to PowerUp GamePlay state - Smash Attack
    private bool HasPowerUpSmashAttack
    { get; set; }
    private bool HasSmashAttackInputPressed
    { get; set; }
    private int SmashAttackCoruntinesRunning
    { get; set; } = 0;
    private bool SmashAttackOnCooldown
    { get; set; } = false;
    private float SmashAttackOnCooldownTimer
    { get; set; } = 2.0f;
    private float SmashAttackJumpForce
    { get; set; } = 25000.0f; // This is very high, as the mass of the player is increased during the Smash Ability.
    private float SmashAttackSlamForce
    { get; set; } = 50000.0f; // This is very high, as the mass of the player is increased during the Smash Ability.
    private float MassModifierOnSmashAttack
    { get; set; } = 100.0f;
    private float ReturnToGroundTimer
    { get; set; } = 0.5f;
    private float SmashAttackForceMultiplier
    { get; set; } = 30000.0f;
    private float SmashAttackExplosionRadius
    { get; set; } = 10.0f;
    [field: SerializeField] public GameObject SmashRadiusObject
    { get; set; }
    private Vector3 SmashRadiusObjectPositionalOffset
    { get; set; } = new Vector3(0, -0.75f, 0);
    private float SmashRadiusMaterialAlphaDefault
    { get; set; } = 0.15f;


    // Reference to other scripts.
    private SpawnManager SpawnManagerScript
    { get; set; }
    private ProjectileDestroyEnemy ProjectileDestroyEnemyScript
    { get; set; }
    private UserInterfaceManager UserInterfaceManagerScript
    { get; set; }
    private PlayerAudioManager PlayerAudioManagerScript
    { get; set; }


    // Start is called before the first frame update
    void Start()
    {
        PlayerRigidBody = GetComponent<Rigidbody>();
        DefaultMass = PlayerRigidBody.mass;
        StartPosition = transform.position;

        FocalPoint = GameObject.Find("Focal Point");

        PowerUpIndicatorParent = GameObject.Find("PowerUpIndicators");
        PowerUpIndicatorOffsetPosition = PowerUpIndicatorParent.transform.position - transform.position;

        SpawnManagerScript = GameObject.Find("SpawnManager").GetComponent<SpawnManager>();

        UserInterfaceManagerScript = GameObject.Find("UserInterfaceManager").GetComponent<UserInterfaceManager>();
        UserInterfaceManagerScript.UpdateLivesNumberDisplay(PlayerLives);

        PlayerAudioManagerScript = GetComponent<PlayerAudioManager>();

        // Ensures that the object size matches the areas of effect of the explosion.
        SmashRadiusObject.transform.localScale = new Vector3(SmashAttackExplosionRadius * 2, 0.015f, SmashAttackExplosionRadius * 2);
    }

    // Update is called once per frame
    void Update()
    {
        // Objects tracked to the location of the player (with an offset), such as indicators and area of effect Game objects.
        PowerUpIndicatorParent.transform.position = transform.position + PowerUpIndicatorOffsetPosition;
        PushBackObject.transform.position = transform.position + PushBackObjectOffset;
        SmashRadiusObject.transform.position = transform.position + SmashRadiusObjectPositionalOffset;

        CaptureUserInputForUpdate();

        // On player fall off, minus lives and either return to platform or end game.
        if (transform.position.y < FallHeightLimit && !GameOver)
        {
            PlayerLives--;
            UserInterfaceManagerScript.UpdateLivesNumberDisplay(PlayerLives);

            if (PlayerLives >= 1)
            {
                transform.position = StartPosition;
                PlayerRigidBody.velocity = PlayerRigidBody.velocity.normalized;
            }
            else if (PlayerLives == 0)
            {
                GameOver = true;
                UserInterfaceManagerScript.GameOverTextDisplay.enabled = true;
            }
        }

        RestartGameOnKey();
    }

    private void FixedUpdate()
    {
        PlayerMovement();
        ShootProjectile();
        SmashAttack();
        DashPlayer();
    }

    private void OnTriggerEnter(Collider other)
    {
        PlayerAndPowerUpTrigger(other);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            IsGrounded = true;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            IsGrounded = false;
        }
    }
    void CaptureUserInputForUpdate()
    {
        if (!GameOver)
        {
            VerticalMovement = Input.GetAxis("Vertical");

            // Default abilities.

            // Since the Dash ability temporarily changes the players mass to prevent being bounced off another enemy to strongly, to avoid conflicting with other abilities that modify mass, only allow dash when player has default mass. 
            if (Input.GetKeyDown(KeyCode.LeftShift) && !IsDashing && PlayerRigidBody.mass == DefaultMass)
            {
                DashInputPressed = true;
            }

            // PowerUp abilities.
            // Accepts input only if the player has the PowerUp bool set to true and the abilty isn't on a cooldown.

            if (Input.GetKeyDown(KeyCode.Space) && HasPowerUpProjectiles && !ProjectileOnCooldown)
            {
                // The bool allows the abilty method to activated within FixedUpdate.
                HasProjectileInputPressed = true;
            }

            // Since the Smash Attack rises the player and changes there mass, only allow the Smash Attack to occur when player is Grounded and has Default mass (prevents conflicting with other abilities that modify these values too).
            if (Input.GetKeyDown(KeyCode.C) && HasPowerUpSmashAttack && !SmashAttackOnCooldown && IsGrounded && PlayerRigidBody.mass == DefaultMass)
            {
                // The bool allows the abilty method to activated within FixedUpdate.
                // Cooldown for smash attack is only activated only once the player has re-contacted the ground.
                HasSmashAttackInputPressed = true;
            }
        }
    }

    void DashPlayer()
    {
        if (DashInputPressed)
        {
            DashInputPressed = false;

            Vector3 directionOfMovement = FocalPoint.transform.forward;

            int maxOutVerticalMovement;

            if (VerticalMovement >= 0)
            {
                maxOutVerticalMovement = 1;
            }
            else
            {
                maxOutVerticalMovement = -1;
            }

            float newMass = PlayerRigidBody.mass * MassModifierDash;
            PlayerRigidBody.mass = newMass;

            PlayerRigidBody.AddForce(directionOfMovement * DashForce * maxOutVerticalMovement, ForceMode.Impulse);

            IsDashing = true;

            DashingLightObject.enabled = true;

            // Update UI
            UserInterfaceManagerScript.UpdateAbilityColor("DashTextDisplay", Color.white);

            // Audio clips.
            PlayerAudioManagerScript.PlayAudioClip(PlayerAudioManagerScript.Dash, 2.5f);

            StartCoroutine(DashCoolDown(DashCooldownTimer));
            StartCoroutine(ReturnToNormalMassAfterDash(DashReturnMassToNormalTimer));
        }
    }

    IEnumerator ReturnToNormalMassAfterDash(float timer)
    {
        yield return new WaitForSeconds(timer);

        PlayerRigidBody.mass = DefaultMass;
        DashingLightObject.enabled = false;
    }

    IEnumerator DashCoolDown(float timer)
    {
        yield return new WaitForSeconds(timer);

        IsDashing = false;
        UserInterfaceManagerScript.UpdateAbilityColor("DashTextDisplay", Color.green);
    }

    void PlayerMovement()
    {
        if (IsGrounded)
        {
            PlayerRigidBody.AddForce(VerticalMovement * PlayerSpeed * FocalPoint.transform.forward, ForceMode.Force);
        }
    }

    // This is accessed via the PushBack shield gameObject that's activated upon PushBack powerup collected.
    public void PushBackEnemy(Collider other)
    {
        // Provide a forceful push on enemies when PushBack power up is active.
        if (other.gameObject.CompareTag("Enemy") && HasPowerUpPushBack)
        {
            // Rigidbody.
            Rigidbody enemyRigidBody = other.gameObject.GetComponent<Rigidbody>();
            Vector3 awayFromPlayerDirection = (other.gameObject.transform.position - transform.position).normalized;

            enemyRigidBody.AddForce(awayFromPlayerDirection * PushBackForceMultiplier, ForceMode.Impulse);

            // UI Update
            UserInterfaceManagerScript.UpdateAbilityColor("RepulseTextDisplay", Color.yellow);

            // Audio clip.
            PlayerAudioManagerScript.PlayAudioClip(PlayerAudioManagerScript.PushBack, 2.5f);

            // Visual Effects.
            if (FlashLightCoruntinesRunning)
            {
                StopCoroutine(FlashOnPushBackTrigger());
            }
            StartCoroutine(FlashOnPushBackTrigger());
        }
    }

    IEnumerator FlashOnPushBackTrigger()
    {
        float defaultIntensity = 2.0f;
        float onTriggerIntensity = 5.0f;

        Light light = GameObject.Find("PushBackSphere").GetComponentInChildren<Light>();

        // Light intensity quickly increases on contact, before returning to normal at a slower rate.
        for (float currentLightIntensity = light.intensity; currentLightIntensity < onTriggerIntensity; currentLightIntensity += 0.25f)
        {
            light.intensity = currentLightIntensity;
            yield return new WaitForSeconds(0.005f);
        }

        for (float currentLightIntensity = light.intensity; currentLightIntensity > defaultIntensity; currentLightIntensity -= 0.25f)
        {
            light.intensity = currentLightIntensity;
            yield return new WaitForSeconds(0.05f);
        }
    }

    void PlayerAndPowerUpTrigger(Collider other)
    {
        if (other.CompareTag("PowerUp"))
        {
            if (other.name.Contains("PowerUpPushBack"))
            {
                HasPowerUpPushBack = true;
                PushBackIndicator.SetActive(true);
                PushBackObject.SetActive(true);

                // Update UI
                UserInterfaceManagerScript.UpdateAbilityColor("RepulseTextDisplay", Color.yellow);

                // Audio Clip.
                PlayerAudioManagerScript.PlayAudioClip(PlayerAudioManagerScript.PushBackPickedUp);

                StartCoroutine(PowerUpCountDown("PowerUpPushBack"));
                PushBackCoruntinesRunning++;
            }
            else if (other.name.Contains("PowerUpProjectile"))
            {
                HasPowerUpProjectiles = true;
                ProjectileIndicator.SetActive(true);

                // Update UI
                UserInterfaceManagerScript.UpdateAbilityColor("LasersTextDisplay", Color.red);

                // Audio Clips.
                PlayerAudioManagerScript.PlayAudioClip(PlayerAudioManagerScript.ProjectilePickedUp);

                StartCoroutine(PowerUpCountDown("PowerUpProjectile"));
                ProjectileCoruntinesRunning++;
            }
            else if (other.name.Contains("PowerUpSmashAttack"))
            {
                HasPowerUpSmashAttack = true;
                SmashAttackIndicator.SetActive(true);

                // Update UI
                UserInterfaceManagerScript.UpdateAbilityColor("SmashTextDisplay", Color.blue);

                // Audio Clips.
                PlayerAudioManagerScript.PlayAudioClip(PlayerAudioManagerScript.SmashPickedUp);

                StartCoroutine(PowerUpCountDown("PowerUpSmashAttack"));
                SmashAttackCoruntinesRunning++;
            }

            Destroy(other.gameObject);
        }
    }

    // The Corrutine will only set powerup states to false once no other Coruntines regarding the same type of Powerup are running.
    IEnumerator PowerUpCountDown(string powerUpName)
    {
        yield return new WaitForSeconds(7);

        int CoroutineRunningRefTemp;
        bool HasPowerUpRefTemp;

        if (powerUpName == "PowerUpPushBack")
        {
            CoroutineRunningRefTemp = PushBackCoruntinesRunning;
            HasPowerUpRefTemp = HasPowerUpPushBack;

            HandlePowerUpEnd(ref CoroutineRunningRefTemp, ref HasPowerUpRefTemp, PushBackIndicator, "RepulseTextDisplay", PushBackObject);

            PushBackCoruntinesRunning = CoroutineRunningRefTemp;
            HasPowerUpPushBack = HasPowerUpRefTemp;
        }
        else if (powerUpName == "PowerUpProjectile")
        {
            CoroutineRunningRefTemp = ProjectileCoruntinesRunning;
            HasPowerUpRefTemp = HasPowerUpProjectiles;

            HandlePowerUpEnd(ref CoroutineRunningRefTemp, ref HasPowerUpRefTemp, ProjectileIndicator, "LasersTextDisplay");

            ProjectileCoruntinesRunning = CoroutineRunningRefTemp;
            HasPowerUpProjectiles = HasPowerUpRefTemp;
        }
        else if (powerUpName == "PowerUpSmashAttack")
        {
            CoroutineRunningRefTemp = SmashAttackCoruntinesRunning;
            HasPowerUpRefTemp = HasPowerUpSmashAttack;

            HandlePowerUpEnd(ref CoroutineRunningRefTemp, ref HasPowerUpRefTemp, SmashAttackIndicator, "SmashTextDisplay");

            SmashAttackCoruntinesRunning = CoroutineRunningRefTemp;
            HasPowerUpSmashAttack = HasPowerUpRefTemp;
        }
    }

    void HandlePowerUpEnd(ref int powerUpsRunning, ref bool hasPowerUp, GameObject Indicator, string textDisplayName, GameObject objectToDeactivate = null)
    {
        if (powerUpsRunning > 1)
        {
            powerUpsRunning--;
        }
        else if (powerUpsRunning == 1)
        {
            Indicator.SetActive(false);
            hasPowerUp = false;
            powerUpsRunning--;

            UserInterfaceManagerScript.UpdateAbilityColor(textDisplayName, Color.black);

            if (objectToDeactivate != null)
            {
                objectToDeactivate.SetActive(false);
            }
        }
    }

    void ShootProjectile()
    {
        // Check if input bool is true, and only allow Projectiles spawn if that is true. 
        if (HasProjectileInputPressed)
        {
            HasProjectileInputPressed = false;

            Vector3 enemyPosition;
            Vector3 directionToEnemy;

            // Spawn a projectile for each enemy in the scene, and send the projectile towards the location of the enemy.
            for (int i = 0; i < SpawnManagerScript.EnemiesInScene.Count; i++)
            {
                enemyPosition = SpawnManagerScript.EnemiesInScene[i].transform.position;

                directionToEnemy = (enemyPosition - transform.position).normalized;

                GameObject projectileSpawn = Instantiate(ProjectilePrefab, (transform.position + directionToEnemy), Quaternion.identity);
                
                // Within the projectiles script componenet, add into it a reference the enemy it is being sent after - for sake of homing projectile feature.
                ProjectileDestroyEnemyScript = projectileSpawn.GetComponent<ProjectileDestroyEnemy>();

                ProjectileDestroyEnemyScript.EnemyOfFocus = SpawnManagerScript.EnemiesInScene[i];
                ProjectileDestroyEnemyScript.ProjectileSpeed = ProjectileSpeed;
            }

            StartCoroutine(ShootLimit(ProjectileOnCooldownTimer));

            // Update UI
            UserInterfaceManagerScript.UpdateAbilityColor("LasersTextDisplay", Color.white);

            // Audio Clips.
            PlayerAudioManagerScript.PlayAudioClip(PlayerAudioManagerScript.Projectile, 3.0f);
        }
    }

    void SmashAttack()
    {
        if (HasSmashAttackInputPressed)
        {
            HasSmashAttackInputPressed = false;

            // AddForce to Player to raise into the air. The mass is increased on launch to stop the player from bouncing hard off other enemies on rise or fall.
            PlayerRigidBody.velocity = Vector3.zero;

            float newMass = PlayerRigidBody.mass * MassModifierOnSmashAttack;
            PlayerRigidBody.mass = newMass;

            PlayerRigidBody.AddForce(Vector3.up * SmashAttackJumpForce, ForceMode.Impulse);

            // Audio Clips.
            PlayerAudioManagerScript.PlayAudioClip(PlayerAudioManagerScript.PreSmashRise, 3.0f);

            // Update UI
            UserInterfaceManagerScript.UpdateAbilityColor("SmashTextDisplay", Color.white);

            // After X amount of time, slam the player into the ground.
            StartCoroutine(ReturnToGround(ReturnToGroundTimer));
        }
    }

    IEnumerator FadeOutExplosionRadius()
    {
        SmashRadiusObject.SetActive(true);

        Color currentColorOfSmashRadius = SmashRadiusObject.GetComponent<MeshRenderer>().material.color;

        float StartAlpha = SmashRadiusMaterialAlphaDefault;
        float EndAlpha = 0.0f;

        for (float currentAlpha = StartAlpha; currentAlpha > EndAlpha; currentAlpha -= 0.01f)
        {
            SmashRadiusObject.GetComponent<MeshRenderer>().material.color = new Color(currentColorOfSmashRadius.r, currentColorOfSmashRadius.g, currentColorOfSmashRadius.b, currentAlpha);
            yield return new WaitForSeconds(0.02f);
        }

        SmashRadiusObject.SetActive(false);
    }

    IEnumerator ExplodeOnReturnToGround()
    {
        yield return new WaitUntil(() => IsGrounded == true);

        for (int i = 0; i < SpawnManagerScript.EnemiesInScene.Count; i++)
        {
            Rigidbody currentEnemyRB = SpawnManagerScript.EnemiesInScene[i].GetComponent<Rigidbody>();

            currentEnemyRB.AddExplosionForce(SmashAttackForceMultiplier, transform.position, SmashAttackExplosionRadius);
        }

        // Audio Clips.
        PlayerAudioManagerScript.PlayAudioClip(PlayerAudioManagerScript.Smash, 3.0f);

        StartCoroutine(FadeOutExplosionRadius());

        // The mass is returned to starting amount on landing to allow bouncing of enemies.
        PlayerRigidBody.mass = DefaultMass;

        StartCoroutine(SmashLimit(SmashAttackOnCooldownTimer));
    }

    IEnumerator ReturnToGround(float timeToWait)
    {
        yield return new WaitForSeconds(timeToWait);

        PlayerRigidBody.AddForce(Vector3.down * SmashAttackSlamForce, ForceMode.Impulse);

        // On contact with ground, send out an explosive force to all enemy within a certain radius.
        StartCoroutine(ExplodeOnReturnToGround());
    }

    IEnumerator SmashLimit(float timeToWait)
    {
        SmashAttackOnCooldown = true;
        yield return new WaitForSeconds(timeToWait);
        SmashAttackOnCooldown = false;

        // Helps ensures UI returns to black once PowerUp has ended and cooldown is still techincally running in the background.
        if (HasPowerUpSmashAttack)
        {
            UserInterfaceManagerScript.UpdateAbilityColor("SmashTextDisplay", Color.blue);
        }
        else
        {
            UserInterfaceManagerScript.UpdateAbilityColor("SmashTextDisplay", Color.black);
        }
    }

    IEnumerator ShootLimit(float timeToWait)
    {
        ProjectileOnCooldown = true;
        yield return new WaitForSeconds(timeToWait);
        ProjectileOnCooldown = false;

        // Helps ensures UI returns to black once PowerUp has ended and cooldown is still techincally running in the background.
        if (HasPowerUpProjectiles)
        {
            UserInterfaceManagerScript.UpdateAbilityColor("LasersTextDisplay", Color.red);
        }
        else
        {
            UserInterfaceManagerScript.UpdateAbilityColor("LasersTextDisplay", Color.black);
        }
        
    }

    void RestartGameOnKey()
    {
        if (GameOver && Input.GetKeyDown(KeyCode.R))
        {
            GameOver = false;

            // Reset the player position.
            transform.position = StartPosition;
            PlayerRigidBody.velocity = Vector3.zero;
            PlayerRigidBody.rotation = Quaternion.Euler(0, 0, 0);

            // Remove all current enemies and powerups from the scene.
            GameObject[] enemiesInScene = GameObject.FindGameObjectsWithTag("Enemy");
            foreach(GameObject enemy in enemiesInScene)
            {
                Destroy(enemy);
            }

            GameObject[] powerUpsInScene = GameObject.FindGameObjectsWithTag("PowerUp");
            foreach (GameObject powerUp in powerUpsInScene)
            {
                Destroy(powerUp);
            }

            // Reset inportant gameplay varibles to there start numbers.
            PlayerLives = 5;
            SpawnManagerScript.WaveNumber = 0;
            SpawnManagerScript.NumberOfEnemiesInWave = 0;

            UserInterfaceManagerScript.InitialiseUIOnStart();
        }
    }
}
