using UnityEngine;
using System.Collections;

public class EnemyController : MonoBehaviour
{
    [Header("Enemy Type")]
    [SerializeField] private EnemyType enemyType = EnemyType.Ground;
    
    [Header("Enemy Settings")]
    [SerializeField] private int health = 1;
    [SerializeField] private float bounceForce = 5f;
    [SerializeField] private int damage = 1;
    [SerializeField] private GameObject deathEffect;
    
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private Transform wallCheck;
    [SerializeField] private LayerMask groundLayer;
    
    [Header("Eagle Specific")]
    [SerializeField] private float patrolRange = 3f;
    [SerializeField] private float attackRange = 5f;
    
    [Header("Animation Settings")]
    [SerializeField] private Sprite[] idleSprites;
    [SerializeField] private Sprite[] walkSprites;
    [SerializeField] private float animationSpeed = 0.2f;
    [SerializeField] private bool useSpriteAnimation = false;
    
    // Enum pour le type d'ennemi
    public enum EnemyType { Ground, Flying }
    
    // Variables privées
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Transform player;
    private bool facingRight = false;
    private bool isDead = false;
    private bool isAttacking = false;
    private Vector3 startPosition;
    private Coroutine animationCoroutine;
    
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        startPosition = transform.position;
        
        // Configurer selon le type d'ennemi
        if (enemyType == EnemyType.Flying)
        {
            // L'aigle vole, pas de gravité
            if (rb != null)
            {
                rb.gravityScale = 0;
                rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            }
            
            // Trouver le joueur
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                player = playerObj.transform;
        }
        
        // Démarrer l'animation
        if (useSpriteAnimation && idleSprites != null && idleSprites.Length > 0)
        {
            StartAnimation(idleSprites);
        }
    }
    
    void FixedUpdate()
    {
        if (isDead) return;
        
        switch (enemyType)
        {
            case EnemyType.Ground:
                UpdateGroundEnemy();
                break;
                
            case EnemyType.Flying:
                UpdateFlyingEnemy();
                break;
        }
    }
    
    void UpdateGroundEnemy()
    {
        // Déplacement simple gauche-droite
        float moveDirection = facingRight ? moveSpeed : -moveSpeed;
        rb.linearVelocity = new Vector2(moveDirection, rb.linearVelocity.y);
        
        // Flip selon la direction
        if (rb.linearVelocity.x > 0 && !facingRight)
        {
            Flip();
        }
        else if (rb.linearVelocity.x < 0 && facingRight)
        {
            Flip();
        }
        
        // Animation selon le mouvement
        if (useSpriteAnimation)
        {
            if (Mathf.Abs(rb.linearVelocity.x) > 0.1f)
            {
                if (walkSprites != null && walkSprites.Length > 0)
                {
                    StartAnimation(walkSprites);
                }
            }
            else
            {
                if (idleSprites != null && idleSprites.Length > 0)
                {
                    StartAnimation(idleSprites);
                }
            }
        }
        
        // Vérifier les collisions avec les bords
        CheckGroundCollisions();
    }
    
    void UpdateFlyingEnemy()
    {
        if (player != null)
        {
            float distanceToPlayer = Vector2.Distance(transform.position, player.position);
            
            if (distanceToPlayer < attackRange)
            {
                // Mode attaque: suivre le joueur
                isAttacking = true;
                Vector3 targetPosition = new Vector3(player.position.x, startPosition.y, 0);
                transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
                
                // Flip vers le joueur
                if (player.position.x > transform.position.x && !facingRight)
                {
                    Flip();
                }
                else if (player.position.x < transform.position.x && facingRight)
                {
                    Flip();
                }
                
                // Animation d'attaque si disponible
                if (isAttacking && walkSprites != null && walkSprites.Length > 0)
                {
                    StartAnimation(walkSprites);
                }
            }
            else
            {
                // Mode patrouille: mouvement sinusoïdal
                isAttacking = false;
                float newX = startPosition.x + Mathf.Sin(Time.time * 0.5f) * patrolRange;
                transform.position = new Vector3(newX, startPosition.y, 0);
                
                // Flip selon la direction
                float direction = newX - transform.position.x;
                if (direction > 0 && !facingRight)
                {
                    Flip();
                }
                else if (direction < 0 && facingRight)
                {
                    Flip();
                }
                
                // Animation normale
                if (!isAttacking && idleSprites != null && idleSprites.Length > 0)
                {
                    StartAnimation(idleSprites);
                }
            }
        }
        else
        {
            // Patrouille simple si pas de joueur
            float newX = startPosition.x + Mathf.Sin(Time.time * 0.5f) * patrolRange;
            transform.position = new Vector3(newX, startPosition.y, 0);
        }
    }
    
    void CheckGroundCollisions()
    {
        // Vérifier si l'ennemi est au bord d'une plateforme
        if (groundCheck != null)
        {
            bool isGrounded = Physics2D.OverlapCircle(groundCheck.position, 0.1f, groundLayer);
            if (!isGrounded)
            {
                Flip();
            }
        }
        
        // Vérifier si l'ennemi touche un mur
        if (wallCheck != null)
        {
            bool isTouchingWall = Physics2D.OverlapCircle(wallCheck.position, 0.1f, groundLayer);
            if (isTouchingWall)
            {
                Flip();
            }
        }
    }
    
    void Flip()
    {
        facingRight = !facingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }
    
    // ============ ANIMATIONS SIMPLES ============
    
    void StartAnimation(Sprite[] sprites)
    {
        if (animationCoroutine != null)
            StopCoroutine(animationCoroutine);
        
        animationCoroutine = StartCoroutine(PlayAnimation(sprites));
    }
    
    IEnumerator PlayAnimation(Sprite[] sprites)
    {
        if (sprites == null || sprites.Length == 0 || spriteRenderer == null)
            yield break;
        
        int index = 0;
        float speed = animationSpeed;
        
        // Animation plus rapide pour la marche/attaque
        if (Mathf.Abs(rb.linearVelocity.x) > 0.1f || isAttacking)
        {
            speed = animationSpeed * 0.5f;
        }
        
        while (!isDead && spriteRenderer != null)
        {
            spriteRenderer.sprite = sprites[index];
            index = (index + 1) % sprites.Length;
            yield return new WaitForSeconds(speed);
        }
    }
    
    IEnumerator HurtAnimation()
    {
        if (spriteRenderer != null)
        {
            // Flash rouge 3 fois
            Color originalColor = spriteRenderer.color;
            
            for (int i = 0; i < 3; i++)
            {
                spriteRenderer.color = Color.red;
                yield return new WaitForSeconds(0.1f);
                spriteRenderer.color = originalColor;
                yield return new WaitForSeconds(0.1f);
            }
        }
    }
    
    IEnumerator DeathAnimation()
    {
        if (spriteRenderer != null)
        {
            // Fade out
            float fadeTime = 0.5f;
            float elapsedTime = 0f;
            Color originalColor = spriteRenderer.color;
            
            while (elapsedTime < fadeTime)
            {
                elapsedTime += Time.deltaTime;
                float alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeTime);
                spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
                yield return null;
            }
            
            spriteRenderer.enabled = false;
        }
    }
    
    // ============ COMBAT ============
    
    public void GetJumpedOn()
    {
        if (isDead) return;
        
        health--;
        
        // Animation de dégâts
        StartCoroutine(HurtAnimation());
        
        if (health <= 0)
        {
            Die();
        }
    }
    
    private void Die()
    {
        if (isDead) return;
        
        isDead = true;
        
        // Arrêter l'animation courante
        if (animationCoroutine != null)
            StopCoroutine(animationCoroutine);
        
        // Animation de mort
        StartCoroutine(DeathAnimation());
        
        // Effet visuel
        if (deathEffect != null)
        {
            Instantiate(deathEffect, transform.position, Quaternion.identity);
        }
        
        // Désactiver les collisions
        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null)
            collider.enabled = false;
        
        // Comportement selon le type
        if (enemyType == EnemyType.Ground)
        {
            rb.linearVelocity = Vector2.zero;
            // Petite impulsion vers le haut
            rb.AddForce(new Vector2(0, bounceForce * 0.5f), ForceMode2D.Impulse);
        }
        else if (enemyType == EnemyType.Flying)
        {
            // L'aigle tombe
            rb.gravityScale = 2;
            rb.AddForce(new Vector2(0, -bounceForce), ForceMode2D.Impulse);
        }
        
        // Destruction
        Destroy(gameObject, 1f);
    }
    
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            bool isPlayerAbove = false;
            
            // Vérifier si le joueur vient d'en haut
            foreach (ContactPoint2D contact in collision.contacts)
            {
                if (contact.normal.y < -0.5f)
                {
                    isPlayerAbove = true;
                    
                    // Le joueur nous a sauté dessus
                    GetJumpedOn();
                    
                    // Donner un bounce au joueur
                    Rigidbody2D playerRb = collision.gameObject.GetComponent<Rigidbody2D>();
                    if (playerRb != null)
                    {
                        playerRb.linearVelocity = new Vector2(playerRb.linearVelocity.x, bounceForce);
                    }
                    break;
                }
            }
            
            // Si le joueur n'est pas au-dessus, lui infliger des dégâts
            if (!isPlayerAbove && !isDead)
            {
                move player = collision.gameObject.GetComponent<move>();
                if (player != null)
                {
                    player.TakeDamage(damage, transform.position);
                }
            }
        }
    }
    
    // Pour visualiser les zones dans l'éditeur
    void OnDrawGizmosSelected()
    {
        if (enemyType == EnemyType.Flying)
        {
            // Zone de patrouille
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(startPosition, patrolRange);
            
            // Zone d'attaque
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRange);
        }
    }
}