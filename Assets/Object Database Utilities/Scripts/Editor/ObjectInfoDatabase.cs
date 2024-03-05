using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ObjectInfoDatabase", menuName = "ScriptableObjects/ObjectInfoDatabase", order = 1)]
public class ObjectInfoDatabase : ScriptableObject
{
    public List<ObjectMetaData> objectDataList = new List<ObjectMetaData>();
}
