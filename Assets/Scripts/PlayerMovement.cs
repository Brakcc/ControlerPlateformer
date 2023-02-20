using System.Collections;
using System.ComponentModel;
using TreeEditor;
using UnityEditor.Search;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Parameters")]
    [Tooltip("Abusez pas vous comprenez qand m�me ce qu'est une vitesse de d�placement")]
    public float moveSpeed;
    [Tooltip("pariel sur la puissance du saut, la puissance du saut est plus basse que la vitesse de d�placement car elle utilise une �chelle diff�rente du SmoothDamp)")]
    public float jumpForce;    
    private float movehorizontal;
    private float movevertical;
    private Vector3 velocity = Vector3.zero;
    private bool isGrounded;
    [Tooltip("R�gle l'acc�l�ration du SmoothDamp (n'y touchez pas trop c'est chiant � r�gler :')")]
    [SerializeField] private float SDOffset;
    [Tooltip("Leger d�lai pour sauter apr�s une chutte d'une plateforme")]
    [SerializeField] private float coyoteTime;
    private float coyoteTimeCounter;
    [Tooltip("Permet de sauter l�g�rement avant d'avoir touch� le sol")]
    [SerializeField] private float jumpBufferTime;
    private float jumpBufferTimeCounter;
    

    [Header("Dash Parameters")]
    [Tooltip("La touche du Dash/autre action")]
    [SerializeField] private KeyCode dashKey = KeyCode.CapsLock;
    [Tooltip("Puissance du Dash")]
    [SerializeField] private float dashPower;
    [Tooltip("Dur�e du Dash, il vaut mieux la laisser faible, sinon le perso ne d�scent plus pendant un moment")]
    [SerializeField] private float dashTime;
    [Tooltip("Le cooldown du Dash")]
    [SerializeField] private float dashCooldown;
    private bool canDash = true;
    private bool isDashing = false;
    [Tooltip("La Trail pendant le Dash")]
    [SerializeField] private TrailRenderer trail;

    [Header("Planer Parameters")]
    [Tooltip("PAS TOUCHE !!!")]
    [SerializeField] private Vector2 directionRay;
    [Tooltip("Permet de r�gler la hauteur minimale du perso n�caissaire pour planer")]
    [SerializeField] private float sizeRay;
    private RaycastHit2D hitground;
    private bool cannotFly;
    [Tooltip("Place la vitesse du RigidBody � 0 � chaque frame pour ralentir le perso en lui for�ant une vitesse aditionn�e � la gravit�")]
    [SerializeField] private float SlowFall;
    //permet de d�clancher le planage en double clic de saut
    private bool canPlane = false;

    /*[Header("WallJump Parameters")]
    [Tooltip("Vitesse de d�scente des murs quand accroch� aux murs")]
    [SerializeField] private float grabSpeed;
    [Tooltip("vitesse horizontale lorque le perso se propulse � l'aide du mur")]
    [SerializeField] private Vector2 powerWallJump;
    [Tooltip("Permet de faire un Wall Jump apr�s avoir lach� le grab")]
    [SerializeField] private float wallJumpBuffer;
    private float wallJumpBufferCounter;
    [SerializeField] private float wallJumpDuration;
    private float wallJumpDirection;
    private bool isWallGrab;
    private bool isWallJump;
    private bool isCloseToWall;
    [Tooltip("Zone de d�tection du mur de droite")]
    [SerializeField] private Transform wallCheck;
    [Tooltip("Le rayon des zones de d�tection des murs")]
    [SerializeField] private float wallCheckRadius;
    [Tooltip("La CollisionLayer des murs")]
    [SerializeField] private LayerMask WallCollisionLayer;
    //[Tooltip("Touche pour se d�placer vers la gauche (uniquement utilis�e pour les WallJumps")]
    //[SerializeField] private KeyCode leftKey = KeyCode.Q;
    //[Tooltip("Touche pour se d�placer vers la droite (uniquement utilis�e pour les WallJumps")]
    //[SerializeField] private KeyCode rightKey = KeyCode.D;
    //[Tooltip("Vitesse verticale quand coll� � un mur")]
    //[SerializeField] private float wallJumpForce;
    //[Tooltip("l�ger recul quand le perso monte le mur � chaque saut, ajoute un peu de r�alisme")]
    //[SerializeField] private float recoilWallJump;
    //private RaycastHit2D wallJumpLeft;
    //private RaycastHit2D wallJumpRight;*/

    [Header("Ground Check Parameters")]
    [Tooltip("Zone de d�tection du sol")]
    [SerializeField] private Transform groundCheck;
    [Tooltip("Rayon de la zone de d�tection su sol")]
    [SerializeField] private float groundCheckRadius;
    [Tooltip("Cette CollisionLayer est la m�me que celle utilis�e pour le flyCheck du planage !")]
    [SerializeField] private LayerMask GroundCollisionLayers;

    [Header("Components")]
    public Rigidbody2D rb;
    public SpriteRenderer spriteRenderer;
    private bool isFacingRight = true;

    public static PlayerMovement instance;

    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogWarning("Il y a plus d'une instance PlayerMovement dans la sc�ne");
            return;
        }

        instance = this;
    }

    void Update()
    {
        //Bool de v�rification de collision au sol
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, GroundCollisionLayers);

        //Bool de vrification de collision pour le fly
        hitground = Physics2D.Raycast(transform.position, directionRay, sizeRay, GroundCollisionLayers);
        cannotFly = hitground.collider;

        //Bool de v�rification de collision aux murs
        //isCloseToWall = Physics2D.OverlapCircle(wallCheck.position, wallCheckRadius, WallCollisionLayer);

        //Raycast de verification de collision aux murs pour le Wall Jump
        //wallJumpLeft = Physics2D.Raycast(transform.position, new Vector2(-1, 0), 2, WallCollisionLayer);
        //wallJumpRight = Physics2D.Raycast(transform.position, new Vector2(1, 0), 2, WallCollisionLayer);

        //Valeur des vitesses 
        movehorizontal = Input.GetAxisRaw("Horizontal") * moveSpeed * Time.fixedDeltaTime;

        //Appel des m�thodes de mouvements
        Moveperso(movehorizontal);
        Jump();
        //WallGrab();
        //WallJump();
        Planer();
        if (Input.GetKeyDown(dashKey) && canDash)
        {
            StartCoroutine(Dash());
        }
        Flip();
    }

    void Moveperso(float _horiz)
    {
        Vector3 baseMoveVelocity = new Vector2(_horiz, rb.velocity.y);
        rb.velocity = Vector3.SmoothDamp(rb.velocity, baseMoveVelocity, ref velocity, SDOffset);
    }

    void Jump()
    {
        if (isGrounded)
        {
            coyoteTimeCounter = coyoteTime;
        }

        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }

        if (Input.GetButtonDown("Jump"))
        {
            jumpBufferTimeCounter = jumpBufferTime;
        }

        else
        {
            jumpBufferTimeCounter -= Time.deltaTime;
        }

        if (coyoteTimeCounter > 0f && jumpBufferTimeCounter > 0f)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            jumpBufferTimeCounter = 0f;
        }

        if (Input.GetButtonUp("Jump") && rb.velocity.y > 0f)
        {
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * 0.5f);
            coyoteTimeCounter = 0f;
        }
    }

    /*void Dash()
    {
        Vector2 dashDir = new Vector2(dashPower * Mathf.Sign(rb.velocity.x), dashPowerCompensation);
        if (canDash >= dashCooldown)
        {
            canDash = dashCooldown;
        }

        else
        {
            canDash += Time.deltaTime;
        }

        if (Input.GetKeyDown(dashKey) && canDash == dashCooldown)
        {
            rb.AddForce(dashDir);
            canDash = 0f;
        }
    }*/

    private IEnumerator Dash()
    {
        //start of Dash
        canDash = false;
        isDashing = true;
        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0f;
        trail.emitting = true;
        rb.velocity = new Vector2(Mathf.Sign(rb.velocity.x) * dashPower, 0f);
        yield return new WaitForSeconds(dashTime);
        //end of Dash
        isDashing = false;
        rb.gravityScale = originalGravity;
        trail.emitting = false;
        //enable Dash after cooldown
        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

    void Planer()
    {
        if (cannotFly)
        {
            canPlane = false;
        }
        
        Vector3 flyVelocity = new Vector2(movehorizontal, SlowFall);
        if (Input.GetButton("Jump") && canPlane)
        {
            rb.velocity = Vector3.SmoothDamp(rb.velocity, flyVelocity, ref velocity, SDOffset);
        }

        if (Input.GetButtonUp("Jump") && !cannotFly)
        {
            canPlane = true;
        }
    }

    /*void LeftWallJumpOld()
    {
        Vector3 grabVelocity = new Vector2(movehorizontal, grabSpeed);
        if (Input.GetButton("Horizontal") && isCloseToLeftWall && !cannotFly)
        {
            rb.velocity = Vector3.SmoothDamp(rb.velocity, grabVelocity, ref velocity, SDOffset);
        }

        if (Input.GetKeyUp(leftKey))
        {
            wallJumpBufferCounter = wallJumpBuffer; 
        }

        if (wallJumpLeft.collider)
        {
            wallJumpBufferCounter -= Time.deltaTime;
        }

        if (Input.GetKey(leftKey) && isCloseToLeftWall && !cannotFly && Input.GetButtonDown("Jump"))
        {
            rb.velocity = new Vector2(recoilWallJump, wallJumpForce);
            wallJumpBufferCounter = wallJumpBuffer;
        }

        if (wallJumpBufferCounter >= 0f && wallJumpLeft.collider && !cannotFly && Input.GetButtonDown("Jump") && Input.GetKey(rightKey))
        {
            rb.velocity = new Vector2(propulseWallJump, wallJumpForce*2);
            wallJumpBufferCounter = wallJumpBuffer;
        }
    }*/

    /*void RightWallJumpOld()
    {
        Vector3 grabVelocity = new Vector2(movehorizontal, grabSpeed);
        if (Input.GetButton("Horizontal") && isCloseToRightWall && !cannotFly)
        {
            rb.velocity = Vector3.SmoothDamp(rb.velocity, grabVelocity, ref velocity, SDOffset);
        }

        if (Input.GetKeyUp(rightKey))
        {
            wallJumpBufferCounter = wallJumpBuffer;
        }

        if (wallJumpRight.collider)
        {
            wallJumpBufferCounter -= Time.deltaTime;
        }

        if (Input.GetKey(rightKey) && isCloseToRightWall && !cannotFly && Input.GetButtonDown("Jump"))
        {
            rb.velocity = new Vector2(-recoilWallJump, wallJumpForce);
            wallJumpBufferCounter = wallJumpBuffer;
        }

        if (wallJumpBufferCounter >= 0f && isCloseToRightWall && !cannotFly && Input.GetButtonDown("Jump") && Input.GetKey(leftKey))
        {
            rb.velocity = new Vector2(-propulseWallJump, wallJumpForce * 2);
            wallJumpBufferCounter = wallJumpBuffer;
        }
    }*/

    /*void WallGrab()
    {
        if (isCloseToWall && !isGrounded && movehorizontal != 0f)
        {
            isWallGrab = true;
            rb.velocity = new Vector2(rb.velocity.x, Mathf.Clamp(rb.velocity.y, -grabSpeed, float.MaxValue));
        }
        else
        {
            isWallGrab = false;
        }
    }*/

    /*void WallJump()
    {
        if (isWallGrab)
        {
            isWallJump = false;
            wallJumpDirection = -transform.localScale.x;
            wallJumpBufferCounter = wallJumpBuffer;
        }
        else
        {
            wallJumpBufferCounter -= Time.deltaTime;
        }

        if(Input.GetButtonDown("Jump") && wallJumpBufferCounter > 0f)
        {
            isWallJump = true;
            rb.velocity = new Vector2(wallJumpDirection * powerWallJump.x, powerWallJump.y);
            wallJumpBufferCounter = 0f;

            if (transform.localScale.x != wallJumpDirection)
            {
                isFacingRight = !isFacingRight;
                Vector3 localScale = transform.localScale;
                localScale.x *= -1f;
                transform.localScale = localScale;
            }

            Invoke(nameof(StopWallJumping), wallJumpDuration);
        }
    }*/

    /*void StopWallJumping()
    {
        isWallJump = false;
    }*/

    void Flip()
    {
        if (isFacingRight && movehorizontal < 0f || !isFacingRight && movehorizontal > 0f)
        {
            isFacingRight = !isFacingRight;
            Vector3 localScale = transform.localScale;
            localScale.x *= -1f;
            transform.localScale = localScale;
        }
    }

    private void OnDrawGizmos()
    {
        //isGrounded
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        Gizmos.DrawLine(groundCheck.position, GetComponentInParent<Transform>().position);

        //cannotFly vercion Raycast
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, hitground.point);

        //isCloseToWall
        //Gizmos.color = Color.blue;
        //Gizmos.DrawWireSphere(wallCheck.position, wallCheckRadius);
    }
}