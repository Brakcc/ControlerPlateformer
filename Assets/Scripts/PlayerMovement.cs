using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerMovement : MonoBehaviour
{
    #region Initialisations 
    [Header("Input list")]
    [Tooltip("mouvements sur nouvel Input System")]
    [SerializeField] private InputActionReference move;
    [Tooltip("jump sur nouvel Input System")]
    [SerializeField] private InputActionReference jump;
    [Tooltip("Dash/action sur nouvel Input System")]
    [SerializeField] private InputActionReference action;
    private float moveX;
    private float moveY;
    

    [Header("Movement Parameters")]
    [Tooltip("Abusez pas vous comprenez qand même ce qu'est une vitesse de déplacement")]
    [SerializeField] private float moveSpeed;
    [Tooltip("vitesse max de chute du perso pour éviter une accélération infinie et le passage à travres les hitbox")]
    [SerializeField] private float maxVerticalSpeed;
    private float movehorizontal;
    private Vector3 velocity = Vector3.zero;
    [Tooltip("Règle l'accélération du SmoothDamp (n'y touchez pas trop c'est chiant à régler :')")]
    [SerializeField] private float SDOffset;
    private bool canMove = true;

    [Header("Jump Parameters")]
    [Tooltip("pariel sur la puissance du saut, la puissance du saut est plus basse que la vitesse de déplacement car elle utilise une échelle différente du SmoothDamp)")]
    public float jumpForce;
    private bool isGrounded;
    private bool isJumping = false;
    [Tooltip("Leger délai pour sauter après une chutte d'une plateforme")]
    [SerializeField] private float coyoteTime;
    private float coyoteTimeCounter;
    [Tooltip("Permet de sauter légèrement avant d'avoir touché le sol")]
    [SerializeField] private float jumpBufferTime;
    private float jumpBufferTimeCounter;

    [Header("Gravity Parameters")]
    [Tooltip("écehlle de gravité de base du perso, quand il est au sol, elle augmente lorqu'il a sauté, hésitez pas à faire des tests avec et la modif au max")]
    [SerializeField] private float baseOriginGravityScale;
    [Tooltip("échelle de gravité du perso en mode durci, plus élevée que celle du mode de base/fragile")]
    [SerializeField] private float hardenedOriginGravityScale;
    [Tooltip("règle la vitesse d'augmentation de la gravité une fois que le perso a quitté le sol, pareil hésitez pas à faire des tests avec")]
    [SerializeField] private float gravityScaleIncrease;
    [Tooltip("La limite max de Gravity Scale, histoire que le perso s'enfonce pas à travers la map en sautant de trop haut")]
    [SerializeField] private float gravityLimit;

    [Header("Dash Parameters")]
    [Tooltip("Puissance du Dash")]
    [SerializeField] private float dashPower;
    [Tooltip("règle la puissance horizontale différée de la puissance verticale du Dash, évitre de tomber comme un caillou directement après avoir fait un Dash horizontal")]
    [SerializeField] private Vector2 dashCompensation;
    [Tooltip("Durée du Dash, il vaut mieux la laisser faible, sinon le perso ne déscent plus pendant un moment")]
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
    [Tooltip("Permet de régler la hauteur minimale du perso nécaissaire pour planer")]
    [SerializeField] private float sizeRay;
    private RaycastHit2D hitground;
    private bool cannotFly;
    [Tooltip("La vitesse de chute du perso pendant qu'il plane")]
    [SerializeField] private float SlowFall;
    //permet de déclancher le planage en double clic de saut
    private bool canPlane = false;
    private bool isFlying = false;*/

    [Header("WallJump Parameters")]
    [Tooltip("Vitesse de déscente des murs quand accroché aux murs")]
    [SerializeField] private float grabSpeed;
    [Tooltip("vitesse horizontale lorque le perso se propulse à l'aide du mur")]
    [SerializeField] private Vector2 powerWallJump;
    [Tooltip("Permet de faire un Wall Jump après avoir laché le grab")]
    [SerializeField] private float wallJumpBuffer;
    private float wallJumpBufferCounter;
    [Tooltip("temps pendant lequel le personnage ne peut pas controler le perso sur l'horizontal, pour éviter que le perso ne se colle directement au mur et puisse Wall Climb")]
    [SerializeField] private float wallJumpDuration;
    private float wallJumpDirection;
    private bool isWallGrab;
    private bool isWallJump;
    private bool isCloseToWall;
    [Tooltip("Zone de détection du mur de droite")]
    [SerializeField] private Transform wallCheck;
    [Tooltip("Le rayon des zones de détection des murs")]
    [SerializeField] private float wallCheckRadius;
    [Tooltip("La CollisionLayer des murs")]
    [SerializeField] private LayerMask WallCollisionLayer;

    [Header("Ground Check Parameters")]
    [Tooltip("Zone de détection du sol")]
    [SerializeField] private Transform groundCheck;
    [Tooltip("Rayon de la zone de détection su sol")]
    [SerializeField] private float groundCheckRadius;
    [Tooltip("Cette CollisionLayer est la même que celle utilisée pour le flyCheck du planage !")]
    [SerializeField] private LayerMask GroundCollisionLayers;

    [Header("Hardened mode Parameters")]
    [Tooltip("vérification du mode durci")]
    public bool isHardened = false;
    /*[Tooltip("durée maxiamle du mode durci")]
    [SerializeField] private float hardenedDuration;
    private float hardenedDCounter;*/
    [Tooltip("vitesse de déplacement en mode durci")]
    [SerializeField] private float hardMoveSpeed;
    private float movehorizontalHardened;
    [Tooltip("puissance du saut quand le mode durci erst activé")]
    [SerializeField] private float hardJumpForce;
    [Tooltip("Couleur du perso en mode durci, surtout utile pour les tests avant implémentation des travaux des GAs")]
    [SerializeField] private Color hardenedColor;

    [Header("Hardened Camera Parameters")]
    [Tooltip("transform de la caméra en mode durci pour le screanshake et autre mouvements de caméra")]
    [SerializeField] private Transform cameraTransHardened;
    [Tooltip("Locale scale de la caméra en idle, sur place")]
    [SerializeField] private Vector3 notMovingCamScale;
    [Tooltip("Locale scale de la caméra en mouvement de type sprint")]
    [SerializeField] private Vector3 sprintCamScale;
    [Tooltip("durée du screen shake pour les retombées au sol en mode durci")]
    [SerializeField] private float shakeDuration;
    [Tooltip("courbe de mouvement de la caméra en screen shake")]
    [SerializeField] private AnimationCurve shakeCurve;
    [Tooltip("distance au sol minimale du perso pour déclancher le screen shake")]
    [SerializeField] private float minDistanceForScreenShake;
    [Tooltip("Taille du raycast pour la détéction au sol pour le screen shake")]
    [SerializeField] private float screenShakeRaySize;
    [Tooltip("PAS TOUCHE !!!")]
    [SerializeField] private Vector2 screenShakeRayDirection;
    private RaycastHit2D screenShakeRay;
    private bool canScreenShake;

    /*[Header("Slope Parameters")]
    [SerializeField] private float slopeRaySize;
    [SerializeField] private float maxSlopeAngle;
    private float downAngle;
    private float downAngleOld;
    private float sideAngle;
    private Vector2 slopeNormalPerpendicular;
    private bool isOnSlope;
    private bool canWalkOnSlope;
    private bool canJump;
    [SerializeField] private PhysicsMaterial2D noFriction;
    [SerializeField] private PhysicsMaterial2D fullFriction;*/

    [Header("Components")]
    [Tooltip("rb utile pour les mouvements")]
    public Rigidbody2D rb;
    [Tooltip("sprite renderer utilisé pour le sens de déplacement du personnage")]
    public SpriteRenderer spriteRenderer;
    [SerializeField] private BoxCollider2D CCol;
    private bool isFacingRight = true;
    private Vector2 colliderSize;
    #endregion

    #region instance
    public static PlayerMovement instance;

    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogWarning("Il y a plus d'une instance PlayerMovement dans la scène");
            return;
        }

        instance = this;
    }
    #endregion

    #region Update
    void Start()
    {
        colliderSize = CCol.size;
    }

    void Update()
    {
        //Vérifications de collision au solA
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, GroundCollisionLayers);
        if (isGrounded)
        {
            isJumping = false;
        }

        //Bool de vrification de collision pour le fly
        //hitground = Physics2D.Raycast(transform.position, directionRay, sizeRay, GroundCollisionLayers);
        //cannotFly = hitground.collider;

        //Bool de vérif de collision pour le ScreenShake, il est ultra petit
        screenShakeRay = Physics2D.Raycast(transform.position, screenShakeRayDirection, screenShakeRaySize, GroundCollisionLayers);

        //Bool de vérification de collision aux murs
        isCloseToWall = Physics2D.OverlapCircle(wallCheck.position, wallCheckRadius, WallCollisionLayer);

        //Raycast de verification de collision aux murs pour le Wall Jump
        //wallJumpLeft = Physics2D.Raycast(transform.position, new Vector2(-1, 0), 2, WallCollisionLayer);
        //wallJumpRight = Physics2D.Raycast(transform.position, new Vector2(1, 0), 2, WallCollisionLayer);

        //Valeur des vitesses 
        moveX = move.action.ReadValue<Vector2>().x;
        moveY = move.action.ReadValue<Vector2>().y;

        /*if (isGrounded)
        {
            movehorizontal = moveX * moveSpeed * Time.fixedDeltaTime;
            movehorizontalHardened = moveX * hardMoveSpeed * Time.fixedDeltaTime;
        }
        else
        {
            movehorizontal = moveX * apexMoveSpeed * Time.fixedDeltaTime;
            movehorizontalHardened = moveX * apexHardMoveSpeed * Time.fixedDeltaTime;
        }*/

        movehorizontal = moveX * moveSpeed * Time.fixedDeltaTime;
        movehorizontalHardened = moveX * hardMoveSpeed * Time.fixedDeltaTime;

        //vérification de la vitesse max de chute du perso
        if (rb.velocity.y >= maxVerticalSpeed)
        {
            rb.velocity = new Vector2(rb.velocity.x, maxVerticalSpeed);
        }

        //Changement de mode du perso
        /*if (Input.GetKey(downKey) && Input.GetButtonDown("Jump"))
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
        }*/

        //verif du screen shake avec condition de dépassement
        if (screenShakeRay.distance >= minDistanceForScreenShake + 0.2f)
        {
            canScreenShake = true;
        }

        //Appel des méthodes de mouvements en mode normal
        if (!isHardened)
        {
            if (canMove)
            {
                Moveperso(movehorizontal);
            }
            Jump(jumpForce);
            GravityPhysics(baseOriginGravityScale);
        }

        //Appel des méthodes de mouvements en mode durci
        if (isHardened)
        {
            if (canMove)
            {
                Moveperso(movehorizontalHardened);
            }
            GravityPhysics(hardenedOriginGravityScale);
            Jump(hardJumpForce);
            if (canScreenShake && screenShakeRay.distance < minDistanceForScreenShake && rb.velocity.y <= -20f)
            {
                canScreenShake = false;
                StartCoroutine(ScreenShake());
            }
        }

        WallGrabV2();
        WallJumpV2();

        //Appel du Dash ou action de sorti de mode durci
        if (couldDash && isGrounded)
        {
            canDash = true;
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
            spriteRenderer.color = hardenedColor;
        }

        //Gestion des Slopes
        //SlopeCheck();

        /*if (downAngle <= maxSlopeAngle)
        {
            canJump = true;
        }*/

        //Flip du sprite en fonction de la direction de déplacement
        Flip();
    }
    #endregion

    #region New Input System
    void OnEnable()
    {
        action.action.performed += PerformAction;
        action.action.canceled -= PerformAction;
    }

    private void PerformAction(InputAction.CallbackContext obj)
    {
        DashV3();
    }

    void OnDisable()
    {
        action.action.performed -= PerformAction;
    }
    #endregion

    #region Méthodes
    void Moveperso(float _horiz)
    {
        /*if (isGrounded && !isOnSlope && !isJumping)
        {
            Vector3 baseMoveVelocity = new Vector2(_horiz, 0.0f);
            rb.velocity = Vector3.SmoothDamp(rb.velocity, baseMoveVelocity, ref velocity, SDOffset);
        }
        else if (isGrounded && isOnSlope && canWalkOnSlope && !isJumping)
        {
            Vector3 baseMoveVelocity = new Vector2(_horiz * slopeNormalPerpendicular.x * -moveX, _horiz * slopeNormalPerpendicular.y * -moveX);
            rb.velocity = Vector3.SmoothDamp(rb.velocity, baseMoveVelocity, ref velocity, SDOffset);
        }
        else if (!isGrounded)
        {
            Vector3 baseMoveVelocity = new Vector2(_horiz, rb.velocity.y);
            rb.velocity = Vector3.SmoothDamp(rb.velocity, baseMoveVelocity, ref velocity, SDOffset);
        }*/
        Vector3 baseMoveVelocity = new Vector2(_horiz, rb.velocity.y);
        rb.velocity = Vector3.SmoothDamp(rb.velocity, baseMoveVelocity, ref velocity, SDOffset);

        //rb.AddForce(new Vector3(_horiz * (isGrounded? 1:mult), 0, 0));
        //rb.velocity = new Vector3(Mathf.Clamp(rb.velocity.x, -maxSpeed, maxSpeed), rb.velocity.y, 0);
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

        if (jump.action.WasPerformedThisFrame())
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

        if (jump.action.WasReleasedThisFrame() && rb.velocity.y > 0f)
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

        //ancienne méthode de gestion de la gravité pour le planage
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

    void DashV3()
    {
        if (canDash)
        {
            isDashing = true;
            couldDash = false;
            canDash = false;
            trail.emitting = true;
            dashDir = new Vector2(moveX, moveY);
            if (dashDir == Vector2.zero)
            {
                dashDir = new Vector2(transform.localScale.x, 0f);
            }
            StartCoroutine(StopDashV3());

            if (isDashing)
            {
                rb.velocity = dashPower * dashDir.normalized * dashCompensation;
                rb.gravityScale = 0f;
                return;
            }
        }
    }

    IEnumerator StopDashV3()
    {
        yield return new WaitForSeconds(dashTime);
        trail.emitting = false;
        isDashing = false;
        rb.gravityScale = baseOriginGravityScale;
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

    /*void SlopeCheck()
    {
        Vector2 checkPos = transform.position - new Vector3(0.0f, colliderSize.y / 2);

        SlopeCheckVerti(checkPos);
        SlopeCheckHoriz(checkPos);
    }

    void SlopeCheckHoriz(Vector2 checkPos)
    {
        RaycastHit2D slopeHitFront = Physics2D.Raycast(checkPos, transform.right, slopeRaySize, GroundCollisionLayers);
        RaycastHit2D slopeHitBack = Physics2D.Raycast(checkPos, -transform.right, slopeRaySize, GroundCollisionLayers);

        if (slopeHitFront)
        {
            isOnSlope = true;
            sideAngle = Vector2.Angle(slopeHitFront.normal, Vector2.up);
        }
        else if (slopeHitBack)
        {
            isOnSlope = true;
            sideAngle = Vector2.Angle(slopeHitBack.normal, Vector2.up);
        }
        else
        {
            sideAngle = 0.0f;
            isOnSlope = false;
        }
    }

    void SlopeCheckVerti(Vector2 checkPos)
    {
        RaycastHit2D hitVerti = Physics2D.Raycast(checkPos, Vector2.down, slopeRaySize, GroundCollisionLayers);

        if (hitVerti)
        {
            slopeNormalPerpendicular = Vector2.Perpendicular(hitVerti.normal);
            downAngle = Vector2.Angle(hitVerti.normal, Vector2.up);

            if (downAngle != downAngleOld)
            {
                isOnSlope = true;
            }

            downAngleOld = downAngle;

            Debug.DrawRay(hitVerti.point, slopeNormalPerpendicular, Color.red);
            Debug.DrawRay(hitVerti.point, hitVerti.normal, Color.yellow);
        }

        if (downAngle > maxSlopeAngle || sideAngle > maxSlopeAngle)
        {
            canWalkOnSlope = false;
        }
        else
        {
            canWalkOnSlope = true;
        }

        if (isOnSlope && canWalkOnSlope && moveX == 0.0f)
        {
            rb.sharedMaterial = fullFriction;
        }
        else
        {
            rb.sharedMaterial = noFriction;
        }
    }*/

    void WallGrabV2()
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
    }

    void WallJumpV2()
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
        if (jump.action.WasPerformedThisFrame() && wallJumpBufferCounter > 0f)
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

            StartCoroutine(StopWallJumpingV2());
        }
    }

    IEnumerator StopWallJumpingV2()
    {
        canMove = false;
        yield return new WaitForSeconds(wallJumpDuration);
        isWallJump = false;
        canMove = true;
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
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(wallCheck.position, wallCheckRadius);
    }
    #endregion

    #region Old fonction
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
    #endregion
}