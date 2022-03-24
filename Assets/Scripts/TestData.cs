using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Testing {
[Serializable]
public class Record
{
    public int x;
    public int y;
    public int z;
    public int velocityx;
    public int velocityy;
    public int velocityz;
    public int x_press;
    public int left;
    public int right;
    public float time;

    public Vector3 GetRealPosition()
    {
        return new Vector3(
            0.01f*x/256.0f, 
            0.01f*y/256.0f + 0.333f, 
            -0.01f*z/256.0f);
    }
    public Vector3 GetRealVelocity()
    {
        return new Vector3(
            0.01f*velocityx/256.0f, 
            0.01f*velocityy/256.0f, 
            -0.01f*velocityz/256.0f);
    }

    public static Record Interpolate(Record small, Record big, float time)
    {
        Record final = new Record();
        float timeDifference = (time - small.time) / (big.time - small.time);
        final.x = (int)Mathf.Lerp(small.x, big.x, timeDifference);
        final.y = (int)Mathf.Lerp(small.y, big.y, timeDifference);
        final.z = (int)Mathf.Lerp(small.z, big.z, timeDifference);
        final.velocityx = (int)Mathf.Lerp(small.velocityx, big.velocityx, timeDifference);
        final.velocityy = (int)Mathf.Lerp(small.velocityy, big.velocityy, timeDifference);
        final.velocityz = (int)Mathf.Lerp(small.velocityz, big.velocityz, timeDifference);
        if (timeDifference < 0.5f)
        {
            final.x_press = small.x_press;
            final.left = small.left;
            final.right = small.right;
        }
        else
        {
            final.x_press = big.x_press;
            final.left = big.left;
            final.right = big.right;
        }
        final.time = time;
        return final;
    }
}


public static class JsonHelper
{
    public static T[] FromJson<T>(string json)
    {
        Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(json);
        return wrapper.Items;
    }

    public static string ToJson<T>(T[] array)
    {
        Wrapper<T> wrapper = new Wrapper<T>();
        wrapper.Items = array;
        return JsonUtility.ToJson(wrapper);
    }

    public static string ToJson<T>(T[] array, bool prettyPrint)
    {
        Wrapper<T> wrapper = new Wrapper<T>();
        wrapper.Items = array;
        return JsonUtility.ToJson(wrapper, prettyPrint);
    }

    [Serializable]
    private class Wrapper<T>
    {
        public T[] Items;
    }
}
[ExecuteAlways]
public class TestData : MonoBehaviour
{
    public Record[] recordTable;
    public float Scale;
    private void Awake()
    {
        string frameData = File.ReadAllText("frame_data.txt");
        recordTable = JsonHelper.FromJson<Record>(frameData);
        float startTime = recordTable[0].time;
        foreach (var record in recordTable)
        {
            record.time -= startTime;
        }
    }

    private void Start() {
        /*Gizmos.color = Color.magenta;
        for (int i = 0; i < recordTable.Length; i++)
        {
            var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.name = "Track " + i;
            sphere.GetComponent<Collider>().enabled = false;
            sphere.transform.position = 
                8.0f * new Vector3(
                recordTable[i].x/65536.0f, 
                recordTable[i].y/65536.0f, 
                -recordTable[i].z/65536.0f);
            sphere.transform.parent = transform;
        }*/
}

    public bool CanGo = false;

    public void Reset()
    {
        CanGo = true;
    }

public Record GetRecord(float time)
{
    var result = recordTable[0];
    if (time == 0f)
    {
        return result;
    }
    for (int i = 1; i<recordTable.Length; i++)
    {
        if (recordTable[i].time > time)
        {
            result = Record.Interpolate(recordTable[i - 1], recordTable[i], time);
            return result;
        }
    }

    return result;
}

private void OnDrawGizmos()
{
    if (recordTable == null)
    {
        return;
    }
    Gizmos.color = Color.white;
    for (int i = 0; i < recordTable.Length-1; i++)
    {
        if ((i % 3) == 0)
        {
            //Gizmos.color = Color.magenta;
            continue;
        }
        /*else
        {
            Gizmos.color = Color.white;
        }*/
        Gizmos.DrawLine( 
            transform.position + Scale * new Vector3(
                recordTable[i].x/256.0f, 
                recordTable[i].y/256.0f + 0.333f, 
                -recordTable[i].z/256.0f),
            transform.position + Scale * new Vector3(
                recordTable[i+1].x/256.0f, 
                recordTable[i+1].y/256.0f + 0.333f, 
                -recordTable[i+1].z/256.0f));
    }
}
}
}