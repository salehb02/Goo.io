using System.Collections;
using System.Linq;
using UnityEngine;

public class CustomCharacterController : CapturableObject
{
    [Space(4)]
    [Header("Object Specific Settings")]
    public float speed = 2f;
    public GameObject ragdoll;
    public SkinnedMeshRenderer skinnedMesh;
    public Color normalColor;

    [Space(2)]
    [Header("Combat")]
    public GameObject weaponModel;
    public GameObject muzzlePoint;
    public GameObject muzzlePrefab;
    public float shootDistance = 5f;
    public LayerMask enemyLayers;
    public int shootPower = 1;
    public float shootAnimationLength;
    [Space(2)]
    public AudioSource gunSFX;
    public AudioClip[] gunShotClips;

    private PlayerData _target;
    private bool combatMode = false;
    private float smoothCombatMode = 0f;
    private bool _overrideRotation = false;
    private Coroutine _shootingCoroutine;
    private ParticleSystem _currentMuzzle;

    // Animator parameters
    public const string WEAPON_BLEND = "Weapon Blend";
    public const string MOVEMENT_SPEED = "Speed";
    public const string SHOOT_TRIGGER = "Shoot";

    private Animator _animator;
    private CharacterController _controller;

    private void Start()
    {
        Init();
    }

    private void Update()
    {
        // Smooth direction
        SmoothedDirection = Vector3.Lerp(SmoothedDirection, Direction, Time.deltaTime * 5f);

        // Move character
        _controller.SimpleMove(SmoothedDirection * speed);

        // Rotate transform to direction
        if (SmoothedDirection != Vector3.zero && !_overrideRotation)
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(SmoothedDirection), Time.deltaTime * 10f);

        // Set animator movement speed
        _animator.SetFloat(MOVEMENT_SPEED, SmoothedDirection.magnitude);

        // Set animator weapon blend
        smoothCombatMode = Mathf.Lerp(smoothCombatMode, combatMode ? 1 : 0, Time.deltaTime * 5f);
        _animator.SetFloat(WEAPON_BLEND, smoothCombatMode);

        // Combat mode
        Combat();
    }

    private void Init()
    {
        _animator = GetComponent<Animator>();
        _controller = GetComponent<CharacterController>();

        SetSkinColor(normalColor);

        combatMode = false;
        _overrideRotation = false;
        _animator.SetLayerWeight(1, 0);

        if (muzzlePrefab)
            _currentMuzzle = Instantiate(muzzlePrefab, muzzlePoint.transform.position, muzzlePoint.transform.rotation, muzzlePoint.transform).GetComponent<ParticleSystem>();
    }

    private void Combat()
    {
        if (!combatMode || ControllingBy == null)
            return;

        if (_target == null)
        {
            var colliders = Physics.OverlapSphere(transform.position, shootDistance, enemyLayers);

            foreach (var col in colliders.ToList())
            {
                var parent = col.transform.parent;
                PlayerData player = null;

                if (!parent)
                    continue;

                player = col.transform.parent.GetComponentInChildren<PlayerData>();

                if (!player || !player.Enemy)
                    continue;

                _target = player;
            }

            return;
        }

        if (Vector3.Distance(transform.position, _target.transform.position) > shootDistance)
        {
            _overrideRotation = false;
            _target = null;
            return;
        }

        _overrideRotation = true;
        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(_target.transform.position - transform.position), Time.deltaTime * 10f);

        if (_shootingCoroutine == null)
            _shootingCoroutine = StartCoroutine(ShootCoroutine());
    }

    private void EnterCombatMode()
    {
        combatMode = true;
        _animator.SetLayerWeight(1, 1);
    }

    private IEnumerator ShootCoroutine()
    {
        _animator.SetTrigger(SHOOT_TRIGGER);
        yield return new WaitForSeconds(0.2f);

        _currentMuzzle?.Play();
        _target.Damage(shootPower);
        gunSFX.PlayOneShot(gunShotClips[Random.Range(0, gunShotClips.Length)]);

        yield return new WaitForSeconds(0.2f);

        _currentMuzzle?.Stop();

        yield return new WaitForSeconds(shootAnimationLength);
        _shootingCoroutine = null;
    }

    private void SetSkinColor(Color color)
    {
        var mats = skinnedMesh.sharedMaterials;
        foreach (var mat in mats)
            mat.SetColor("_BaseColor", color);
        skinnedMesh.materials = mats;
    }

    public void Damage(int amount = 1)
    {
        ControllingBy.Damage(amount);
    }

    public override void Capture(PlayerData controlBy, Color color)
    {
        base.Capture(controlBy, color);

        SetSkinColor(color);
        EnterCombatMode();
    }

    public override void LeaveObject()
    {
        base.LeaveObject();

        var ragdoll = Instantiate(this.ragdoll, transform.position, transform.rotation, null).GetComponent<Ragdoll>();
        ragdoll.SetColor(normalColor);

        Destroy(gameObject);
    }
}