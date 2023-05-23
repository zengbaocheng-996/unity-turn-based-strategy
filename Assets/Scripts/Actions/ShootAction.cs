using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootAction : BaseAction
{
    public event EventHandler<OnShootEvenArgs> OnShoot;
    public class OnShootEvenArgs:EventArgs
    {
        public Unit targetUnit;
        public Unit shootingUnit;
    }
    private enum State 
    {
        Aiming,
        Shooting,
        Cooloff,
    }
    [SerializeField] private LayerMask obstacleLayerMask;
    private State state;
    private int maxShootDistance = 4;
    private float stateTimer;
    private Unit targetUnit;
    private bool canShootBullet;
    public override string GetActionName()
    {
        return "Shoot";
    }
    private void Update()
    {
        if (!isActive)
        {
            return;
        }

        stateTimer -= Time.deltaTime;
        switch(state)
        {
            case State.Aiming:
                Vector3 aimDir = (targetUnit.GetWorldPosition()-unit.GetWorldPosition()).normalized;
                float rotateSpeed = 20f;
                transform.forward = Vector3.Lerp(transform.forward, aimDir, Time.deltaTime * rotateSpeed);
                break;
            case State.Shooting:
                if(canShootBullet)
                {
                    Shoot();
                    canShootBullet = false;
                }
                break;
            case State.Cooloff:
                break;
        }
        if (stateTimer <= 0f)
        {
            NextState(); 
        }
    }
    private void Shoot()
    {
        OnShoot?.Invoke(this, new OnShootEvenArgs
        {
            targetUnit=targetUnit,
            shootingUnit=unit
        });
        targetUnit.Damage(40);
    }
    private void NextState()
    {
        switch(state)
        {
            case State.Aiming:
                state = State.Shooting;
                float shootingStateTime = 0.1f;
                stateTimer = shootingStateTime;
                break;
            case State.Shooting:
                state = State.Cooloff;
                float coolOffStateTime = 0.5f;
                stateTimer = coolOffStateTime; 
                break;
            case State.Cooloff:
                ActionComplete();
                break;
        }
    }
    public override List<GridPosition> GetValidActionGridPositionList()
    {
        GridPosition unitGridPosition = unit.GetGridPosition();
        return GetValidActionGridPositionList(unitGridPosition);
    }
    public List<GridPosition> GetValidActionGridPositionList(GridPosition unitGridPosition)
    {
        List<GridPosition> validGridPositionList = new List<GridPosition>();

        for (int x = -maxShootDistance; x <= maxShootDistance; x++)
        {
            for (int z = -maxShootDistance; z <= maxShootDistance; z++)
            {
                GridPosition offsetGridPosition = new GridPosition(x, z);
                GridPosition testGridPosition = unitGridPosition + offsetGridPosition;
                if (!LevelGrid.Instance.IsValidGridPosition(testGridPosition))
                {
                    continue;
                }
                int testDistance = Mathf.Abs(x) + Math.Abs(z);
                if(testDistance>maxShootDistance)
                {
                    continue;
                }
                //validGridPositionList.Add(testGridPosition);
                //continue;
                if (!LevelGrid.Instance.HasAnyUnitOnGridPosition(testGridPosition))
                {
                    // Grid position is empty, no unit
                    continue;
                }

                Unit targetUnit = LevelGrid.Instance.GetUnitAtGridPosition(testGridPosition);
               
                if (targetUnit.IsEnemy()==unit.IsEnemy())
                {
                    // Both Units on same 'team'
                    continue;
                }
                Vector3 unitWorldPosition = LevelGrid.Instance.GetWorldPosition(unitGridPosition);
                Vector3 shootDir = (targetUnit.GetWorldPosition() - unitWorldPosition).normalized;
                float unitShoulderHeight = 1.7f;
                if(Physics.Raycast(
                    unitWorldPosition + Vector3.up * unitShoulderHeight,
                    shootDir,
                    Vector3.Distance(unitWorldPosition, targetUnit.GetWorldPosition()),
                    obstacleLayerMask))
                {
                    // Blocked by an Obstacle
                    continue;
                }

                validGridPositionList.Add(testGridPosition);
            }
        }
        return validGridPositionList;
    }

    public override void TakeAction(GridPosition gridPosition, Action onActionComplete)
    {
        targetUnit = LevelGrid.Instance.GetUnitAtGridPosition(gridPosition);
        state = State.Aiming;
        float aimingStateTime = 1f;
        stateTimer = aimingStateTime;
        canShootBullet = true;
        ActionStart(onActionComplete);

    }
    public Unit GetTargetUnit()
    {
        return targetUnit;
    }
    public int GetMaxShootDistance()
    {
        return maxShootDistance;
    }
    public override EnemyAIAction GetEnemyAIAction(GridPosition gridPosition)
    {
        Unit targetUnit =  LevelGrid.Instance.GetUnitAtGridPosition(gridPosition);
        
        return new EnemyAIAction
        {
            gridPosition = gridPosition,
            actionValue = 100+Mathf.RoundToInt((1 - targetUnit.GetHealthNormalized())*100f),
        };
    }
    public int GetTargetCountAtPosition(GridPosition gridPosition)
    {
        return GetValidActionGridPositionList(gridPosition).Count;
    }
}
