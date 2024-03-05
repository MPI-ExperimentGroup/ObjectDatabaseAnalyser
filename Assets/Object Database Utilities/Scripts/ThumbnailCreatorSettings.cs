using UnityEngine;

[CreateAssetMenu(fileName = "ThumbnailCreatorSettings", menuName = "ScriptableObjects/ThumbnailCreatorSettings", order = 1)]
public class ThumbnailCreatorSettings : ScriptableObject
{
    public int width = 512;
    public int height = 512;
    public Color backgroundColor = new Color(0.5f, 0.5f, 0.5f, 0f);
    public Vector3[] cameraAngles = { new Vector3(-1f, -1f, -1f) };
}
