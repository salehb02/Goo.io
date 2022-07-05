using MK.Toon;
using System.Collections;
using System.Linq;
using UnityEngine;
using Pathfinding;

public class CustomCharacterController : CapturableObject
{
    public enum WeaponAnimationType { Pistol = 1, RifleOrSMG = 2, FlameThrower = 3 }

    [Space(4)]
    [Header("Object Specific Settings")]
    public float speed = 2f;
    public SkinnedMeshRenderer skinnedMesh;
    public PlayerData.CustomShaderColors normalColors;

    [Space(2)]
    [Header("Combat")]
    public WeaponAnimationType weaponAnimationType;
    public GameObject weaponModel;
    public GameObject muzzlePoint;
    public GameObject muzzlePrefab;
    public float shootDistance = 5f;
    public LayerMask enemyLayers;
    public float shootPower = 1;
    public float shootAnimationLength;
    [Space(2)]
    public AudioSource gunSFX;
    public AudioClip[] gunShotClips;
    public Rigidbody bullet;
    public float bulletForce;
    public ParticleSystem hitVFX;

    [Space(2)]
    [Header("Weapon Icon")]
    public GameObject weaponIcon;
    public float rotationSpeed = 1;
    public float sinusMovementSpeed = 1;
    public float sinusMaxMovement = 1f;
    private Vector3 weaponIconInitPos;

    private bool combatMode = false;
    private float smoothCombatMode = 0f;
    private float smoothFireMode = 0f;
    public bool OverrideRotation { get; set; } = false;
    private Coroutine _shootingCoroutine;
    private ParticleSystem _currentMuzzle;
    private bool _init = false;

    // Animator parameters
    public const string WEAPON_BLEND = "Weapon Blend";
    public const string MOVEMENT_SPEED = "Speed";
    public const string SHOOT_TRIGGER = "Shoot";
    public const string FIRE_VALUE = "Fire";

    private Animator _animator;
    private CharacterController _controller;

    // Pathfinding
    public AIPath AIPath { get; set; }

    private void Update()
    {
        if (AIPath.enabled == false)
        {
            // Smooth direction
            SmoothedDirection = Vector3.Lerp(SmoothedDirection, Direction, Time.deltaTime * 5f);

            // Move character
            _controller.SimpleMove(SmoothedDirection * speed);

            // Rotate transform to direction
            if (SmoothedDirection != Vector3.zero && !OverrideRotation)
                transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(SmoothedDirection), Time.deltaTime * 30f);

            // Set animator movement speed
            _animator.SetFloat(MOVEMENT_SPEED, SmoothedDirection.magnitude);
        }
        else
        {
            AIPath.destination = AIDestination;
            _animator.SetFloat(MOVEMENT_SPEED, AIPath.velocity.magnitude);
        }

        // Set animator weapon blend
        smoothCombatMode = Mathf.Lerp(smoothCombatMode, !combatMode ? 0 : (int)weaponAnimationType, Time.deltaTime * 5f);
        _animator.SetFloat(WEAPON_BLEND, smoothCombatMode);

        // Set animator weapon fire ready
        smoothFireMode = Mathf.Lerp(smoothFireMode, combatMode && Target ? 1 : 0, Time.deltaTime * 5f);
        _animator.SetFloat(FIRE_VALUE, smoothFireMode);

        // Combat mode
        Combat();

        if (Input.GetKey(KeyCode.K) && ControllingBy && ControllingBy.Enemy == false)
        {
            if (_shootingCoroutine == null)
                _shootingCoroutine = StartCoroutine(ShootCoroutine());

            _animator.SetLayerWeight(1, 1);
        }

        // Weapon icon rotation
        if (weaponIcon)
        {
            weaponIcon.transform.Rotate(Vector3.up * rotationSpeed);
            weaponIcon.transform.position = weaponIconInitPos + (Vector3.up * Mathf.Sin(sinusMovementSpeed * Time.time) * sinusMaxMovement);
        }
    }

    public override void Init()
    {
        base.Init();

        if (_init)
            return;

        _animator = GetComponent<Animator>();
        _controller = GetComponent<CharacterController>();
        AIPath = GetComponent<AIPath>();

        if (weaponIcon)
            weaponIconInitPos = weaponIcon.transform.position;

        DisableRagdoll();
        SetSkinColor(normalColors);

        combatMode = false;
        OverrideRotation = false;
        weaponModel.gameObject.SetActive(false);
        AIPath.enabled = false;

        if (muzzlePrefab)
            _currentMuzzle = Instantiate(muzzlePrefab, muzzlePoint.transform.position, muzzlePoint.transform.rotation * Quaternion.Euler(0, 180, 0), muzzlePoint.transform).GetComponent<ParticleSystem>();

        _init = true;
    }

    private void Combat()
    {
        if (!combatMode || ControllingBy == null)
            return;

        if (Target == null)
        {
            var colliders = Physics.OverlapSphere(transform.position, shootDistance, enemyLayers);

            foreach (var col in colliders.ToList())
            {
                if (!col.CompareTag("Venom") && !col.CompareTag("Character"))
                    continue;

                var player = col.GetComponentInChildren<PlayerData>();

                if (!player || player == ControllingBy)
                    continue;

                Target = player;
            }

            return;
        }

        if (Vector3.Distance(transform.position, Target.transform.position) > shootDistance)
        {
            OverrideRotation = false;
            Target = null;
            return;
        }

        OverrideRotation = true;

        var targetPosition = Target.transform.position;
        targetPosition.y = transform.position.y;
        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(targetPosition - transform.position), Time.deltaTime * 30f);

        if (_shootingCoroutine == null)
            _shootingCoroutine = StartCoroutine(ShootCoroutine());
    }

    private void EnterCombatMode()
    {
        combatMode = true;
    }

    private IEnumerator ShootCoroutine()
    {
        _animator.SetTrigger(SHOOT_TRIGGER);

        PlayMuzzle();

        var target = Target.GooMode ? Target.transform.position : Target.CapturableObject.venomEntrance.transform.position;
        var bltRigid = Instantiate(this.bullet, muzzlePoint.transform.position, Quaternion.LookRotation(target - muzzlePoint.transform.position), null);
        bltRigid.AddForce(bltRigid.transform.forward * bulletForce);

        var bullet = bltRigid.GetComponent<Bullet>();
        bullet.hitVFX = hitVFX;
        bullet.power = shootPower;
        bullet.characterController = this;

        Destroy(bltRigid.gameObject, 10f);

        gunSFX.PlayOneShot(gunShotClips[Random.Range(0, gunShotClips.Length)]);

        yield return new WaitForSeconds(shootAnimationLength);
        _shootingCoroutine = null;
    }

    private void PlayMuzzle()
    {
        if (!_currentMuzzle)
            return;

        if (_currentMuzzle.isStopped)
            _currentMuzzle.Play();
    }

    private void SetSkinColor(PlayerData.CustomShaderColors colors)
    {
        var mat = new Material(skinnedMesh.sharedMaterial);
        Properties.albedoColor.SetValue(mat, colors.mainColor);
        Properties.goochBrightColor.SetValue(mat, colors.goochBright);
        Properties.goochDarkColor.SetValue(mat, colors.goochDark);
        Properties.rimBrightColor.SetValue(mat, colors.rimBright);
        Properties.rimDarkColor.SetValue(mat, colors.rimDark);
        skinnedMesh.material = mat;
    }

    public void Damage(int amount = 1)
    {
        ControllingBy.Damage(amount);
    }

    public override void Capture(PlayerData controlBy, PlayerData.CustomShaderColors colors)
    {
        base.Capture(controlBy, colors);

        SetSkinColor(colors);
        EnterCombatMode();
        weaponModel.gameObject.SetActive(true);

        if (weaponIcon)
            weaponIcon.gameObject.SetActive(false);

        if (controlBy.Enemy)
        {
            AIPath.enabled = true;
        }
    }

    public override void PreviewMode(PlayerData.CustomShaderColors colors)
    {
        base.PreviewMode(colors);

        Init();

        SetSkinColor(colors);

        weaponModel.gameObject.SetActive(true);

        if (weaponIcon)
            weaponIcon.gameObject.SetActive(false);

        EnterCombatMode();
    }

    public override void LeaveObject()
    {
        base.LeaveObject();

        SetSkinColor(normalColors);
        EnableRagdoll();
    }

    public void DisableRagdoll()
    {
        foreach (var collider in GetComponentsInChildren<Collider>())
        {
            if (collider == _controller)
                continue;

            collider.enabled = false;
        }

        foreach (var rigid in GetComponentsInChildren<Rigidbody>())
            rigid.isKinematic = true;
    }

    public void EnableRagdoll()
    {
        Destroy(_animator);
        Destroy(_controller);
        Destroy(_currentMuzzle.gameObject);
        Destroy(gunSFX.gameObject);
        Destroy(AIPath);

        foreach (var collider in GetComponentsInChildren<Collider>())
        {
            collider.gameObject.layer = LayerMask.NameToLayer(ControlPanel.RAGDOLL_LAYER);
            collider.enabled = true;
        }

        foreach (var rigid in GetComponentsInChildren<Rigidbody>())
            rigid.isKinematic = false;

        //var destoyer = skinnedMesh.gameObject.AddComponent<DestroyOnBecomeInvisible>();
        //destoyer.parent = gameObject;

        Destroy(this);
    }
}