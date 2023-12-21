using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * Custom attribute for inspector.
 */

/// <summary>
/// Lock the inspector field, but show the value.
/// </summary>

[System.AttributeUsage(System.AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
public class ReadOnlyFieldAttribute : PropertyAttribute { }
