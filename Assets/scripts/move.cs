using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class move : MonoBehaviour
{
    public CharacterController2D controller;
    public float runSpeed = 40.0f;
    float horizontalMove = 0f;
    bool jump = false;
    bool crouch = false;
    public Animator animator;
    
    [Header("Enemy Interaction")]
    [SerializeField] private float enemyBounceForce = 10f;
    [SerializeField] private float invincibilityTime = 1f;
    [SerializeField] private int maxHealth = 3;
    
    [Header("Death Settings")]
    [SerializeField] private GameObject gameOverUI;
    [SerializeField] private GameObject deathEffect;
    [SerializeField] private float fadeOutTime = 1f;
    
    [Header("Audio")]
    [SerializeField] private AudioClip deathSound;
    [SerializeField] private AudioClip hurtSound;
    [SerializeField] private AudioClip collectSound;
    [SerializeField] private float soundVolume = 1f; // Contrôle le volume
    
    [Header("UI References - ASSIGN IN INSPECTOR")]
    public TMP_Text scoreText;          // Score en jeu
    public TMP_Text diamondText;        // Diamants en jeu
    public TMP_Text finalScoreText;     // Score final sur GameOver
    public TMP_Text gameOverText;       // Texte Game Over
    
    // Variables privées
    private int currentHealth;
    private int currentScore = 0;
    private int diamondCount = 0;
    private bool isInvincible = false;
    private float invincibilityTimer = 0f;
    private bool isDead = false;
    private Rigidbody2D rb;
    private AudioSource audioSource;
    private SpriteRenderer spriteRenderer;
    
    void Start()
    {
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        // Configurer l'AudioSource
        SetupAudioSource();
        
        // Initialiser le score et les diamants à 0
        currentScore = 0;
        diamondCount = 0;
        
        // Cacher Game Over UI au début
        if (gameOverUI != null)
        {
            gameOverUI.SetActive(false);
        }
        
        // Mettre à jour l'UI initiale
        UpdateScoreDisplay();
        UpdateDiamondDisplay();
        
        Debug.Log("Game started - Score: 0, Diamonds: 0");
    }
    
    void SetupAudioSource()
    {
        // Récupérer ou créer l'AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            Debug.Log("AudioSource ajouté au joueur");
        }
        
        // Configurer l'AudioSource
        audioSource.playOnAwake = false;
        audioSource.volume = soundVolume;
        audioSource.spatialBlend = 0f; // 2D sound
        audioSource.loop = false;
        
        Debug.Log($"AudioSource configuré. Collect sound: {collectSound != null}, Hurt sound: {hurtSound != null}, Death sound: {deathSound != null}");
    }
    
    void Update()
    {
        if (isDead) return;
        
        horizontalMove = Input.GetAxisRaw("Horizontal") * runSpeed;
        animator.SetFloat("speed", Mathf.Abs(horizontalMove));

        if (Input.GetButtonDown("Jump"))
        {
            jump = true;
            animator.SetBool("IsJump", true);
        }
        if (Input.GetButtonDown("Crouch"))
        {
            crouch = true;
        }
        else if (Input.GetButtonUp("Crouch"))
        {
            crouch = false;
        }
        
        if (isInvincible)
        {
            invincibilityTimer -= Time.deltaTime;
            if (invincibilityTimer <= 0)
            {
                isInvincible = false;
                StopBlinking();
            }
        }
    }

    private void StopBlinking()
    {
        if (spriteRenderer != null)
            spriteRenderer.color = Color.white;
    }

    public void OnLanding()
    {
        animator.SetBool("IsJump", false);
    }
    
    public void OnCrouch(bool IsCrouch)
    {
        animator.SetBool("IsCrouch", IsCrouch);
    }

    void FixedUpdate()
    {
        if (isDead) return;
        
        controller.Move(horizontalMove * Time.fixedDeltaTime, crouch, jump);
        jump = false;
    }

    // ============ COLLECTIBLES ============
    
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (isDead) return;
        
        // Le Collectibleitem gère lui-même le score
        // On fait juste jouer le son ici
        if (collision.CompareTag("Diamond") || collision.CompareTag("Cherry"))
        {
            PlayCollectSound();
        }
    }
    
    void PlayCollectSound()
    {
        if (collectSound != null)
        {
            if (audioSource != null)
            {
                Debug.Log("Jouer son collect");
                audioSource.PlayOneShot(collectSound, soundVolume);
            }
            else
            {
                Debug.LogWarning("AudioSource est null!");
                // Essayer de jouer le son directement
                AudioSource.PlayClipAtPoint(collectSound, transform.position, soundVolume);
            }
        }
        else
        {
            Debug.LogWarning("Collect sound clip non assigné!");
        }
    }
    
    void PlayHurtSound()
    {
        if (hurtSound != null)
        {
            if (audioSource != null)
            {
                Debug.Log("Jouer son hurt");
                audioSource.PlayOneShot(hurtSound, soundVolume);
            }
            else
            {
                AudioSource.PlayClipAtPoint(hurtSound, transform.position, soundVolume);
            }
        }
    }
    
    void PlayDeathSound()
    {
        if (deathSound != null)
        {
            Debug.Log("Jouer son death");
            // Pour le son de mort, on utilise PlayClipAtPoint pour être sûr qu'il joue
            AudioSource.PlayClipAtPoint(deathSound, transform.position, soundVolume);
        }
    }
    
    // ============ GESTION DU SCORE ============
    
    public void AddScore(int points)
    {
        currentScore += points;
        UpdateScoreDisplay();
        Debug.Log($"Score added: {points}. Total: {currentScore}");
    }
    
    public void AddDiamond(int amount)
    {
        diamondCount += amount;
        UpdateDiamondDisplay();
        Debug.Log($"Diamond added: {amount}. Total: {diamondCount}");
    }
    
    void UpdateScoreDisplay()
    {
        if (scoreText != null)
        {
            scoreText.text = $"Score: {currentScore}";
        }
        
        if (finalScoreText != null)
        {
            finalScoreText.text = $"Final Score: {currentScore}";
        }
    }
    
    void UpdateDiamondDisplay()
    {
        if (diamondText != null)
        {
            diamondText.text = $"Diamonds: {diamondCount}";
        }
    }
    
    // ============ COMBAT ============
    
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            HandleEnemyCollision(collision);
        }
    }
    
    void HandleEnemyCollision(Collision2D collision)
    {
        bool jumpedOnEnemy = false;
        
        foreach (ContactPoint2D contact in collision.contacts)
        {
            if (contact.normal.y > 0.5f)
            {
                jumpedOnEnemy = true;
                DestroyEnemy(collision.gameObject);
                
                if (rb != null)
                {
                    rb.linearVelocity = new Vector2(rb.linearVelocity.x, enemyBounceForce);
                }
                break;
            }
        }
        
        if (!jumpedOnEnemy && !isInvincible && !isDead)
        {
            TakeDamage(1, collision.transform.position);
        }
    }
    
    void DestroyEnemy(GameObject enemy)
    {
        EnemyController enemyController = enemy.GetComponent<EnemyController>();
        if (enemyController != null)
        {
            enemyController.GetJumpedOn();
        }
        else
        {
            Destroy(enemy);
        }
    }
    
    public void TakeDamage(int damage, Vector2 damageSource)
    {
        if (isInvincible || isDead) return;
        
        currentHealth -= damage;
        isInvincible = true;
        invincibilityTimer = invincibilityTime;
        
        // Jouer le son de dégâts
        PlayHurtSound();
        
        if (rb != null)
        {
            Vector2 knockbackDirection = (Vector2)transform.position - damageSource;
            knockbackDirection = knockbackDirection.normalized;
            rb.AddForce(knockbackDirection * 5f, ForceMode2D.Impulse);
        }
        
        animator.SetTrigger("Hurt");
        StartCoroutine(BlinkEffect());
        
        Debug.Log($"Damage taken! Health: {currentHealth}/{maxHealth}");
        
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    System.Collections.IEnumerator BlinkEffect()
    {
        while (isInvincible && !isDead)
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.color = new Color(1, 1, 1, 0.3f);
                yield return new WaitForSeconds(0.1f);
                spriteRenderer.color = Color.white;
                yield return new WaitForSeconds(0.1f);
            }
            else
            {
                yield return new WaitForSeconds(0.2f);
            }
        }
    }
    
    // ============ MORT ============
    
    void Die()
    {
        if (isDead) return;
        
        isDead = true;
        Debug.Log($"Player died! Final Score: {currentScore}, Final Diamonds: {diamondCount}");
        
        // Jouer le son de mort
        PlayDeathSound();
        
        // Arrêter le mouvement
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.isKinematic = true;
        }
        
        // Animation de mort
        animator.SetBool("IsDead", true);
        
        // Désactiver les collisions
        GetComponent<Collider2D>().enabled = false;
        
        // Effet visuel
        if (deathEffect != null)
        {
            Instantiate(deathEffect, transform.position, Quaternion.identity);
        }
        
        // Afficher le Game Over UI immédiatement
        ShowGameOver();
    }
    
    void ShowGameOver()
    {
        if (gameOverUI != null)
        {
            UpdateScoreDisplay();
            UpdateDiamondDisplay();
            gameOverUI.SetActive(true);
            
            if (gameOverText != null)
            {
                gameOverText.text = "GAME OVER";
            }
            
            SetupGameOverButtons();
            Time.timeScale = 0f;
            Debug.Log("Game Over UI shown");
        }
    }
    
    void SetupGameOverButtons()
    {
        if (gameOverUI != null)
        {
            Button restartButton = null;
            Transform restartTransform = gameOverUI.transform.Find("RestartButton");
            
            if (restartTransform != null)
            {
                restartButton = restartTransform.GetComponent<Button>();
            }
            
            if (restartButton == null)
            {
                restartButton = gameOverUI.GetComponentInChildren<Button>();
            }
            
            if (restartButton != null)
            {
                restartButton.onClick.RemoveAllListeners();
                restartButton.onClick.AddListener(RestartGame);
                restartButton.interactable = true;
                Debug.Log("Restart button configured");
            }
        }
    }
    
    // ============ FONCTIONS UI ============
    
    public void RestartGame()
    {
        Debug.Log("Restarting game...");
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    
    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
    
    public void QuitGame()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
    
    // ============ GETTERS ============
    
    public int GetCurrentHealth() { return currentHealth; }
    public int GetMaxHealth() { return maxHealth; }
    public bool IsDead() { return isDead; }
    public int GetCurrentScore() { return currentScore; }
    public int GetDiamondCount() { return diamondCount; }
    
    public void AddHealth(int amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
    }
}