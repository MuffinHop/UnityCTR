using System;
using UnityEngine;

namespace OpenCTR.Level
{
    public class InstancedModel
    {
        public Vector3 Position;
        public Vector3 Rotation;
        public Vector3 Scale;
        public string ModelName;

        public InstancedModel()
        {
        }

        public InstancedModel(string name, Vector3 pos, Vector3 rot, Vector3 scale)
        {
            Position = pos;
            Rotation = rot;
            ModelName = name;
            Scale = scale;
        }

        public void Update()
        {
            if (ModelName == "c" || ModelName == "t" || ModelName == "r")
                Rotation += new Vector3(2f * Time.time, 0, 0);
        }
    }
}
