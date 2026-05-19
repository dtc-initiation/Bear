namespace BearCore;

using System;
using System.Collections.Generic;

public static class EventBus {
    private static readonly Dictionary<Type, object> handlers = new ();
    
    public static void Subscribe<T>(Action<T> handler) {
        Type handlerType = typeof(T);
        if (!handlers.TryGetValue(handlerType, out var existingHandler)) {
            handlers[handlerType] = handler;
        } else {
            handlers[handlerType] = (Action<T>)existingHandler + handler;
        }
    }

    public static void Unsubscribe<T>(Action<T> handler) {
        Type handlerType = typeof(T);
        if (handlers.TryGetValue(handlerType, out var existingHandler)) {
            Action<T> updated = (Action<T>)existingHandler - handler;
            if (updated == null) {
                handlers.Remove(handlerType);
            } else {
                handlers[handlerType] = updated;
            }
        }
    }

    public static void Raise<T>(T @event) {
        if (handlers.TryGetValue(typeof(T), out var existingHandler)) {
            ((Action<T>) existingHandler).Invoke(@event);
        }
    }
}
