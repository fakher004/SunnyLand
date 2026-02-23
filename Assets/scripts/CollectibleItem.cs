using UnityEngine;

public class Collectibleitem : MonoBehaviour
{
    [Header("Type")]
    public bool isDiamond = true;
    
    [Header("Valeurs")]
    public int scoreValue = 10;
    
    [Header("Animation")]
    public float rotationSpeed = 100f;
    public float floatHeight = 0.3f;
    
    private Vector3 startPosition;
    
    void Start()
    {
        startPosition = transform.position;
        
        // Définir le tag
        if (isDiamond)
        {
            gameObject.tag = "Diamond";
            Debug.Log($"Diamond created - Score Value: {scoreValue}");
        }
        else
        {
            gameObject.tag = "Cherry";
            Debug.Log($"Cherry created - Score Value: {scoreValue}");
        }
        
        // S'assurer d'avoir un collider trigger
        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null)
        {
            collider.isTrigger = true;
        }
        else
        {
            gameObject.AddComponent<CircleCollider2D>().isTrigger = true;
        }
    }
    
    void Update()
    {
        FloatAnimation();
        
        if (isDiamond)
        {
            transform.Rotate(0, rotationSpeed * Time.deltaTime, 0);
        }
    }
    
    void FloatAnimation()
    {
        float newY = startPosition.y + Mathf.Sin(Time.time * 2f) * floatHeight;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log($"Collectible ({gameObject.name}) touched by Player");
            Collect(other.gameObject);
        }
    }
    
    void Collect(GameObject player)
    {
        Debug.Log($"Collecting {gameObject.name} - IsDiamond: {isDiamond}, ScoreValue: {scoreValue}");
        
        move playerScript = player.GetComponent<move>();
        if (playerScript != null)
        {
            if (isDiamond)
            {
                // DIAMANT: +scoreValue ET +1 diamant
                Debug.Log($"Diamond: Adding {scoreValue} points and 1 diamond");
                playerScript.AddScore(scoreValue);
                playerScript.AddDiamond(1);
            }
            else
            {
                // CERISE: +scoreValue SEULEMENT
                Debug.Log($"Cherry: Adding {scoreValue} points only");
                playerScript.AddScore(scoreValue);
                // PAS de AddDiamond() pour les cerises!
            }
        }
        else
        {
            Debug.LogError("move script not found on player!");
        }
        
        // Désactiver visuellement
        SpriteRenderer renderer = GetComponent<SpriteRenderer>();
        if (renderer != null)
            renderer.enabled = false;
        
        // Désactiver collider
        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null)
            collider.enabled = false;
        
        // Détruire après 0.3 secondes
        Destroy(gameObject, 0.3f);
    }
}