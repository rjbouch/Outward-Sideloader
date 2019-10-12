﻿using System;
using System.Collections;
using System.Linq;
using System.Text;
using UnityEngine;
using System.IO;

namespace SideLoader
{
    public class AssetReplacer : MonoBehaviour
    {
        public SideLoader script; 

        public IEnumerator ReplaceActiveAssets()
        {
            script.Log("Replacing active assets...");
            float start = Time.time;

            // ============ materials ============
            var list = Resources.FindObjectsOfTypeAll<Material>()
                        .Where(x => x.mainTexture != null && script.TextureData.ContainsKey(x.mainTexture.name))
                        .ToList();

            script.Log(string.Format("Found {0} materials to replace.", list.Count));

            int i = 0;
            foreach (Material m in list)
            {
                string name = m.mainTexture.name;
                i++; script.Log(string.Format(" - Replacing material {0} of {1}: {2}", i, list.Count, name));

                // set maintexture (diffuse map)
                m.mainTexture = script.TextureData[name];

                // ======= bump map =======                
                if (name.EndsWith("_d")) { name = name.Substring(0, name.Length - 2); } // try remove the _d suffix, if its there                
                name += "_n";  // add the _n (normal map)
                if (script.TextureData.ContainsKey(name))
                {
                    script.Log("  -- Setting bump map for " + m.name);
                    m.SetTexture("_BumpMap", script.TextureData[name]);
                }

                yield return null;
            }

            // ============ something else... ============


            // ==============================================

            script.Log("Active assets replaced. Time: " + (Time.time - start), 0);
            script.Loading = false;
        }

        public IEnumerator LoadTextures()
        {
            script.Log("Reading Texture2D data...");
            float start = Time.time;

            var filesToRead = script.FilePaths[ResourceTypes.Texture];

            foreach (string file in filesToRead)
            {
                // Make sure the file we're trying to read actually exists (it should but who knows)
                string fullPath = script.loadDir + @"\" + ResourceTypes.Texture + @"\" + file;
                if (!File.Exists(fullPath))
                    continue;

                Texture2D texture2D = LoadPNG(fullPath);

                script.TextureData.Add(Path.GetFileNameWithoutExtension(file), texture2D);

                script.Log(" - Texture loaded: " + file);

                yield return null;
            }

            script.Loading = false;
            script.Log("Textures loaded. Time: " + (Time.time - start), 0);
        }

        public static Texture2D LoadPNG(string filePath)
        {
            Texture2D tex = null;
            byte[] fileData;

            if (File.Exists(filePath))
            {
                fileData = File.ReadAllBytes(filePath);
                tex = new Texture2D(2, 2);
                tex.LoadImage(fileData); //..this will auto-resize the texture dimensions.
            }
            return tex;
        }
    }    
}