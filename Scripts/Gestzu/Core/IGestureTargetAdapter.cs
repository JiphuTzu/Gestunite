using UnityEngine;
using System.Collections;
namespace Gestzu.Core
{
    public interface IGestureTargetAdapter
    {
        GameObject target { get; }
        bool Contains(GameObject target);
    }
}

