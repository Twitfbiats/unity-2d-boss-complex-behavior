using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityRandom = UnityEngine.Random;
using SystemRandom = System.Random;

public class BossScript : MonoBehaviour
{
    [SerializeField] private GameObject target;
    private Animator animator;
    public ScytheScript scytheScript;
    public CatcherScript catcherScript;
    public GameObject spinningEffect;
    private new Rigidbody2D rigidbody2D;
    public ScreenEffect screenEffect;
    [SerializeField] private bool isMoving = false;
    [SerializeField] private bool canMove = true;
    [SerializeField] private bool stopMove = false;
    [SerializeField] private int direction;
    private bool directionUpdatable = true;
    [SerializeField] public float moveSpeed = 5f;
    private float waitMoveTime = 1f;
    private Coroutine waitMoveCoroutine;
    private float attackRange = 8f;
    private bool canAttack = true;
    private float attackLength;
    private bool attack = false;
    private bool stopAttack = false;
    private float attackCooldown = 2.5f;
    private byte attack1Chance = 30;
    private bool attack1 = false;
    private bool stopAttack1 = false;
    private float attack1Length;
    private byte attack2Chance = 30;
    private bool attack2 = false;
    private float attack2Length;
    private bool stopAttack2 = false;
    private byte throwChance = 75;
    [SerializeField] private bool inAction = false;
    private enum ActionEnum {Attack, Throw, Spin, Flash, Swing, Attack3, Attack4}
    private int actionEnumLength;
    private bool throwScythe = false;
    private bool startThrowScythe = false;
    private bool handleThrowScythe = false;
    private bool handleThrowScytheTrigger1 = true;
    private bool stopThrowScythe = false;
    private bool canThrow = true;
    private float throwScyteCooldown = 6f;
    private bool catchScythe = false;
    private float catchLength;
    private bool stopCatch = false;
    private bool canSpin = true;
    private bool spin = false;
    private bool handleSpin = false;
    private float spinDistance;
    private enum DirectionEnum {left = -1, right = 1}
    private DirectionEnum spinMoveDirection;
    private float spinMoved = 0;
    private float spinMoveAmmount = 0;
    public float spinMoveSpeed = 30f;
    private bool stopSpin = false;
    private float spinCooldown = 3f;
    private byte spinChance = 50;
    private bool canFlash = true;
    private byte flashChance = 50;
    private bool isFalling;
    private float flashLength;
    private float flashCooldown = 3f;
    private float swingDistance = 20f;
    private bool canSwing = true;
    private byte swingChance = 50;
    private bool handleSwing = false;
    private float swingMoveAmount = 0;
    private float swingLength;
    private float swingCooldown = 3f;
    private float attack3Distance = 20f;
    private bool canAttack3 = true;
    private byte attack3Chance = 50;
    private float attack3Cooldown = 3f;
    private float attack3HoldLength;
    private float attack3WaitNextLength = 2f;
    private float attack3StartLength;
    private bool attack3LogicBool = false;
    private Coroutine nextAttack3Coroutine;
    public VortexMidPointScript vortexMidPointScript;
    private bool canChain = true;
    private float chainCooldown = 1.5f;
    public VortexScript vortexScript;
    private AnimatedObject animatedObject = new AnimatedObject();
    public BossPortalScript bossPortalScript;
    public byte bossPortalChance = 75;
    public float bossPortalCooldown = 3f;
    public bool canBossPortal = true;
    public float suctionValue = 0.1f;
    public KunaiObjectPool kunaiObjectPool;
    private bool canAttack4 = true;
    private byte attack4Chance = 50;
    private byte attack4CurrentFire = 0;
    private float[] attack4FireSpeedPatern = new float[6] {50, 50, 100, 100 ,200, 0};
    private float[] attack4AnimationSpeedModifiers = new float[6] {1, 1, 0.75f, 0.75f, 0.5f, 0};
    private float attack4AnimationSpeedModifier;
    private float attack4Cooldown = 5f;

    // Start is called before the first frame update
    void Start()
    {
        animator = gameObject.GetComponent<Animator>();
        rigidbody2D = gameObject.GetComponent<Rigidbody2D>();
        attackLength = GetAnimationClipLength(this.animator, "Attack");
        attack1Length = GetAnimationClipLength(this.animator, "Attack1");
        attack2Length = GetAnimationClipLength(this.animator, "Attack2");
        catchLength = GetAnimationClipLength(this.animator, "Catch");
        flashLength = GetAnimationClipLength(this.animator, "Flash");
        swingLength = GetAnimationClipLength(this.animator, "Swing");
        attack3HoldLength = GetAnimationClipLength(this.animator, "Attack3_Hold");
        attack3StartLength = GetAnimationClipLength(this.animator, "Attack3_Start");
        waitMoveCoroutine = StartCoroutine(NullCoroutine());
        actionEnumLength = Enum.GetValues(typeof(ActionEnum)).Length;
        ChainScript.catchAtTheEnd += NextAttack3Early;
        kunaiObjectPool = GetComponentInChildren<KunaiObjectPool>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void FixedUpdate() 
    {
        UpdateDirection();
        UpdateScaling();
        IsFalling();

        CheckMoving();
        MoveToTarget();

        BossAction();
        Attack();
        Attack2();
        Attack1();
        ThrowScythe();
        HandleThrowScythe();
        CatchScythe();
        Spin();
        HandleSpin();
        HandleSwing();
    }

    // Specify problems in here only
    private void Problem()
    {
        
    }

    private void BossAction()
    {
        if (!inAction && !isFalling)
        {
            ActionEnum actionEnum = (ActionEnum)UnityRandom.Range(0, actionEnumLength);
            var distanceToTarget = CalculateDistance(target);
            var inAttackRange = distanceToTarget < attackRange;

            switch (actionEnum)
            {
                case ActionEnum.Attack:
                {
                    if (inAttackRange)
                    {
                        if (canAttack)
                        {
                            canAttack = false;
                            attack = true;
                            inAction = true;     
                        }
                    }

                    break;
                }
                case ActionEnum.Throw:
                {
                    if (!inAttackRange && canThrow)
                    {
                        if (HaveChance(throwChance))
                        {
                            canThrow = false;
                            throwScythe = true;
                            inAction = true;
                        }
                        else
                        {
                            canThrow = false;
                            StartCoroutine(ResetThrow());
                        }
                    }
            
                    break;
                }
                case ActionEnum.Spin:
                {
                    if (canSpin)
                    {
                        if (HaveChance(spinChance))
                        {
                            spin = true;
                            inAction = true;
                            canSpin = false;
                        }
                        else
                        {
                            canSpin = false;
                            StartCoroutine(ResetSpin());
                        }
                    }

                    break;
                }
                case ActionEnum.Flash:
                {
                    if (!inAttackRange && canFlash)
                    {
                        if (HaveChance(flashChance))
                        {
                            Flash();
                            inAction = true;
                            canFlash = false;
                        }
                        else
                        {
                            canFlash = false;
                            StartCoroutine(ResetFlash());
                        }
                    }

                    break;
                }
                case ActionEnum.Swing:
                {
                    if (distanceToTarget > swingDistance && canSwing)
                    {
                        if (HaveChance(swingChance))
                        {
                            canSwing = false;
                            inAction = true;
                            Swing();
                        }
                        else
                        {
                            canSwing = false;
                            StartCoroutine(ResetSwing());
                        }
                    }

                    break;
                }
                case ActionEnum.Attack3:
                {
                    if (distanceToTarget > attack3Distance && canAttack3)
                    {
                        if (HaveChance(attack3Chance))
                        {
                            inAction = true;
                            canAttack3 = false;
                            Attack3();
                        }
                        else
                        {
                            canAttack3 = false;
                            StartCoroutine(ResetAttack3());
                        }
                    }
                    break;
                }
                case ActionEnum.Attack4:
                {
                    if (!inAttackRange && canAttack4)
                    {
                        if (HaveChance(attack4Chance))
                        {
                            inAction = true;
                            canAttack4 = false;
                            Attack4();
                        }
                        else
                        {
                            canAttack4 = false;
                            StartCoroutine(ResetAttack4());
                        }
                    }

                    break;
                }
                default: break;
            }
        }
    }

    public void Attack4()
    {
        StopMove();
        attack4CurrentFire = 0;
        attack4AnimationSpeedModifier = attack4AnimationSpeedModifiers[0];
        animator.SetFloat("Attack4AnimationSpeedModifier", attack4AnimationSpeedModifier);

        animator.SetBool("Attack4", true);
    }

    public void Attack4Logic()
    {
        kunaiObjectPool.FireOne(attack4FireSpeedPatern[attack4CurrentFire++]);
    }

    public void Attack4EndLogic()
    {
        attack4AnimationSpeedModifier = attack4AnimationSpeedModifiers[attack4CurrentFire];
        animator.SetFloat("Attack4AnimationSpeedModifier", attack4AnimationSpeedModifier);

        if (attack4CurrentFire == attack4FireSpeedPatern.Length - 1)
        {
            animator.SetBool("Attack4", false);
            inAction = false;
            waitMoveCoroutine = StartCoroutine(WaitMove());
            StartCoroutine(ResetAttack4());
        }
    }

    public IEnumerator ResetAttack4()
    {
        yield return new WaitForSeconds(attack4Cooldown);

        canAttack4 = true;
    }

    public void Attack3()
    {
        StopMove();
        vortexScript.Active(true);
        nextAttack3Coroutine = StartCoroutine(NextAttack3());

        animator.SetBool("Attack3_Hold", true);
        StartCoroutine(Attack3Logic());
    }

    public IEnumerator NextAttack3()
    {
        yield return new WaitForSeconds(attack3HoldLength + attack3WaitNextLength);
        NextAttack3Logic();
    }

    public void NextAttack3Logic()
    {
        animator.SetBool("Attack3_Start", true);
        animator.SetBool("Attack3_Hold", false);
        StartCoroutine(StopAttack3Start());   
    }

    public void NextAttack3Early()
    {
        if (attack3LogicBool)
        {
            StopCoroutine(nextAttack3Coroutine);
            NextAttack3Logic();
        }
    }

    public IEnumerator StopAttack3Start()
    {
        yield return new WaitForSeconds(attack3StartLength);

        animator.SetBool("Attack3_Start", false);
        inAction = false;
        vortexScript.Active(false);
        waitMoveCoroutine = StartCoroutine(WaitMove());
        attack3LogicBool = false;

        StartCoroutine(ResetAttack3());
    }

    public IEnumerator ResetAttack3()
    {
        yield return new WaitForSeconds(attack3Cooldown);

        canAttack3 = true;
    }

    public IEnumerator Attack3Logic()
    {
        attack3LogicBool = true;
        while (attack3LogicBool)
        {
            yield return new WaitForSeconds(Time.fixedDeltaTime);

            target.transform.position -= direction * new Vector3(suctionValue, 0, 0);

            if (canBossPortal)
            {
                if (HaveChance(bossPortalChance))
                {
                    bossPortalScript.StartPortal();
                }
                
                canBossPortal = false;
                StartCoroutine(ResetBossPortal());
            }

            if (canChain)
            {
                vortexMidPointScript.transform.position = transform.position;
                vortexMidPointScript.Begin();
                canChain = false;
                StartCoroutine(ResetChain());
            }
        }
    }

    public IEnumerator ResetChain()
    {
        yield return new WaitForSeconds(chainCooldown);

        canChain = true;
    }

    public IEnumerator ResetBossPortal()
    {
        yield return new WaitForSeconds(bossPortalCooldown);

        canBossPortal = true;
    }

    public void Swing()
    {
        swingMoveAmount = direction * swingDistance * Time.fixedDeltaTime;
        directionUpdatable = false;
        StartCoroutine(StopSwing());
        StopMove();

        handleSwing = true;

        animator.SetBool("Swing", true);
    }

    public void HandleSwing()
    {
        if (handleSwing)
        {
            transform.position += new Vector3(swingMoveAmount, 0);
        }
    }

    public IEnumerator StopSwing()
    {
        yield return new WaitForSeconds(swingLength);

        animator.SetBool("Swing", false);
        directionUpdatable = true;
        handleSwing = false;
        inAction = false;
        waitMoveCoroutine = StartCoroutine(WaitMove());

        StartCoroutine(ResetSwing());
    }

    public IEnumerator ResetSwing()
    {
        yield return new WaitForSeconds(swingCooldown);

        canSwing = true;
    }

    public void Flash()
    {
        StopMove();
        StartCoroutine(StopFlash());
        transform.position = target.transform.position + new Vector3(2,3);

        animator.SetBool("Flash", true);
    }

    public IEnumerator StopFlash()
    {
        yield return new WaitForSeconds(flashLength);
        animator.SetBool("Flash", false);

        inAction = false;
        waitMoveCoroutine = StartCoroutine(WaitMove());
        
        StartCoroutine(ResetFlash());
    }

    public IEnumerator ResetFlash()
    {
        yield return new WaitForSeconds(flashCooldown);
        canFlash = true;
    }

    public void Spin()
    {
        if (spin)
        {
            spin = false;
            spinMoved = 0;
            spinDistance = target.transform.position.x - transform.position.x;
            spinMoveDirection = spinDistance > 0 ? DirectionEnum.right : DirectionEnum.left;
            spinMoveAmmount = spinMoveSpeed * (int)spinMoveDirection * Time.fixedDeltaTime;
            handleSpin = true;

            StopMove();

            spinningEffect.SetActive(true);
            animator.SetBool("Spin", true);
        }

        if (stopSpin)
        {
            stopSpin = false;

            spinningEffect.SetActive(false);
            animator.SetBool("Spin", false);
        }
    }

    public void HandleSpin()
    {
        if (handleSpin)
        {
            if (Math.Abs(spinMoved - spinDistance) > Math.Abs(spinMoveAmmount))
            {
                spinMoved += spinMoveAmmount;
                transform.position += new Vector3(spinMoveAmmount, 0);
            }
            else
            {
                handleSpin = false;
                stopSpin = true;
                waitMoveCoroutine = StartCoroutine(WaitMove());
                inAction = false;

                StartCoroutine(ResetSpin());
            }
        }
    }

    public IEnumerator ResetSpin()
    {
        yield return new WaitForSeconds(spinCooldown);

        canSpin = true;
    }

    public void ThrowScythe()
    {
        if (throwScythe)
        {
            throwScythe = false;
            StopMove();
            handleThrowScythe = true;

            animator.SetBool("Throw", true);
        }

        if (stopThrowScythe)
        {
            stopThrowScythe = false;
            animator.SetBool("Throw", false);
        }
    }

    public void HandleThrowScythe()
    {
        if (handleThrowScythe)
        {
            if (startThrowScythe)
            {
                startThrowScythe = false;
                scytheScript.gameObject.SetActive(true);
                scytheScript.triggered = true;
            }

            if (handleThrowScytheTrigger1 && scytheScript.travelState == ScytheScript.TravelStateEnum.TravelLine)
            {
                handleThrowScytheTrigger1 = false;
                catcherScript.gameObject.SetActive(true);
            }

            if (catcherScript.catched)
            {
                catcherScript.catched = false;
                catcherScript.gameObject.SetActive(false);
                scytheScript.Finish();
                stopThrowScythe = true;
                catchScythe = true;
                handleThrowScythe = false;
                handleThrowScytheTrigger1 = true;
            }
        }
    }
    
    public void CatchScythe()
    {
        if (catchScythe)
        {
            catchScythe = false;
            StartCoroutine(StopCatchScythe());

            animator.SetBool("Catch", true);
        }

        if (stopCatch)
        {
            stopCatch = false;

            animator.SetBool("Catch", false);
        }
    }

    public IEnumerator StopCatchScythe()
    {
        yield return new WaitForSeconds(catchLength);

        stopCatch = true;
        inAction = false;
        waitMoveCoroutine = StartCoroutine(WaitMove());

        StartCoroutine(ResetThrow());
    }

    public IEnumerator ResetThrow()
    {
        yield return new WaitForSeconds(throwScyteCooldown);
        canThrow = true;
    }

    public void StartThrowScythe()
    {
        startThrowScythe = true;
    }

    public void Attack()
    {
        if (attack)
        {
            attack = false;
            StopMove();
            StartCoroutine(StopAttack());

            animator.SetBool("Attack", true);
        }

        if (stopAttack)
        {
            stopAttack = false;
            animator.SetBool("Attack", false);
        }
    }

    public IEnumerator StopAttack()
    {
        yield return new WaitForSeconds(attackLength);
        
        stopAttack = true;
        if (HaveChance(attack2Chance))
        {
            attack2 = true;
        }
        else
        {
            inAction = false;
            waitMoveCoroutine = StartCoroutine(WaitMove());
        }

        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }

    public void Attack1()
    {
        if (attack1)
        {
            attack1 = false;
            StartCoroutine(StopAttack1());

            animator.SetBool("Attack1", true);
        }

        if (stopAttack1)
        {
            stopAttack1 = false;

            animator.SetBool("Attack1", false);
        }
    }

    public IEnumerator StopAttack1()
    {
        yield return new WaitForSeconds(attack1Length);
        
        stopAttack1 = true;
        inAction = false;
        waitMoveCoroutine = StartCoroutine(WaitMove());
    }

    public void Attack2()
    {
        if (attack2)
        {
            attack2 = false;
            StartCoroutine(StopAttack2());

            animator.SetBool("Attack2", true);
        }

        if (stopAttack2)
        {
            stopAttack2 = false;

            animator.SetBool("Attack2", false);
        }
    }

    public IEnumerator StopAttack2()
    {
        yield return new WaitForSeconds(attack2Length);
        
        stopAttack2 = true;

        if (HaveChance(attack1Chance))
        {
            attack1 = true;
        }
        else
        {
            inAction = false;
            waitMoveCoroutine = StartCoroutine(WaitMove());
        }
    }

    public IEnumerator WaitMove()
    {
        yield return new WaitForSeconds(waitMoveTime);

        canMove = true;
    }

    public void CheckMoving()
    {
        if (canMove)
        {
            canMove = false;
            isMoving = true;
            animator.SetBool("Move", true);
        }
        else if (stopMove)
        {
            stopMove = false;
            isMoving = false;
            animator.SetBool("Move", false);
        }
    }

    public void MoveToTarget()
    {
        if (isMoving)
        {
            transform.position = new Vector3
            (
                transform.position.x + direction * moveSpeed * Time.fixedDeltaTime,
                transform.position.y
            );
        }
    }

    public void StopMove()
    {
        stopMove = true;
        StopCoroutine(waitMoveCoroutine);
    }

    public void UpdateDirection()
    {
        if (directionUpdatable) direction = gameObject.transform.position.x < target.transform.position.x ? 1 : -1;
    }

    public void UpdateScaling()
    {
        gameObject.transform.localScale = new Vector3(direction, 1, 1);
    }

    public float CalculateDistance(GameObject target)
    {
        return Vector2.Distance(transform.position, target.transform.position);
    }

    public float GetAnimationClipLength(Animator animator, String clip)
    {
        return animatedObject.GetAnimationClipLength(animator, clip);
    }

    public bool HaveChance(int percent)
    {
        return UnityRandom.Range(0, 100) < percent;
    }

    /* used this to initialize some coroutine */
    public IEnumerator NullCoroutine() {yield return new WaitForSeconds(0);}

    public void IsFalling()
    {
        if (rigidbody2D.velocity.y < -0.01) isFalling = true;
        else isFalling = false;
    }
}
