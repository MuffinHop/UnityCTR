using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using CTRFramework.Shared;
using UnityEngine;

namespace OGdata.MetaPhys
{
    public class GetMetaPhysData
    {
        private static string[] _regions = new string[]
        {
            "SCUS_944.26", // NTSC
            "SCPS_101.18", // NTSC-J
            "SCES_021.05" // PAL
        };
        public static List<MetaPhys> LoadMetaPhys()
        {
            List<MetaPhys>[] characterMetaPhysics = new List<MetaPhys>[5];
            int regionIndex = 0;
            Debug.Log("Loading MetaPhys data.");
            foreach (string region in _regions)
            {
                string path = $@"{Application.dataPath}/BIG_ctr/{region}";
                Debug.Log($"Looking from {path}.");
                if (File.Exists(path) == false)
                {
                    regionIndex++;
                    continue;
                }

                int index = 0;
                using (BinaryReaderEx br = new BinaryReaderEx(File.OpenRead(path)))
                {
                    int statsAmount = (regionIndex > 0) ? 5 : 4; // NTSC has only four stats
                    for (int characterSpec = 0; characterSpec < statsAmount; characterSpec++)
                    {
                        characterMetaPhysics[characterSpec] = new List<MetaPhys>();
                    }

                    br.BaseStream.Position = 496144;
                    for (int i = 0; i < 110; i++)
                    {
                        int memoryAddress = br.ReadInt32();
                        int wordSize = br.ReadInt32();
                        for (int characterSpec = 0; characterSpec < statsAmount; characterSpec++)
                        {
                            MetaPhys metaPhys = new MetaPhys();
                            metaPhys.Address = memoryAddress;
                            metaPhys.Index = index;
                            metaPhys.WordSize = wordSize;
                            metaPhys.Value = br.ReadInt32();
                            characterMetaPhysics[characterSpec].Add(metaPhys);
                            Debug.Log(metaPhys.Value);
                        }

                        br.ReadInt32(); //always zero?
                    }

                    return null;
                }

            }
            Debug.Log("No MetaPhys found.");
            return null;
        }
    }
}