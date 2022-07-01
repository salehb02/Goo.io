using UnityEngine;

public class AIController : MonoBehaviour
{
    public enum AIStatue { LookingForCapturable, MovingToCapturable, LookingForTarget, MovingToTarget, AttackingTarget, LeaveCapturedBody }

    private Vector2 leaveBodyChance = new Vector2(0.6f,0.85f);

    private bool willLeave;

    private PlayerData playerData;
    private AIStatue currentStatue;
    private CapturableObject currentCapturable;
    private GooController currentGoo;
    private PlayerData currentTarget;
    private GameManager gameManager;

    private void Start()
    {
        playerData = GetComponent<PlayerData>();
        currentGoo = playerData.GetComponentInParent<GooController>();
        gameManager = FindObjectOfType<GameManager>();

        CalculateLeaveChance();
    }

    private void Update()
    {
        if(enabled && currentGoo.AIPath.enabled == false)
            currentGoo.AIPath.enabled = true;

        if (currentCapturable == null)
        {
            currentStatue = AIStatue.LookingForCapturable;
        }
        else
        {
            if (currentCapturable.ControllingBy != playerData)
            {
                currentStatue = AIStatue.MovingToCapturable;
            }
            else
            {
                if (currentTarget == null)
                {
                    currentStatue = AIStatue.LookingForTarget;
                }
                else
                {
                    if (currentCapturable.Target)
                        currentStatue = AIStatue.AttackingTarget;
                    else
                        currentStatue = AIStatue.MovingToTarget;

                    if (currentCapturable && playerData.Health < playerData.maxHealth / 3f && willLeave && gameManager.SpawnedCapturables.Count > 0)
                    {
                        currentStatue = AIStatue.LeaveCapturedBody;
                    }
                }
            }
        }

        ProccesAI();
    }

    private void ProccesAI()
    {
        if (currentStatue == AIStatue.LookingForCapturable)
        {
            var nearest = NearestCapturable();

            if (nearest != null)
                currentCapturable = nearest;

            return;
        }

        if (currentStatue == AIStatue.MovingToCapturable)
        {
            if (!currentCapturable.Capturable())
            {
                currentCapturable = null;
                return;
            }

            currentGoo.AIDestination = currentCapturable.transform.position;
            //playerData.Movement((currentCapturable.transform.position - transform.position).normalized / 1.2f);

            return;
        }

        if (currentStatue == AIStatue.LookingForTarget)
        {
            var target = NearestTarget();

            if (target != null)
                currentTarget = target;

            return;
        }

        if (currentStatue == AIStatue.MovingToTarget)
        {
            currentCapturable.AIDestination = currentTarget.transform.position;
            //playerData.Movement((currentTarget.transform.position - transform.position).normalized / 1.2f);

            return;
        }

        if (currentStatue == AIStatue.AttackingTarget)
        {
            //playerData.Movement(Vector3.zero);

            return;
        }

        if (currentStatue == AIStatue.LeaveCapturedBody)
        {
            if (!willLeave)
                return;

            if (currentCapturable.ControllingBy == playerData)
            {
                //currentCapturable.LeaveObject();
                currentCapturable = null;
                currentTarget = null;
                playerData.SetToGoo();
                CalculateLeaveChance();
            }

            return;
        }
    }

    private CapturableObject NearestCapturable()
    {
        CapturableObject nearest = null;

        foreach (var capturable in gameManager.SpawnedCapturables)
        {
            if (!capturable.Capturable())
                continue;

            if (nearest == null)
            {
                nearest = capturable;
            }
            else
            {
                if (Vector3.Distance(transform.position, capturable.transform.position) < Vector3.Distance(transform.position, nearest.transform.position))
                    nearest = capturable;
            }
        }

        return nearest;
    }

    private PlayerData NearestTarget()
    {
        PlayerData nearest = null;

        foreach (var player in gameManager.players)
        {
            if (player == playerData)
                continue;

            if (nearest == null)
            {
                nearest = player;
            }
            else
            {
                if (Vector3.Distance(transform.position, player.transform.position) < Vector3.Distance(transform.position, nearest.transform.position))
                    nearest = player;
            }
        }

        return nearest;
    }

    private void CalculateLeaveChance()
    {
        if (Random.value <= Random.Range(leaveBodyChance.x, leaveBodyChance.y))
        {
            willLeave = true;
            return;
        }

        willLeave = false;
    }
}