﻿namespace Hyperbee.Json.Filters.Values;

public struct ValueType<T>( T value ) : INodeType
{
    public readonly NodeTypeKind Kind => NodeTypeKind.Value;

    public INodeTypeComparer Comparer { get; set; }

    public T Value { get; } = value;
}