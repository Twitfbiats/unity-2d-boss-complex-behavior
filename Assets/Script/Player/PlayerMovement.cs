using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private void Start()
    {
        try
        {
            Init();
            SetTimeAndCooldown();
        }
        catch (System.Exception e)
        {
            Debug.Log(e.Message);
        }
    }

    // Update is called once per frame
    // 30 fps => called every 1/30s (0.033s)
    // 1000 fps => called every 1/1000s 
    void Update()
    {
        HandleConditionMove();
        HandleConditionDirection();
        HandleConditionDash();
        HandleConditionShoot();
    }

    // FixedUpdate is called once every fixedDeltaTime (0.02s)
    public void FixedUpdate()
    {
        HandleMoving();
        HandleConditionFall();
        HandleFalling();
        HandleConditionJump();
        HandleConditionGround();
        HandleGrounding();
        HandleClimbing();
        HandleConditionAttack();
        HandleNormalShoot();
        HandleBigShoot();
    }

    public void HandleConditionGround()
    {
        isGrounded = groundCheck.IsGrounded;
    }

    public void HandleConditionMove()
    {
        if (canMove)
        {
            // Get keyboard value
            // A will be -1 and D will be 1
            horizontal = Input.GetAxisRaw("Horizontal");
            absHorizontal = Math.Abs(horizontal);
            // Change the variable inside animator
            animator.SetFloat("Speed", absHorizontal);
        } else horizontal = 0;
    }

    public void HandleConditionDirection()
    {
        // Set the direction of player only when
        // keys are typed
        if (horizontal != 0 && !isDashing)
        {
            direction = horizontal;
            HandleDirection();
        }
    }

    public void HandleConditionJump()
    {
        // Jumping
        // Check if we clicked or hold the L button
        if (Input.GetKey(KeyCode.L))
        {
            // Define how long we can hold the button
            if (maxJumpStrength - jumpStrengthTimer < 0.02)
            {
                HandleJumping(jumpStrengthTimer);
                jumpStrengthTimer = 0;
            }
            else jumpStrengthTimer += Time.fixedDeltaTime;
        }
        else if (jumpStrengthTimer != 0)
        {
            HandleJumping(jumpStrengthTimer);
            jumpStrengthTimer = 0;
        }
    }

    public void HandleConditionFall()
    {
        // Fall
        // If vertical velocity < 0, we are falling
        if (rigidbody2D.velocity.y < -0.01)
        {
            isFalling = true;
            /* if we are falling we are not jumping
            need further investigation though */
            TurnOffJumping();
        }
        else isFalling = false;
        animator.SetBool("Fall", isFalling);
    }

    public void HandleConditionDash()
    {
        // Check if we clicked the LeftShift button
        if (Input.GetKeyDown(KeyCode.LeftShift) && canDash)
        {
            // Temporarily remove gravity and every velocites
            rigidbody2D.gravityScale = 0;
            // Add a horizontal velocity to player
            rigidbody2D.velocity = new Vector2(dashVelocity*direction, 0);
            isDashing = true;
            canDash = false;
            canMove = false;
            StartCoroutine(HandleAfterImage());
            StartCoroutine(ResetDashing());
            animator.SetBool("Dash", isDashing);
        }
    }

    public void HandleConditionAttack()
    {
        /* Check if we clicked the J button,
        depend on each state we are in we 
        will have different kinds of attack */
        if (Input.GetKey(KeyCode.J))
        {
            if (isClimbing)
            {
                if (canClimbAttack)
                {
                    isClimbAttacking = true;
                    canClimbAttack = false;
                    StartCoroutine(ResetClimbAttack(false));
                    animator.SetBool("ClimbAttack", isClimbAttacking);
                }
            }
            else if (isDashing)
            {
                if (canAttack)
                {
                    isAttacking = true;
                    canAttack = false;
                    canMove = false;
                    StartCoroutine(ResetAttack(false));
                    animator.SetBool("Attack", isAttacking);
                }
            }
            else if (isJumping || isFalling)
            {
                if (canJumpAttack)
                {
                    isJumpAttacking = true;
                    canJumpAttack = false;
                    StartCoroutine(ResetJumpAttack(false));
                    animator.SetBool("JumpAttack", isJumpAttacking);
                }
            }
            else if (canAttack)
            {
                isAttacking = true;
                canAttack = false;
                canMove = false;
                StartCoroutine(ResetAttack(false));
                animator.SetBool("Attack", isAttacking);
            }
        }
    }

    public void HandleConditionShoot()
    {
        /* Check if we clicked the U key */
        if (Input.GetKeyDown(KeyCode.K))
        {
            if (canShoot) 
            {
                /* this variable will be use for blending
                shooting and big shooting animation, for
                more details:
                go to animator -> double click on
                WeakShoot state  */
                animator.SetFloat("ShootBigShootBlend", 0f);
                HandleShooting();
                HandleNormalShootEffect1();
            }
        }
    }

    public void HandleMoving()
    {
        transform.position += new Vector3(horizontal * moveSpeed * Time.fixedDeltaTime, 0);
    }

    public void HandleDirection()
    {
        /* handle player direction base on 
        direction variable */
        transform.localScale = new Vector3(direction < 0 ? 1 : -1, 1, 1);
        animator.SetFloat("Direction", direction);
    }

    public void HandleJumping(float jumpStrength)
    {
        if (canJump)
        {
            canGround = false;
            StartCoroutine(ResetGrounding());
            /* after we jump, turn off climbing
            for a while */
            StartCoroutine(ResetClimbing());
            HandleNotClimbing();

            var proportion = jumpStrength/maxJumpStrength;
            /* Add a vertical force to the player
            base on jump strength */
            rigidbody2D.AddForce(new Vector2(0, 10 * proportion), ForceMode2D.Impulse);
            /* change jump animation speed base
            on jump strength */
            jumpAnimationSpeed = 1/proportion;
            isJumping = true;
            canJump = false;
            animator.SetFloat("JumpAnimationSpeed", jumpAnimationSpeed);
            animator.SetBool("Jump", isJumping);

            /* stop climb attacking */
            StartCoroutine(ResetClimbAttack(true));
        }
    }

    public void HandleFalling()
    {
        if (isFalling)
        {
            /* make falling faster than jumping */
            rigidbody2D.velocity += new Vector2(0, fallAcceleration);
        }
    }

    public void HandleShooting()
    {
        isShooting = true;
        canShoot = false;
        /* use for blending animation */
        animator.SetFloat("ShootBlend", 1f);

        shootCoroutine = StartCoroutine(ResetShooting());
        StopCoroutine(prevShootCoroutine);
        prevShootCoroutine = shootCoroutine;

        animator.SetBool("Shoot", isShooting);
        StartCoroutine(HandleShootFlashingEffect());

        /* implement firing logic below this comment */
    }

    public IEnumerator HandleShootFlashingEffect()
    {
        /* Make the sprite color blur and then switch back */
        spriteRenderer.material.color = new Color(1, 1, 1, 0.75294f);

        yield return new WaitForSeconds(0.05f);

        spriteRenderer.material.color = new Color(1, 1, 1, 1);
    }

    public void HandleNormalShoot()
    {
        /* normal shoot state */
        if (isNormalShoot)
        {
            /* after a certain amount of time,
            if we are still holding the shooting
            key, change to big shoot state */
            if (IsHoldingKKey())
            {
                if (normalShootTimer > normalShootToBigShootTime)
                {
                    isNormalShoot = false;
                    normalShootTimer = 0;
                    isBigShoot = true;
                    HandleBigShootEffect(true);
                }
                else
                {
                    normalShootTimer += Time.fixedDeltaTime;
                }
            }
            else // if we release the key, fire normal shoot
            {
                isNormalShoot = false;
                normalShootTimer = 0;
                normalShootCharging.SetActive(false);
                HandleShooting();
                HandleNormalShootEffect1();

                /* write normal shoot logic below this line */
            }
        }
    }

    public void HandleNormalShootEffect(bool active)
    {
        normalShootCharging.SetActive(active);
    }

    public void HandleNormalShootEffect1()
    {
        /* handle some effects while shooting */
        normalShootEffect1.transform.position = transform.position;
        if (isClimbing) normalShootEffect1.transform.position += new Vector3(normalShootEffect1ClimbPosX * direction, normalShootEffect1ClimbPosY);
        else if (isDashing) normalShootEffect1.transform.position += new Vector3(normalShootEffect1DashPosX * direction, normalShootEffect1DashPosY);
        else if (absHorizontal > 0) normalShootEffect1.transform.position += new Vector3(normalShootEffect1HaveSpeedPosX * direction, normalShootEffect1HaveSpeedPosY);
        else normalShootEffect1.transform.position += new Vector3(normalShootEffect1NoSpeedPosX * direction, normalShootEffect1NoSpeedPosY);
        normalShootEffect1.SetActive(true);
    }

    public void HandleBigShoot()
    {
        if (isBigShoot)
        {
            /* if we release the key while
            we are in big shoot state,
            fire big shoot */
            if (!IsHoldingKKey())
            {
                isBigShoot = false;
                HandleNormalShootEffect(false);
                HandleBigShootEffect(false);
                animator.SetFloat("ShootBigShootBlend", 1f);
                HandleShooting();
                HandleBigShootEffect1();

                /* write big shoot logic below this line */
            }
        }
    }

    public void HandleBigShootEffect(bool active)
    {
        bigShootCharging.SetActive(active);
    }

    public void HandleBigShootEffect1()
    {
        bigShootEffect1.transform.position = transform.position;
        if (isClimbing) bigShootEffect1.transform.position += new Vector3(bigShootEffect1ClimbPosX * direction, bigShootEffect1ClimbPosY);
        else if (isDashing) bigShootEffect1.transform.position += new Vector3(bigShootEffect1DashPosX * direction, bigShootEffect1DashPosY);
        else if (absHorizontal > 0) bigShootEffect1.transform.position += new Vector3(bigShootEffect1HaveSpeedPosX * direction, bigShootEffect1HaveSpeedPosY);
        else bigShootEffect1.transform.position += new Vector3(bigShootEffect1NoSpeedPosX* direction, bigShootEffect1NoSpeedPosY);
        bigShootEffect1.SetActive(true);
    }

    public void HandleGrounding()
    {
        // Check if we are on the ground
        if (isGrounded && canGround)
        {
            /* when we are on the ground 
            we can jump */
            canJump = true;
            /* when we are on the ground we
            are not jumping */
            TurnOffJumping();
            /* stop jump attacking */
            StartCoroutine(ResetJumpAttack(true));
        }
    }

    public void HandleClimbing()
    {
        if (climbCheck.IsClimbing && canClimb)
        {
            TurnOffDashing();
            TurnOffJumping();

            isClimbing = true;
            canClimb = false;
            canDash = false;
            canJump = true;
            canMove = false;
            direction*=-1;
            HandleDirection();

            rigidbody2D.gravityScale = 0;
            rigidbody2D.velocity = Vector2.zero;

            animator.SetBool("Climb", climbCheck.IsClimbing);
        }
    }

    public void HandleNotClimbing()
    {
        isClimbing = false;
        if (!isDashing) rigidbody2D.gravityScale = defaultGravity;
        if (!isAttacking && !isDashing) canMove = true;
        canDash = true;

        animator.SetBool("Climb", false);
    }

    public void TurnOffDashing()
    {
        isDashing = false;
        rigidbody2D.velocity = Vector2.zero;
        rigidbody2D.gravityScale = defaultGravity;

        animator.SetBool("Dash", isDashing);
    }

    public void TurnOffJumping()
    {
        // Stop jumping
        isJumping = false;
        if (rigidbody2D.velocity.y > 0) rigidbody2D.velocity = new Vector2(rigidbody2D.velocity.x, 0);
        animator.SetBool("Jump", isJumping);
    }

    public IEnumerator ResetClimbing()
    {
        /* wait for some time to climb
        again */
        yield return new WaitForSeconds(climbCooldown);

        canClimb = true;
    }

    public IEnumerator ResetAttack(bool immediately)
    {
        /* Wait a little bit before we can attack
        again or reset without waiting*/
        yield return new WaitForSeconds(immediately ? 0 : attackCooldown);

        if (isAttacking)
        {
            isAttacking = false;
            canAttack = true;
            canMove = true;
            animator.SetBool("Attack", isAttacking);
        }
    }

    public IEnumerator ResetJumpAttack(bool immediately)
    {
        /* The same as attack */
        yield return new WaitForSeconds(immediately ? 0 : jumpAttackCooldown);

        if (isJumpAttacking)
        {
            isJumpAttacking = false;
            canJumpAttack = true;
            animator.SetBool("JumpAttack", isJumpAttacking);
        }
    }

    private IEnumerator ResetClimbAttack(bool immediately)
    {
        /* The same as attack */
        yield return new WaitForSeconds(immediately ? 0 : climbAttackCooldown);

        if (isClimbAttacking)
        {
            isClimbAttacking = false;
            canClimbAttack = true;
            animator.SetBool("ClimbAttack", isClimbAttacking);
        }
    }

    private IEnumerator ResetGrounding()
    {
        /* this function fix the bug when jumping
        and grounding at the same time */
        yield return new WaitForSeconds(groundCooldown);

        canGround = true;
    }

    public IEnumerator ResetDashing()
    {
        /* wait for sometime before we can dash again */
        yield return new WaitForSeconds(dashTime);

        if (isDashing)
        {
            TurnOffDashing();
            canDash = true;
            canMove = true;
        }
    }

    public IEnumerator ResetShootBlend()
    {
        yield return new WaitForSeconds(shootBlendResetTime);
        if (!isShooting) animator.SetFloat("ShootBlend", 0f);
    }

    public IEnumerator ResetShooting()
    {
        yield return new WaitForSeconds(shootCooldown);

        isShooting = false;
        canShoot = true;
        animator.SetBool("Shoot", isShooting);
        StartCoroutine(ResetShootBlend());

        yield return new WaitForSeconds(shootToNormalShootTime);

        /* after we shoot, if we are still
        holding the shooting key, change
        state to normal shoot */
        if (IsHoldingKKey())
        {
            isNormalShoot = true;
            HandleNormalShootEffect(true);
        }
    }

    public IEnumerator HandleAfterImage()
    {
        /* create effect only if we are dashing
        and the cooldown is reached */
        while(isDashing)
        {
            CreateAfterImage();

            yield return new WaitForSeconds(afterImageCooldown);
        }
    }

    public void CreateAfterImage()
    {
        /* Create effect using object pooling
        technique */
        GameObject afterImage = null;
        for (int i=0;i<numberOfAfterImages;i++)
        {
            afterImage = afterImages[i];
            if (!afterImage.activeSelf) break;
        }

        if (afterImage != null)
        {
            afterImage.GetComponent<SpriteRenderer>().sprite = spriteRenderer.sprite;
            afterImage.transform.position = transform.position;
            afterImage.transform.localScale = new Vector3(transform.localScale.x, 1, 1);
            afterImage.SetActive(true);
        }
    }

    public bool IsHoldingKKey()
    {
        if (Input.GetKey(KeyCode.K)) return true;
        else return false;
    }

    public void SetTimeAndCooldown()
    {
        /* Get the DashLeft animation time and let it be the dash time */
        var dashLeftClip = animator.runtimeAnimatorController.animationClips.First((aC) => aC.name.Equals("DashLeft_1"));
        dashTime = dashLeftClip.length;

        var attackLeftClip = animator.runtimeAnimatorController.animationClips.First((aC) => aC.name.Equals("AttackLeft_1"));
        attackCooldown = attackLeftClip.length;

        var jumpAttackLeftClip = animator.runtimeAnimatorController.animationClips.First((aC) => aC.name.Equals("JumpAttackLeft_1"));
        jumpAttackCooldown = jumpAttackLeftClip.length;

        var climbAttackLeftClip = animator.runtimeAnimatorController.animationClips.First((aC) => aC.name.Equals("ClimbAttackLeft_1"));
        climbAttackCooldown = climbAttackLeftClip.length;

        var weakShootLeftClip = animator.runtimeAnimatorController.animationClips.First((aC) => aC.name.Equals("WeakShootLeft_1"));
        shootCooldown = weakShootLeftClip.length;
    }

    public void Init()
    {
        animator = GetComponent<Animator>();
        rigidbody2D = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        groundCheck = GetComponentInChildren<GroundCheck>();
        defaultGravity = rigidbody2D.gravityScale;
        defaultGravity = rigidbody2D.gravityScale;
        canMove = true;
        defaultGravity = rigidbody2D.gravityScale;
        normalShootCharging = GameObject.Find("NormalShootCharging");
        normalShootCharging.SetActive(false);
        bigShootCharging = GameObject.Find("BigShootCharging");
        bigShootCharging.SetActive(false);
        prevShootCoroutine = StartCoroutine(NullCoroutine());
        normalShootEffect1 = GameObject.Find("NormalShootEffect1");
        normalShootEffect1.SetActive(false);
        bigShootEffect1 = GameObject.Find("BigShootEffect1");
        bigShootEffect1.SetActive(false);

        afterImages = new List<GameObject>(numberOfAfterImages);
        for (int i=0;i<numberOfAfterImages;i++)
        {
            var afterImage = Instantiate(afterImagesPrefab);
            afterImage.SetActive(false);
            afterImages.Add(afterImage);
        }
    }

    public IEnumerator NullCoroutine() {yield return new WaitForSeconds(0);}

    private float horizontal;
    private float absHorizontal;
    private float direction;
    [SerializeField] private float moveSpeed = 5f;
    private bool canMove = true;
    [SerializeField] private bool isDashing;
    private bool canDash = true;
    /* This will be equal to the dash animation time,
    if you want to change it change the animation
    time itself*/
    private float dashTime;
    /* Unity doesn't allow exposing private variables
    to the Inspector, if we want to do that 
    anyway, we can use [SerializeField] */
    [SerializeField] private float dashVelocity = 20f;
    /* Create some effects after dash using object 
    pooling technique*/
    public List<GameObject> afterImages;
    [SerializeField] private GameObject afterImagesPrefab;
    [SerializeField] private short numberOfAfterImages = 10;
    [SerializeField] private float afterImageCooldown = 0.01f;
    private bool isJumping;
    private bool canJump = true;
    [SerializeField] private float maxJumpStrength = 0.2f;
    private float jumpStrengthTimer = 0;
    private float jumpAnimationSpeed;
    private bool isFalling;
    // fall will be faster depend on this
    [SerializeField] private float fallAcceleration = -0.1f;
    [SerializeField] private GroundCheck groundCheck;
    // check if we are on the ground
    [SerializeField] private bool isGrounded;
    private bool canGround = true;
    [SerializeField] private float groundCooldown = 0.2f;
    [SerializeField] private ClimbCheck climbCheck;
    [SerializeField] private bool isClimbing;
    private bool canClimb = true;
    [SerializeField] private float climbCooldown = 0.2f;
    private bool isAttacking = false;
    private bool canAttack = true;
    private float attackCooldown;
    private bool isJumpAttacking = false;
    private bool canJumpAttack = true;
    private float jumpAttackCooldown;
    private bool isClimbAttacking = false;
    private bool canClimbAttack = true;
    private float climbAttackCooldown;
    private bool isShooting = false;
    private bool canShoot = true;
    private float shootCooldown;
    private Coroutine shootCoroutine;
    private Coroutine prevShootCoroutine;
    public float shootBlendResetTime = 0.2f;
    [SerializeField] private float shootToNormalShootTime = 0.3f;
    private bool isNormalShoot = false;
    [SerializeField] private float normalShootToBigShootTime = 0.7f;
    private float normalShootTimer = 0f;
    private GameObject normalShootCharging;
    private float normalShootEffect1NoSpeedPosX = 1.43f;
    private float normalShootEffect1NoSpeedPosY = 0.67f;
    private float normalShootEffect1HaveSpeedPosX = 1.97f;
    private float normalShootEffect1HaveSpeedPosY = 0.72f;
    private float normalShootEffect1ClimbPosX = 1.85f;
    private float normalShootEffect1ClimbPosY = 0.47f;
    private float normalShootEffect1DashPosX = 2.44f;
    private float normalShootEffect1DashPosY = 0.13f;
    private float bigShootEffect1NoSpeedPosX = 1f;
    private float bigShootEffect1NoSpeedPosY = 0.66f;
    private float bigShootEffect1HaveSpeedPosX = 1.43f;
    private float bigShootEffect1HaveSpeedPosY = 0.66f;
    private float bigShootEffect1ClimbPosX = 1.41f;
    private float bigShootEffect1ClimbPosY = 0.47f;
    private float bigShootEffect1DashPosX = 1.55f;
    private float bigShootEffect1DashPosY = 0.43f;
    private bool isBigShoot = false;
    private GameObject bigShootCharging;
    private Animator animator;
    private new Rigidbody2D rigidbody2D;
    private SpriteRenderer spriteRenderer;
    private float defaultGravity;
    private GameObject normalShootEffect1;
    private GameObject bigShootEffect1;
}
