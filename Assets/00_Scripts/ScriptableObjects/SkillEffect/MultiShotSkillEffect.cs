using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

[CreateAssetMenu(fileName = "MultiShot", menuName = "Skill Effect/MultiShot")]
public class MultiShotSkillEffect : SkillEffect
{
    public float shotDelay = 0.1f;

    private readonly Dictionary<Entity, MultiShotHandler> _activeHandlers = new();

    public override bool Execute(EntityContext context, int stack)
    {
        var entity = context.source as Entity;

        // 이미 실행 중이면 무시
        if (_activeHandlers.ContainsKey(entity)) return false;

        // Handler가 stack번 공격 후 소멸
        var handler = new MultiShotHandler(entity, stack, shotDelay);
        _activeHandlers[entity] = handler;

        handler.OnComplete += () => _activeHandlers.Remove(entity);
        handler.Start();

        return true;
    }
}

public class MultiShotHandler
{
    public event Action OnComplete;

    private readonly Entity _entity;
    private readonly int _totalCount;
    private readonly float _delay;
    private CancellationTokenSource _cts;

    public MultiShotHandler(Entity entity, int totalCount, float delay)
    {
        _entity = entity;
        _totalCount = totalCount;
        _delay = delay;
    }

    public void Start()
    {
        _cts = new CancellationTokenSource();

        _entity.OnDeath += Cancel;

        _ = RunLoop(_cts.Token);
    }

    private async Task RunLoop(CancellationToken token)
    {
        try
        {
            for (int i = 0; i < _totalCount; i++)
            {
                await Task.Delay((int)(_delay * 1000), token);
                _entity.AttackHelper.Attack();
            }
        }
        catch (OperationCanceledException) { }
        finally
        {
            _entity.OnDeath -= Cancel;
            _cts.Dispose();

            OnComplete?.Invoke();
        }
    }

    public void Cancel() => _cts?.Cancel();
}
