using System;
using System.IO;
using CTRFramework.Big;
using DiscUtils;
using DiscUtils.Iso9660;
using DiscUtils.CoreCompat;
using TMPro;
using UnityEngine;

namespace ISO
{
    class IsoExtractor : MonoBehaviour
    {
        [SerializeField] private string _fileLocation;
        [SerializeField] private TextMeshProUGUI _textMeshPro;
        private void Start()
        {
            _textMeshPro.text = $".iso Extraction, file must be located in {_fileLocation}";
            ExtractFile(Application.dataPath + "/" + _fileLocation);
        }

        public void ExtractFile(string FileSource)
        {
            using (FileStream ISOStream = File.Open(FileSource, FileMode.Open))
            {
                CDReader Reader = new CDReader(ISOStream, true, true);
                ExtractDirectory(Reader.Root, Path.GetFileNameWithoutExtension(FileSource), "");
                Reader.Dispose();
                ISOStream.Close();
                BigFile bigFile = BigFile.FromFile(Application.dataPath + "/BIG_ctr/BIGFILE.BIG");
                bigFile.Extract(Application.dataPath + "/Output/");
            }
        }
        
        void ExtractDirectory(DiscDirectoryInfo Dinfo, string RootPath, string PathinISO)
        {
            if (!string.IsNullOrWhiteSpace(PathinISO))
            {
                PathinISO += "\\" + Dinfo.Name;
            }
            RootPath += "\\" + Dinfo.Name;
            AppendDirectory(RootPath);
            foreach (DiscDirectoryInfo dinfo in Dinfo.GetDirectories())
            {
                ExtractDirectory(dinfo, RootPath, PathinISO);
            }
            foreach (DiscFileInfo finfo in Dinfo.GetFiles())
            {
                using (Stream FileStr = finfo.OpenRead())
                {
                    var path = Application.dataPath + "\\BIG_" + RootPath + "\\" + finfo.Name;
                    if (Directory.Exists(Path.GetDirectoryName(path)) == false)
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(path));
                    }
                    using (FileStream Fs = File.Create(path)) // Here you can Set the BufferSize Also e.g. File.Create(RootPath + "\\" + finfo.Name, 4 * 1024)
                    {
                        FileStr.CopyTo(Fs, 4 * 1024); // Buffer Size is 4 * 1024 but you can modify it in your code as per your need
                    }
                }
            }
        }
        static void AppendDirectory(string path)
        {
            try
            {
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
            }
            catch (DirectoryNotFoundException Ex)
            {
                AppendDirectory(Path.GetDirectoryName(path));
            }
            catch (PathTooLongException Exx)
            {
                AppendDirectory(Path.GetDirectoryName(path));
            }
        }
    }
}