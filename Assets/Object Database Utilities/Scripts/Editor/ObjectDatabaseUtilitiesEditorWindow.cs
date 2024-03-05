using UnityEditor;
using UnityEngine;

public class ObjectDatabaseUtilitiesEditorWindow : EditorWindow
{
    string databaseName = "DefaultDatabase";
    string objectsFolderPath = "Assets/Object Database Utilities/3D Objects";
    string regexFilter = "";
    Object objectInfoDatabaseObject;
    bool createModelNameSubDirectory = true;
    string texturesFolderName = "";
    string materialsFolderName = "";
    Object thumbnailCreatorSettings;

    readonly string databaseUtilityFolderPath = "Assets/Object Database Utilities";
    readonly string outputFolderName = "Output";
    readonly string cSVFolderName = "CSV";
    readonly string thumbnailsFolderName = "Thumbnails";
    readonly string sOFolderName = "ScriptableObjects";

    private void Awake()
    {
        thumbnailCreatorSettings = AssetDatabase.LoadAssetAtPath($"{databaseUtilityFolderPath}/Settings/ThumbnailCreatorSettings.asset", typeof(ThumbnailCreatorSettings));
        var defaultDatabaseSOPath = $"{databaseUtilityFolderPath}/{outputFolderName}/{databaseName}/{sOFolderName}/{databaseName}.asset";
        objectInfoDatabaseObject = AssetDatabase.LoadAssetAtPath(defaultDatabaseSOPath, typeof(ObjectInfoDatabase));
    }

    [MenuItem("Object Database Utilities/Object Database Utilities...")]
    public static void ShowWindow()
    {
        ObjectDatabaseUtilitiesEditorWindow editorWindow = GetWindow<ObjectDatabaseUtilitiesEditorWindow>();
        editorWindow.titleContent = new GUIContent("Object Database Utilities");
    }

    void OnGUI()
    {
        databaseName = EditorGUILayout.TextField("Database name", databaseName);
        objectsFolderPath = EditorGUILayout.TextField("Object Folder Path", objectsFolderPath);
        regexFilter = EditorGUILayout.TextField("Regex Filter (optional)", regexFilter);

        if (GUILayout.Button("Fill Database SO"))
        {
            var sOOutputFilePath = $"{databaseUtilityFolderPath}/{outputFolderName}/{databaseName}/{sOFolderName}/{databaseName}";
            var assetPaths = ObjectDatabaseUtilities.GetAssetPaths(objectsFolderPath, regexFilter: regexFilter);
            objectInfoDatabaseObject = ObjectDatabaseUtilities.FillDatabaseSO(assetPaths, sOOutputFilePath);
            Selection.activeObject = objectInfoDatabaseObject;
        }

        objectInfoDatabaseObject = EditorGUILayout.ObjectField(objectInfoDatabaseObject, typeof(ObjectInfoDatabase), false);

        if (GUILayout.Button("Export SO to CSV"))
        {
            string cSVExportFilePath = $"{databaseUtilityFolderPath}/{outputFolderName}/{databaseName}/{cSVFolderName}/{databaseName}.csv";
            var cSVObject = ObjectDatabaseUtilities.ExportToCSV(cSVExportFilePath, (ObjectInfoDatabase)objectInfoDatabaseObject);
            Selection.activeObject = cSVObject;
        }

        GUILayout.Space(20f);
        GUILayout.Label("Batch Extracting", EditorStyles.boldLabel);
        EditorGUIUtility.labelWidth = 200f;
        createModelNameSubDirectory = EditorGUILayout.Toggle("createModelNameSubDirectory", createModelNameSubDirectory);
        texturesFolderName = EditorGUILayout.TextField("Textures Folder Name (optional)", texturesFolderName);
        materialsFolderName = EditorGUILayout.TextField("Materials Folder Name (optional)", materialsFolderName);

        if (GUILayout.Button("Extract Textures"))
        {
            var assetPaths = ObjectDatabaseUtilities.GetAssetPaths(objectsFolderPath, regexFilter: regexFilter);
            ObjectDatabaseUtilities.ExtractTextures(assetPaths, createModelNameSubDirectory, texturesFolderName);
        }

        if (GUILayout.Button("Extract Materials"))
        {
            var assetPaths = ObjectDatabaseUtilities.GetAssetPaths(objectsFolderPath, regexFilter: regexFilter);
            ObjectDatabaseUtilities.MultiExtractMaterials(assetPaths, createModelNameSubDirectory, materialsFolderName);
        }

        GUILayout.Space(20f);
        GUILayout.Label("Thumbnail Creator", EditorStyles.boldLabel);
        thumbnailCreatorSettings = EditorGUILayout.ObjectField(thumbnailCreatorSettings, typeof(ThumbnailCreatorSettings), false);

        if (GUILayout.Button("Create Thumbnails"))
        {
            string thumbnailExportFolderPath = $"{databaseUtilityFolderPath}/{outputFolderName}/{databaseName}/{thumbnailsFolderName}";
            var assetPaths = ObjectDatabaseUtilities.GetAssetPaths(objectsFolderPath, regexFilter: regexFilter);
            var thumbnailsFolderObject = ObjectDatabaseUtilities.CreateThumbnails(assetPaths, thumbnailExportFolderPath, (ThumbnailCreatorSettings)thumbnailCreatorSettings);
            Selection.activeObject = thumbnailsFolderObject;
        }
    }
}