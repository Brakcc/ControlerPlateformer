using System.Collections;
using System.ComponentModel;
using System.Xml.Linq;
using TreeEditor;
using UnityEditor.Search;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Input list")]
    [Tooltip("Touche du bas du controler")]
    [SerializeField] private KeyCode downKey = KeyCode.S;
    [Tooltip("Topuche du haut du controleur")]
    [SerializeField] private KeyCode upKey = KeyCode.Z;
    [Tooltip("Touche du Dash/autre action")]
    [SerializeField] private KeyCode dashKey = KeyCode.CapsLock;

    [Header("Movement Parameters")]
    [Tooltip("Abusez pas vous comprenez qand m�me ce qu'est une vitesse de d�placement")]
    public float moveSpeed;
    [Tooltip("vitesse max de chute du perso pour �viter une acc�l�ration infinie et le passage � travres les hitbox")]
    [SerializeField] private float maxVerticalSpeed;
    [Tooltip("pariel sur la puissance du saut, la puissance du saut est plus basse que la vitesse de d�placement car elle utilise une �chelle diff�rente du SmoothDamp)")]
    public float jumpForce;
    private float movehorizontal;
    private Vector3 velocity = Vector3.zero;
    private bool isGrounded;
    private bool isJumping = false;
    [Tooltip("R�gle l'acc�l�ration du SmoothDamp (n'y touchez pas trop c'est chiant � r�gler :')")]
    [SerializeField] private float SDOffset;
    [Tooltip("Leger d�lai pour sauter apr�s une chutte d'une plateforme")]
    [SerializeField] private float coyoteTime;
    private float coyoteTimeCounter;
    [Tooltip("Permet de sauter l�g�rement avant d'avoir touch� le sol")]
    [SerializeField] private float jumpBufferTime;
    private float jumpBufferTimeCounter;

    [Header("Gravity Parameters")]
    [Tooltip("�cehlle de gravit� de base du perso, quand il est au sol, elle augmente lorqu'il a saut�, h�sitez pas � faire des tests avec et la modif au max")]
    [SerializeField] private float baseOriginGravityScale;
    [Tooltip("�chelle de gravit� du perso en mode durci, plus �lev�e que celle du mode de base/fragile")]
    [SerializeField] private float hardenedOriginGravityScale;
    [Tooltip("r�gle la vitesse d'augmentation de la gravit� une fois que le perso a quitt� le sol, pareil h�sitez pas � faire des tests avec")]
    [SerializeField] private float gravityScaleIncrease;
    [Tooltip("La limite max de Gravity Scale, histoire que le perso s'enfonce pas � travers la map en sautant de trop haut")]
    [SerializeField] private float gravityLimit;

    [Header("Stick To Ground Parameters")]
    [Tooltip("taille minimum du perso au dessus du sol")]
    [SerializeField] private float gANCSize; 
    [Tooltip("empty")]
    [SerializeField] private RaycastHit2D groundAlignementNC;


    [Header("Dash Parameters")]
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
    private Vector2 dashDir;
    private bool couldDash = true;

    /*[Header("Planer Parameters")]
    [Tooltip("PAS TOUCHE !!!")]
    [SerializeField] private Vector2 directionRay;
    [Tooltip("Permet de r�gler la hauteur minimale du perso n�caissaire pour planer")]
    [SerializeField] private float sizeRay;
    private RaycastHit2D hitground;
    private bool cannotFly;
    [Tooltip("La vitesse de chute du perso pendant qu'il plane")]
    [SerializeField] private float SlowFall;
    //permet de d�clancher le planage en double clic de saut
    private bool canPlane = false;
    private bool isFlying = false;*/

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

    [Header("Hardened mode Parameters")]
    [Tooltip("v�rification du mode durci")]
    [SerializeField] private bool isHardened = false;
    [Tooltip("dur�e maxiamle du mode durci")]
    [SerializeField] private float hardenedDuration;
    private float hardenedDCounter;
    [Tooltip("vitesse de d�placement en mode durci")]
    [SerializeField] private float hardMoveSpeed;
    private float movehorizontalHardened;
    [Tooltip("puissance du saut quand le mode durci erst activ�")]
    [SerializeField] private float hardJumpForce;

    [Header("Hardened Camera Parameters")]
    [Tooltip("transform de la cam�ra en mode durci pour le screanshake et autre mouvements de cam�ra")]
    [SerializeField] private Transform cameraTransHardened;
    [Tooltip("Locale scale de la cam�ra en idle, sur place")]
    [SerializeField] private Vector3 notMovingCamScale;
    [Tooltip("Locale scale de la cam�ra en mouvement de type sprint")]
    [SerializeField] private Vector3 sprintCamScale;
    [Tooltip("dur�e du screen shake pour les retomb�es au sol en mode durci")]
    [SerializeField] private float shakeDuration;
    [Tooltip("courbe de mouvement de la cam�ra en screen shake")]
    [SerializeField] private AnimationCurve shakeCurve;
    [Tooltip("distance au sol minimale du perso pour d�clancher le screen shake")]
    [SerializeField] private float minDistanceForScreenShake;
    [Tooltip("Taille du raycast pour la d�t�ction au sol pour le screen shake")]
    [SerializeField] private float screenShakeRaySize;
    [Tooltip("PAS TOUCHE !!!")]
    [SerializeField] private Vector2 screenShakeRayDirection;
    private RaycastHit2D screenShakeRay;
    private bool canScreenShake;

    [Header("Components")]
    [Tooltip("rb utile pour les mouvements")]
    public Rigidbody2D rb;
    [Tooltip("sprite renderer utilis� pour le sens de d�placement du personnage")]
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
        //V�rifications de collision au solA
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, GroundCollisionLayers);
        groundAlignementNC = Physics2D.Raycast(transform.position, -transform.up, gANCSize, GroundCollisionLayers);

        //Bool de vrification de collision pour le fly
        //hitground = Physics2D.Raycast(transform.position, directionRay, sizeRay, GroundCollisionLayers);
        //cannotFly = hitground.collider;

        //Bool de v�rif de collision pour le ScreenShake, il est ultra petit
        screenShakeRay = Physics2D.Raycast(transform.position, screenShakeRayDirection, screenShakeRaySize, GroundCollisionLayers);

        //Bool de v�rification de collision aux murs
        //isCloseToWall = Physics2D.OverlapCircle(wallCheck.position, wallCheckRadius, WallCollisionLayer);
        //Raycast de verification de collision aux murs pour le Wall Jump
        //wallJumpLeft = Physics2D.Raycast(transform.position, new Vector2(-1, 0), 2, WallCollisionLayer);
        //wallJumpRight = Physics2D.Raycast(transform.position, new Vector2(1, 0), 2, WallCollisionLayer);

        //Valeur des vitesses 
        movehorizontal = Input.GetAxisRaw("Horizontal") * moveSpeed * Time.fixedDeltaTime;
        movehorizontalHardened = Input.GetAxisRaw("Horizontal") * hardMoveSpeed * Time.fixedDeltaTime;

        //v�rification de la vitesse max de chute du perso
        if (rb.velocity.y >= maxVerticalSpeed)
        {
            rb.velocity = new Vector2(rb.velocity.x, maxVerticalSpeed);
        }

        //Changement de mode du perso
        if (Input.GetKey(downKey) && Input.GetButtonDown("Jump"))
        {
            hardenedDCounter = hardenedDuration;
        }
        if (hardenedDCounter > 0f)
        {
            isHardened = true;
            hardenedDCounter -= Time.deltaTime;
        }
        else
        {
            isHardened = false;
            hardenedDCounter = -0.1f;
        }

        //verif du screen shake avec condition de d�passement
        if (screenShakeRay.distance >= minDistanceForScreenShake + 0.2f)
        {
            canScreenShake = true;
        }

        //Appel des m�thodes de mouvements en mode normal
        if (!isHardened)
        {
            Moveperso(movehorizontal);
            GravityPhysics(baseOriginGravityScale);
            Jump(jumpForce);
            if (canScreenShake && screenShakeRay.distance < minDistanceForScreenShake && rb.velocity.y <= -20f)
            {
                isJumping = false;
                canScreenShake = false;
            }
        }

        //Appel des m�thodes de mouvements en mode durci
        if (isHardened)
        {
            Moveperso(movehorizontalHardened);
            GravityPhysics(hardenedOriginGravityScale);
            Jump(hardJumpForce);
            if (canScreenShake && screenShakeRay.distance < minDistanceForScreenShake && rb.velocity.y <= -20f)
            {
                isJumping = false;
                canScreenShake = false;
                StartCoroutine(ScreenShake());
            }
        }

        //Appel du Dash ou action de sorti de mode durci
        if (couldDash && isGrounded)
        {
            canDash = true;
        }

        if (Input.GetKeyDown(dashKey) && canDash)
        {
            DashV3();
        }

        //petites couleurs sympas
        if (!isHardened && isDashing)
        {
            spriteRenderer.color = Color.green;
        }
        if (!isHardened && !isDashing)
        {
            spriteRenderer.color = Color.white;
        }
        if (isHardened)
        {
            spriteRenderer.color = Color.red;
        }

        //StickToGround();

        //Flip du sprite en fonction de la direction de d�placement
        Flip();
    }

    void Moveperso(float _horiz)
    {
        Vector3 baseMoveVelocity = new Vector2(_horiz, rb.velocity.y);
        rb.velocity = Vector3.SmoothDamp(rb.velocity, baseMoveVelocity, ref velocity, SDOffset);
    }

    void Jump(float _jumpForce)
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
            isJumping = true;
            rb.velocity = new Vector2(rb.velocity.x, _jumpForce);
            jumpBufferTimeCounter = 0f;
        }

        if (Input.GetButtonUp("Jump") && rb.velocity.y > 0f)
        {
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * 0.5f);
            coyoteTimeCounter = 0f;
        }
    }

    void GravityPhysics(float originGS)
    {
        if (isGrounded)
        {
            rb.gravityScale = originGS;
        }

        //ancienne m�thode de gestion de la gravit� pour le planage
        /*else if (!isGrounded && isFlying)
        {
            rb.gravityScale = originGS;
        }*/

        else if (!isGrounded)
        {
            rb.gravityScale += Time.deltaTime * gravityScaleIncrease;
        }

        if (rb.gravityScale >= gravityLimit)
        {
            rb.gravityScale = gravityLimit;
        }
    }

    void StickToGround()
    {
        if (groundAlignementNC.distance < gANCSize)
        {
            transform.position.Set(transform.position.x, transform.position.y + (gANCSize - groundAlignementNC.distance), transform.position.z);
        }
    }

    void DashV3()
    {
        isDashing = true;
        couldDash = false;
        canDash = false;
        trail.emitting = true;
        dashDir = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        if (dashDir == Vector2.zero)
        {
            dashDir = new Vector2(transform.localScale.x, 0f);
        }
        StartCoroutine(StopDashV3());

        if (isDashing)
        {
            rb.velocity = dashPower * dashDir.normalized;
            return;
        }
    }

    IEnumerator StopDashV3()
    {
        yield return new WaitForSeconds(dashTime);
        trail.emitting = false;
        isDashing = false;
        yield return new WaitForSeconds(dashCooldown);
        couldDash = true;
    }

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

    IEnumerator ScreenShake()

    {
        float elapsedTime = 0f;

        while (elapsedTime < shakeDuration)
        {
            Vector3 originCamPos = new Vector3(cameraTransHardened.position.x, cameraTransHardened.position.y, cameraTransHardened.position.z);
            elapsedTime += Time.deltaTime;
            float shakeStrengh = shakeCurve.Evaluate(elapsedTime / shakeDuration);
            cameraTransHardened.position = originCamPos + Random.insideUnitSphere * shakeStrengh;
            yield return null;
        }
    }

    void OnDrawGizmos()
    {
        //isGrounded
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        Gizmos.DrawLine(groundCheck.position, GetComponentInParent<Transform>().position);

        //cannotFly vercion Raycast
        //Gizmos.color = Color.green;
        //Gizmos.DrawLine(transform.position, hitground.point);

        //ScreenShake groundCheck
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, screenShakeRay.point);

        //isCloseToWall
        //Gizmos.color = Color.blue;
        //Gizmos.DrawWireSphere(wallCheck.position, wallCheckRadius);

        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, groundAlignementNC.point);
    }

    /*void DashV1()
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

    /*IEnumerator DashV2()
    {
        //start of Dash
        canDash = false;
        isDashing = true;
        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0f;
        trail.emitting = true;
        if (Input.GetButton("Horizontal"))
        {
            rb.velocity = new Vector2(Mathf.Sign(rb.velocity.x) * dashPower, 0f);
        }
        else if (Input.GetKey(downKey))
        {
            rb.velocity = new Vector2(0f, -dashPower/gravityCompensation);
        }
         else if (Input.GetKey(upKey))
        {
            rb.velocity = new Vector2(0f, dashPower*gravityCompensation*100);
        }
        yield return new WaitForSeconds(dashTime);
        //end of Dash
        isDashing = false;
        rb.gravityScale = originalGravity;
        trail.emitting = false;
        //enable Dash after cooldown
        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }*/

    /*void Planer()
    {
        if (cannotFly)
        {
            canPlane = false;
        }

        Vector3 flyVelocity = new Vector2(movehorizontal, SlowFall * Time.fixedDeltaTime);
        if (Input.GetButton("Jump") && canPlane)
        {
            rb.velocity = Vector3.SmoothDamp(rb.velocity, flyVelocity, ref velocity, SDOffset);
            isFlying = true;
        }

        if (Input.GetButtonUp("Jump") && !cannotFly)
        {
            canPlane = true;
            isFlying = false;
        }
    }*/

    /*void LeftWallJumpV1()
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

    /*void RightWallJumpV1()
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

    /*void WallGrabV2()
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

    /*void WallJumpV2()
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

    /*void StopWallJumpingV2()
    {
        isWallJump = false;
    }*/
}