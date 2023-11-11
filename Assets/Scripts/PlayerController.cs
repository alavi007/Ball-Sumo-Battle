using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float speed = 5.0f;
    public bool hasPowerup;
    private float powerupStrength = 15.0f;

    private Rigidbody playerRb;
    private GameObject focalPoint;

    public GameObject powerupIndicator;

    //Smash (Line 17-25)
    public PowerUpType currentPowerUp = PowerUpType.None;
    private Coroutine powerupCountdown;

    public float hangTime;
    public float smashSpeed;
    public float explosionForce;
    public float explosionRadius;
    bool smashing = false;
    float floorY;

    // Start is called before the first frame update
    void Start()
    {
        playerRb = GetComponent<Rigidbody>();
        focalPoint = GameObject.Find("Focal Point");
    }

    // Update is called once per frame
    void Update()
    {
        float verticalInput = Input.GetAxis("Vertical");
        playerRb.AddForce(focalPoint.transform.forward * speed * verticalInput);

        powerupIndicator.transform.position = transform.position + new Vector3(0, -0.5f, 0);

        //Smash (Line 43-47)
        if (currentPowerUp == PowerUpType.Smash && Input.GetKeyDown(KeyCode.Space) && !smashing)
        {
            smashing = true;
            StartCoroutine(Smash());
            Debug.Log("Player collided with the island with powerup set to " + currentPowerUp.ToString());
        }

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Powerup"))
        {
            hasPowerup = true;

            //Smash (Line 58)
            currentPowerUp = other.gameObject.GetComponent<Powerup>().powerUpType;

            powerupIndicator.gameObject.SetActive(true);
            Destroy(other.gameObject);

            //Smash (Line 64-68)
            if (powerupCountdown != null)
            {
                StopCoroutine(powerupCountdown);
            }
            powerupCountdown = StartCoroutine(PowerupCountdownRoutine());
        }
    }

    IEnumerator PowerupCountdownRoutine()
    {
        yield return new WaitForSeconds(7);
        hasPowerup = false;

        //Smash (Line 78)
        currentPowerUp = PowerUpType.None;
        
        powerupIndicator.gameObject.SetActive(false);
    }

    private void OnCollisionEnter(Collision collision)
    {
        //Smash (Line 86 ** Only from --> currentPowerUp == PowerUpType.Pushback)
        if (collision.gameObject.CompareTag("Enemy") && currentPowerUp == PowerUpType.Pushback)
        {
            Rigidbody enemyRigidBody = collision.gameObject.GetComponent<Rigidbody>();
            Vector3 awayFromPlayer = collision.gameObject.transform.position - transform.position;

            enemyRigidBody.AddForce(awayFromPlayer * powerupStrength, ForceMode.Impulse);

            Debug.Log("Player collided with: " + collision.gameObject.name + " with powerup set to " + currentPowerUp.ToString());
        }
    }

    //Smash (Line 98-126)
    IEnumerator Smash()
    {
        var enemies = FindObjectsOfType<Enemy>();
        
        floorY = transform.position.y;
        
        float jumpTime = Time.time + hangTime;
        while (Time.time < jumpTime)
        {
            
            playerRb.velocity = new Vector2(playerRb.velocity.x, smashSpeed);
            yield return null;
        }
        
        while (transform.position.y > floorY)
        {
            playerRb.velocity = new Vector2(playerRb.velocity.x, -smashSpeed * 2);
            yield return null;
        }
        
        for (int i = 0; i < enemies.Length; i++)
        {
            
            if (enemies[i] != null)
                enemies[i].GetComponent<Rigidbody>().AddExplosionForce(explosionForce,
                transform.position, explosionRadius, 0.0f, ForceMode.Impulse);
        }
        smashing = false;
    }

}
