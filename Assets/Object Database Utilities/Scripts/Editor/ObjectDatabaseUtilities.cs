using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEditor;
using System.Linq;
using System.Globalization;
using System.Text.RegularExpressions;

public static class ObjectDatabaseUtilities
{
    public static ObjectInfoDatabase FillDatabaseSO(string[] assetPaths, string sOOutputFilePath)
    {
        //Initialize database scriptable object
        var objectInfoDatabase = (ObjectInfoDatabase)AssetDatabase.LoadAssetAtPath(sOOutputFilePath, typeof(ObjectInfoDatabase));
        if (!objectInfoDatabase)
        {
            var newFolderPath = Path.GetFullPath(Path.GetDirectoryName(sOOutputFilePath));
            Directory.CreateDirectory(newFolderPath);
            objectInfoDatabase = ScriptableObject.CreateInstance<ObjectInfoDatabase>();
            AssetDatabase.CreateAsset(objectInfoDatabase, $"{sOOutputFilePath}.asset");
        }
        objectInfoDatabase.objectDataList.Clear();

        foreach (var assetPath in assetPaths)
        {
            GameObject model = (GameObject)AssetDatabase.LoadMainAssetAtPath(assetPath);
            var newObjectMetaData = new ObjectMetaData();

            newObjectMetaData.assetName = model.name;
            newObjectMetaData.fileName = assetPath.Split('/').Last();
            newObjectMetaData.directChildCount = model.transform.childCount;
            newObjectMetaData.totalTransforms = model.GetComponentsInChildren<Transform>(true).Length;

            //Materials and Textures
            var meshRenderers = model.GetComponentsInChildren<MeshRenderer>();
            newObjectMetaData.meshRendererCount = meshRenderers.Length;
            HashSet<Material> materials = new HashSet<Material>();
            HashSet<Texture> mainTextures = new HashSet<Texture>();
            foreach (var currentMeshRenderer in meshRenderers)
            {
                foreach (var sharedMaterial in currentMeshRenderer.sharedMaterials)
                {
                    materials.Add(sharedMaterial);
                    if (sharedMaterial.mainTexture != null)
                    {
                        mainTextures.Add(sharedMaterial.mainTexture);
                    }
                }
            }
            newObjectMetaData.materials = materials.ToArray();
            newObjectMetaData.mainTextures = mainTextures.ToArray();

            //triangle count and meshcount
            int triangleIndicesCount = 0;
            var meshFilters = model.GetComponentsInChildren<MeshFilter>();
            foreach (var currentMeshFilter in meshFilters)
            {
                triangleIndicesCount += currentMeshFilter.sharedMesh.triangles.Length;
            }
            newObjectMetaData.triangleCount = triangleIndicesCount / 3;
            newObjectMetaData.subMeshCount = meshFilters.Length;

            //Bounds
            var bounds = new Bounds(Vector3.zero, Vector3.zero);
            bounds = EncapsulateBounds(model.transform, bounds);
            newObjectMetaData.bounds = bounds;
            objectInfoDatabase.objectDataList.Add(newObjectMetaData);
        }
        return objectInfoDatabase;
    }

    public static void ExtractTextures(string[] assetPaths, bool createModelNameSubdirectory, string texturesFolderName = "")
    {
        List<string> assetsToReimport = new List<string>();
        foreach (var assetPath in assetPaths)
        {
            var modelImporter = AssetImporter.GetAtPath(assetPath) as ModelImporter;
            modelImporter.isReadable = true;
            var assetFolderPath = Path.GetDirectoryName(assetPath);
            var assetName = Path.GetFileNameWithoutExtension(assetPath);
            var outputFolderPath = assetFolderPath;

            outputFolderPath = createModelNameSubdirectory ? $"{outputFolderPath}/{assetName}" : outputFolderPath;
            outputFolderPath = texturesFolderName == "" ? outputFolderPath : $"{outputFolderPath}/{texturesFolderName}";
            var fullOutputFolderPath = Path.GetFullPath(outputFolderPath);
            Directory.CreateDirectory(fullOutputFolderPath);

            modelImporter.ExtractTextures(outputFolderPath);
            if (Directory.GetFiles(fullOutputFolderPath).Length == 0) // Check for empty folder when no textures have been extraxted
            {
                AssetDatabase.DeleteAsset(outputFolderPath);
            }
            else
            {
                assetsToReimport.Add(assetPath);
            }
        }
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        foreach (string assetToReimport in assetsToReimport)
        {
            AssetDatabase.ImportAsset(assetToReimport);  // reimporting assets to link the extracted textures
        }
    }

    public static void MultiExtractMaterials(string[] assetPaths, bool createModelNameSubdirectory, string materialsFolderName)
    {
        foreach (var assetPath in assetPaths)
        {
            var assetFolderPath = Path.GetDirectoryName(assetPath);
            var assetName = Path.GetFileNameWithoutExtension(assetPath);
            var outputFolderPath = assetFolderPath;
            outputFolderPath = createModelNameSubdirectory ? $"{outputFolderPath}/{assetName}" : outputFolderPath;
            outputFolderPath = materialsFolderName == "" ? outputFolderPath : $"{outputFolderPath}/{materialsFolderName}";
            var fullOutputFolderPath = Path.GetFullPath(outputFolderPath);
            Directory.CreateDirectory(fullOutputFolderPath);
            ExtractMaterials(assetPath, outputFolderPath);
        }
    }

    public static void ExtractMaterials(string assetPath, string destinationPath)
    {
        HashSet<string> extractedMaterials = new HashSet<string>();
        IEnumerable<Object> embeddedMaterials = from x in AssetDatabase.LoadAllAssetsAtPath(assetPath)
                                         where x.GetType() == typeof(Material)
                                         select x;
        foreach (Object embeddedMaterial in embeddedMaterials)
        {
            string path = $"{destinationPath}/{embeddedMaterial.name}.mat";
            path = AssetDatabase.GenerateUniqueAssetPath(path);
            string errorMessage = AssetDatabase.ExtractAsset(embeddedMaterial, path);
            if (string.IsNullOrEmpty(errorMessage))
            {
                extractedMaterials.Add(assetPath);
            }
        }

        foreach (string extractedMaterial in extractedMaterials)
        {
            AssetDatabase.WriteImportSettingsIfDirty(extractedMaterial);
            AssetDatabase.ImportAsset(extractedMaterial, ImportAssetOptions.ForceUpdate);
        }
    }

    public static Object ExportToCSV(string outputPath, ObjectInfoDatabase objectInfoDatabase)
    {
        var fullFilePath = Path.GetFullPath(outputPath);
        Debug.Log($"writing csv: {fullFilePath}");

        var newFolderPath = Path.GetDirectoryName(fullFilePath);
        Directory.CreateDirectory(newFolderPath);

        StreamWriter writer = new StreamWriter(outputPath);

        //write headers
        writer.WriteLine(
            "filename," +
            "directChildCount," +
            "totalTransformsCount," +
            "meshRendererCount," +
            "materialCount," +
            "materialNames," +
            "subMeshCount," +
            "triangleCount," +
            "mainTextureCount," +
            "mainTextureNames," +
            "mainTextureSizes," +
            "xSize," +
            "ySize," +
            "zSize"
            );

        var objectsDataList = objectInfoDatabase.objectDataList;
        for (int i = 0; i < objectsDataList.Count; i++)
        {
            string currentLine = "";
            currentLine += objectsDataList[i].fileName + ",";
            currentLine += objectsDataList[i].directChildCount + ",";
            currentLine += objectsDataList[i].totalTransforms + ",";
            currentLine += objectsDataList[i].meshRendererCount + ",";
            currentLine += objectsDataList[i].materials.Length + ",";
            currentLine += $"\"{CollectionToString(objectsDataList[i].materials.ToArray())}\",";
            currentLine += objectsDataList[i].subMeshCount + ",";
            currentLine += objectsDataList[i].triangleCount + ",";
            currentLine += objectsDataList[i].mainTextures.Length + ",";
            currentLine += $"\"{CollectionToString(objectsDataList[i].mainTextures)}\",";
            currentLine += $"\"{GetTextureSizesString(objectsDataList[i].mainTextures)}\",";
            currentLine += objectsDataList[i].bounds.size.z.ToString("0.00", CultureInfo.InvariantCulture) + ",";
            currentLine += objectsDataList[i].bounds.size.x.ToString("0.00", CultureInfo.InvariantCulture) + ",";
            currentLine += objectsDataList[i].bounds.size.y.ToString("0.00", CultureInfo.InvariantCulture);
            writer.WriteLine(currentLine);
        }
        writer.Close();
        AssetDatabase.Refresh();
        AssetDatabase.SaveAssets();

        return AssetDatabase.LoadAssetAtPath(outputPath, typeof(Object));
    }

    public static Object CreateThumbnails(string[] assetPaths, string outputFolder, ThumbnailCreatorSettings settings)
    {
        RuntimePreviewGenerator.MarkTextureNonReadable = false;
        RuntimePreviewGenerator.BackgroundColor = settings.backgroundColor;

        foreach (var assetPath in assetPaths)
        {
            var currentObject = (GameObject)AssetDatabase.LoadMainAssetAtPath(assetPath);
            var i = 0;
            foreach (var camerAngle in settings.cameraAngles)
            {
                RuntimePreviewGenerator.PreviewDirection = camerAngle;
                var thumbnail = RuntimePreviewGenerator.GenerateModelPreview(currentObject.transform, settings.width, settings.height);

                byte[] bytes = thumbnail.EncodeToPNG();
                var fullOutputPath = Path.GetFullPath(outputFolder);
                if (!Directory.Exists(fullOutputPath))
                {
                    Directory.CreateDirectory(fullOutputPath);
                    Debug.Log($"creating {fullOutputPath}");
                }
                File.WriteAllBytes(fullOutputPath + "\\" + currentObject.name + "_view" + i + ".png", bytes);
                i++;
            }
        }
        AssetDatabase.Refresh();
        AssetDatabase.SaveAssets();
        Debug.Log($"Thumbnails created: {outputFolder}");
        return AssetDatabase.LoadAssetAtPath(outputFolder, typeof(Object));

    }

    static string CollectionToString(IEnumerable<Object> collection)
    {
        string s = "";
        foreach (var item in collection)
        {
            s += item.name + "\n";
        }
        s = s.TrimEnd('\n');
        return s;
    }

    static string GetTextureSizesString(IEnumerable<Texture> textures)
    {
        string s = "";
        foreach (var texture in textures)
        {
            s += texture.width + "x" + texture.height + "\n";
        }
        s = s.TrimEnd('\n');
        return s;
    }

    private static Bounds EncapsulateBounds(Transform currentTransform, Bounds bounds)
    {
        var renderer = currentTransform.GetComponent<Renderer>();
        if (renderer != null)
        {
            bounds.Encapsulate(renderer.bounds);
        }

        foreach (Transform child in currentTransform)
        {
            bounds = EncapsulateBounds(child, bounds);
        }
        return bounds;
    }

    private static bool CheckForRegexMatch(string input, string pattern)
    {
        if (pattern != "")
        {
            var regex = new Regex(pattern);
            return regex.IsMatch(input) ? true : false;
        }
        else
        {
            return true;
        }
    }

    public static string[] GetAssetPaths(string rootFolder, string unityFilter = "t:Model", string regexFilter = "")
    {
        List<string> assetPaths = new List<string>();
        foreach (var modelGUID in AssetDatabase.FindAssets(unityFilter, new[] { rootFolder }))
        {
            var assetPath = AssetDatabase.GUIDToAssetPath(modelGUID);
            if (!CheckForRegexMatch(assetPath, regexFilter))
            {
                continue;
            }
            assetPaths.Add(assetPath);
        }
        return assetPaths.ToArray();
    }
}

[System.Serializable]
public class ObjectMetaData
{
    public string assetName;
    public string fileName;
    public int directChildCount;
    public int totalTransforms;
    public int meshRendererCount;
    public Material[] materials;
    public int subMeshCount;
    public int triangleCount;
    public Texture[] mainTextures;
    public Bounds bounds;
}
