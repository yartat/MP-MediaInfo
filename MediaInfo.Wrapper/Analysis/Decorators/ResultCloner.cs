#region Copyright (C) 2017-2026 Yaroslav Tatarenko

// Copyright (C) 2017-2026 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2026 MediaArea.net SARL.
// https://mediaarea.net

#endregion

using System;
using System.Collections.Generic;
using System.Reflection;

namespace MediaInfo.Analysis.Decorators;

/// <summary>
/// Makes an independent copy of an analysis result, so that a caller who changes a stream cannot change what the cache
/// hands to the next caller.
/// </summary>
/// <remarks>
/// The streams are classes with public setters and nested objects (tags, HDR info, lists), so the copy walks the fields
/// instead of a list of properties: a member added to the model later is copied without touching this class. Objects that
/// are shared inside the result (the best video stream is also an element of the video list) stay shared inside the copy.
/// Strings, enums, delegates, exceptions and the types that do not belong to this library or to the collections of
/// the framework are not copied: they are immutable, or there is nothing a caller could usefully change in them.
/// </remarks>
internal static class ResultCloner
{
  private static readonly MethodInfo MemberwiseCloneMethod =
    typeof(object).GetMethod("MemberwiseClone", BindingFlags.Instance | BindingFlags.NonPublic)!;

  public static T Clone<T>(T value)
    where T : class =>
    (T)Copy(value, new Dictionary<object, object>(ReferenceComparer.Instance))!;

  private static object? Copy(object? value, Dictionary<object, object> copies)
  {
    if (value is null)
    {
      return null;
    }

    var type = value.GetType();
    if (!ShouldCopy(type))
    {
      return value;
    }

    if (copies.TryGetValue(value, out var existing))
    {
      return existing;
    }

    if (value is Array array)
    {
      var arrayCopy = (Array)array.Clone();
      copies[value] = arrayCopy;
      if (!type.GetElementType()!.IsValueType)
      {
        for (var i = 0; i < arrayCopy.Length; i++)
        {
          arrayCopy.SetValue(Copy(arrayCopy.GetValue(i), copies), i);
        }
      }

      return arrayCopy;
    }

    var clone = MemberwiseCloneMethod.Invoke(value, null)!;
    copies[value] = clone;
    for (var current = type; current is not null && current != typeof(object); current = current.BaseType)
    {
      foreach (var field in current.GetFields(
        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
      {
        if (field.FieldType.IsValueType || field.FieldType == typeof(string))
        {
          continue;
        }

        field.SetValue(clone, Copy(field.GetValue(clone), copies));
      }
    }

    return clone;
  }

  private static bool ShouldCopy(Type type)
  {
    if (type.IsValueType || type == typeof(string) || typeof(Delegate).IsAssignableFrom(type)
        || typeof(Exception).IsAssignableFrom(type) || typeof(Type).IsAssignableFrom(type))
    {
      return false;
    }

    if (type.IsArray)
    {
      return true;
    }

    var assembly = type.Assembly;
    if (assembly == typeof(ResultCloner).Assembly)
    {
      return true;
    }

    // The lists and dictionaries the model is built from, wherever they are declared (the compiler also makes list types
    // for collection expressions). A lazy LINQ iterator is not a collection of its own and is left alone.
    return type.Namespace != "System.Linq" && IsCollection(type);
  }

  private static bool IsCollection(Type type)
  {
    if (typeof(System.Collections.ICollection).IsAssignableFrom(type))
    {
      return true;
    }

    foreach (var contract in type.GetInterfaces())
    {
      if (contract.IsGenericType)
      {
        var definition = contract.GetGenericTypeDefinition();
        if (definition == typeof(ICollection<>) || definition == typeof(IReadOnlyCollection<>))
        {
          return true;
        }
      }
    }

    return false;
  }

  private sealed class ReferenceComparer : IEqualityComparer<object>
  {
    public static readonly ReferenceComparer Instance = new();

    bool IEqualityComparer<object>.Equals(object? x, object? y) => ReferenceEquals(x, y);

    int IEqualityComparer<object>.GetHashCode(object obj) => System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(obj);
  }
}
