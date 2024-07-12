﻿using System.Diagnostics;

namespace Hyperbee.Json;

public record JsonPathQuery( string Query, JsonPathSegment Segments, bool Normalized );


[DebuggerDisplay( "{Value}, SelectorKind = {SelectorKind}" )]
public record SelectorDescriptor
{
    public SelectorKind SelectorKind { get; init; }
    public string Value { get; init; }

    public void Deconstruct( out string value, out SelectorKind selectorKind )
    {
        value = Value;
        selectorKind = SelectorKind;
    }
}

[DebuggerTypeProxy( typeof( SegmentDebugView ) )]
[DebuggerDisplay( "First = ({Selectors?[0]}), IsSingular = {IsSingular}, Count = {Selectors?.Length}" )]
public class JsonPathSegment
{
    internal static readonly JsonPathSegment Final = new(); // special end node

    public bool IsFinal => Next == null;

    public bool IsSingular { get; } // singular is true when the selector resolves to one and only one element

    public JsonPathSegment Next { get; set; }
    public SelectorDescriptor[] Selectors { get; init; }

    private JsonPathSegment() { }

    public JsonPathSegment( JsonPathSegment next, string selector, SelectorKind kind )
    {
        Next = next;
        Selectors =
        [
            new SelectorDescriptor { SelectorKind = kind, Value = selector }
        ];
        IsSingular = InitIsSingular();
    }

    public JsonPathSegment( SelectorDescriptor[] selectors )
    {
        Selectors = selectors;
        IsSingular = InitIsSingular();
    }

    public JsonPathSegment Prepend( string selector, SelectorKind kind )
    {
        return new JsonPathSegment( this, selector, kind );
    }

    public IEnumerable<JsonPathSegment> AsEnumerable()
    {
        var current = this;

        while ( current != Final )
        {
            yield return current;

            current = current.Next;
        }
    }

    public bool IsNormalized
    {
        get
        {
            var current = this;

            while ( current != Final )
            {
                if ( !current.IsSingular )
                    return false;

                current = current.Next;
            }

            return true;
        }
    }

    private bool InitIsSingular()
    {
        // singular is one selector that is not a group

        if ( Selectors.Length != 1 )
            return false;

        return (Selectors[0].SelectorKind & SelectorKind.Singular) == SelectorKind.Singular;
    }

    public void Deconstruct( out bool singular, out SelectorDescriptor[] selectors )
    {
        singular = IsSingular;
        selectors = Selectors;
    }

    internal class SegmentDebugView( JsonPathSegment instance )
    {
        [DebuggerBrowsable( DebuggerBrowsableState.RootHidden )]
        public SelectorDescriptor[] Selectors => instance.Selectors;

        [DebuggerBrowsable( DebuggerBrowsableState.Collapsed )]
        public JsonPathSegment Next => instance.Next;
    }
}
