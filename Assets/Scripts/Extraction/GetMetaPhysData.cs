using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using CTRFramework.Shared;
using UnityEditor;
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

        private static string[] _classes = new string[]
        {
            "BALANCED", "ACCELERATION", "SPEED", "TURN", "MAX"
        };
        public static List<MetaPhys>[] LoadMetaPhys()
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
                            MetaPhys metaPhys = ScriptableObject.CreateInstance<MetaPhys>();
                            metaPhys.Address = memoryAddress;
                            metaPhys.Index = index;
                            metaPhys.WordSize = wordSize;
                            metaPhys.Value = br.ReadInt32();
                            characterMetaPhysics[characterSpec].Add(metaPhys);
                            Debug.Log(metaPhys.Value);

                            var metaPath = @$"Assets/MetaData/{_classes[characterSpec]}/{_classes[characterSpec]}_{metaPhys.Address}.asset";
                            Debug.Log(metaPath);
                            AssetDatabase.CreateAsset(metaPhys, metaPath);
                        }
                        br.ReadInt32(); //always zero?
                    }
                    AssetDatabase.SaveAssets();

                    if (statsAmount == 4) // we need to generate a max stats character since we're on NTSC
                    {
                        characterMetaPhysics[4] = new List<MetaPhys>();
                        for (int i = 0; i < 110; i++)
                        {
                            MetaPhys maxMetaPhys = ScriptableObject.CreateInstance<MetaPhys>();
                            int maxValue = -1;
                            for (int characterSpec = 0; characterSpec < statsAmount; characterSpec++)
                            {
                                MetaPhys metaPhys = characterMetaPhysics[characterSpec][i];
                                maxMetaPhys.Address = Math.Max(maxMetaPhys.Address, metaPhys.Address);
                                maxMetaPhys.Index = Math.Max(maxMetaPhys.Index, metaPhys.Index);
                                maxMetaPhys.WordSize = Math.Max(maxMetaPhys.WordSize, metaPhys.WordSize);
                                maxMetaPhys.Value = Math.Max(maxMetaPhys.Value, metaPhys.Value);
                            }
                            var metaPath = @$"Assets/MetaData/{_classes[4]}/{_classes[4]}_{maxMetaPhys.Address}.asset";
                            Debug.Log(metaPath);

                            AssetDatabase.CreateAsset(maxMetaPhys, metaPath);
                        }
                        AssetDatabase.SaveAssets();
                    }
                    return characterMetaPhysics;
                }

            }
            Debug.Log("No MetaPhys found.");
            return null;
        }
    }
}