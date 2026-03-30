using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BTCheckAnimationEnd : BTNode
{
    private Animator animator;
    private List<string> animationStateNames = new List<string>();

    public BTCheckAnimationEnd(Monster monster, string animationStateName)
    {
        animator = monster.GetComponentInChildren<Animator>();
        animationStateNames.Add(animationStateName);
    }

    public BTCheckAnimationEnd(Monster monster, params string[] animationStateNames)
    {
        animator = monster.GetComponentInChildren<Animator>();
        this.animationStateNames = animationStateNames.ToList();
    }

    public override BTNodeState Evaluate()
    {
        AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);

        if (IsName(state, animationStateNames) && state.normalizedTime < 1f)
        {
            return BTNodeState.Failure;
        }

        return BTNodeState.Success;
    }

    private bool IsName(AnimatorStateInfo state, List<string> animationStateNames)
    {
        for (int i = 0; i < animationStateNames.Count; i++)
        {
            if (state.IsName(animationStateNames[i])) return true;
        }

        return false;
    }
}
