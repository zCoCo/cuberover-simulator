using UnityEngine;

using System;
using System.Reflection;
using System.Linq;

using System.Runtime.InteropServices;

/// <summary>
///     Abstract Class Definition for a Pseudosensor which collects data from
///     whatever GameObject it's attached to and returns that data in a packet
///     which will be serialized as JSON before being sent up stream to the
///     database via the backend.
///     This approach allows for a singular definition of the format of the data
///     (in the override of GetSensorData) right where the data is collected.
///
/// Author: Connor Colombo (CMU)
/// Last Update: 7/6/2020, Colombo (CMU)
/// </summary>
public abstract class PseudoSensor : MonoBehaviour
{
    /// <summary>
    /// Returns an Anonymous Type Object Defining the Form and Contents of the
    /// Data to be Pushed to the Database.
    /// </summary>
    public abstract dynamic GetSensorData();

    /// <summary>
    /// Returns a JSON string Containing Key-Value Pairs with string Keys ands
    /// Values of Type T. Contents will get pushed to DB.
    /// </summary>
    public string GetSensorJSON()
    {
        return JsonUtility.ToJson(GetSensorData());
    }

    /// <summary>
    /// Equivalent Size in Bytes of the CORE Data Package being Sent if Sent
    /// from the Real Rover (Not the size of the Key-Value Pair Dictionary;
    /// rather, the cumulative size of the values in the dictionary.
    /// NOTE: This may return inconsistent results (w.r.t. to the size on the
    /// IRL system) when T is boolean or char because of they way the Marshaller
    /// handles these types. As such, these values should be treated as
    /// approximates and not truths.
    /// </summary>
    public int GetDataSize()
    {
        // Basically return cumulative size of all fields and properties
        // Note: Basic structs use fields (or properties if specified) and anonymous types use properties.
        Type type = GetSensorData().GetType();
        FieldInfo[] fields = type.GetFields();
        PropertyInfo[] props = type.GetProperties();
        return fields.Sum(f => Marshal.SizeOf(f.FieldType)) + props.Sum(p => Marshal.SizeOf(p.PropertyType));
    }

    /// <summary>
    /// Helper function which returns the size of the member described by the
    /// given MemberInfo object.
    /// If the member is neither a value nor a string,
    /// the member is assumed to be an object with members of its own; so, this function is recursed.
    /// </summary>
    /*protected int GetMemberSize<M>(M member) where M : MemberInfo
    {
        Type type;
        if

        if(M is FieldInfo)
        {
            type = (FieldInfo)(member).FieldType;
        }
        if (pi.PropertyType.IsValueType)
        {
            return Marshal.SizeOf(pi.PropertyType);
        } else if(pi.PropertyType)
    }*/

    // TODO:
    // Make pseudosensor work with anon types
    // -- Make GetMemberSize recurse
    // Template PseudoLocalizer and verify recursion depth
    // Remove pseudosensor<T> references
    // Make buffer a (json) string buffer in BufferedSensorReader
    // Verify json serialization of anon types in unity
    // -- if not, switch to Payload struct (while keeping object nesting)
    // Complete BufferedSensorReader
    // -- Ensure switching on/off works
    // Complete TelemetrySender
    // -- polling
    // -- sending over ZMQ
    // -- -- ZMQ Messaging Layer (in BackendConnection or separate? <- i'd argue inhouse)
    // Write the python for deserializing json (if necessary for mongo) and passing up stream to arbitrary collection (without data class format validation - since that's handled here)
    // Impl. PseudoLocalizer
    // Test.
}
