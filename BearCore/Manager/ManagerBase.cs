using System;
using UnityEngine;

namespace BearCore.Manager;

public abstract class ManagerBase : MonoBehaviour, IComparable<ManagerBase> {
    public int initOrder;
    public abstract void Setup();
    public abstract void Init();
    public abstract void Deinit();

    public virtual void Awake() { }
    public virtual void Start() { }
    public virtual void Update() { }

    public int CompareTo(ManagerBase? other) {
        if (ReferenceEquals(this, other)) return 0;
        if (other is null) return 1;
        return initOrder.CompareTo(other.initOrder);
    }
}