﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

namespace CTRFramework.Shared
{
    public class Pose : IReadWrite
    {
        public static Pose Zero = new Pose(Vector3.zero, Vector3.zero);

        private Vector3 position;
        private Vector3 rotation;

        #region ComponentModel
        [CategoryAttribute("Values"), DescriptionAttribute("Position vector.")]
        public Vector3 Position
        {
            get => position;
            set => position = value;
        }

        [CategoryAttribute("Values"), DescriptionAttribute("Rotation vector.")]
        public Vector3 Rotation
        {
            get => rotation;
            set => rotation = value;
        }
        #endregion

        public Pose()
        {
        }

        public static Pose FromReader(BinaryReaderEx br)
        {
            return new Pose(br);
        }

        public Pose(BinaryReaderEx br)
        {
            Read(br);
        }

        public void Move(Vector3 move)
        {
            position += move;
        }

        public void Rotate(Vector3 rot)
        {
            rotation += rot;
        }

        public Pose(Vector3 pos, Vector3 ang)
        {
            position = pos;
            rotation = ang;
        }

        public void Read(BinaryReaderEx br)
        {
            position = br.ReadVector3s(1 / 100f);
            rotation = br.ReadVector3s(1 / 4096f);
        }

        public void Write(BinaryWriterEx bw, List<UIntPtr> patchTable = null)
        {
            bw.WriteVector3s(position, 1 / 100f);
            bw.WriteVector3s(rotation, 1 / 4096f);
        }

        public override string ToString()
        {
            return $"Pos: {position} Rot: {rotation}";
        }
    }
}