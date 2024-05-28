using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Editor.EMeshCombiner
{
    public class MeshCombiner
    {
        public GameObject CombineMeshes(GameObject[] objects)
        {
            List<MeshFilter> meshFilters = new List<MeshFilter>();
            List<Material> materials = new List<Material>();

            foreach (var obj in objects)
            {
                MeshFilter meshFilter = obj.GetComponent<MeshFilter>();
                MeshRenderer meshRenderer = obj.GetComponent<MeshRenderer>();
                if (meshFilter != null && meshRenderer != null)
                {
                    meshFilters.Add(meshFilter);
                    foreach (var mat in meshRenderer.sharedMaterials)
                    {
                        if (!materials.Contains(mat))
                        {
                            materials.Add(mat);
                        }
                    }
                }
            }

            List<CombineInstance> combineInstances = new List<CombineInstance>();
            for (int i = 0; i < materials.Count; i++)
            {
                List<CombineInstance> subCombineInstances = new List<CombineInstance>();
                foreach (var meshFilter in meshFilters)
                {
                    MeshRenderer meshRenderer = meshFilter.GetComponent<MeshRenderer>();
                    if (meshRenderer != null)
                    {
                        for (int j = 0; j < meshRenderer.sharedMaterials.Length; j++)
                        {
                            if (meshRenderer.sharedMaterials[j] == materials[i])
                            {
                                CombineInstance ci = new CombineInstance
                                {
                                    mesh = meshFilter.sharedMesh,
                                    subMeshIndex = j,
                                    transform = meshFilter.transform.localToWorldMatrix
                                };
                                subCombineInstances.Add(ci);
                            }
                        }
                    }
                }

                Mesh subMesh = new Mesh();
                subMesh.CombineMeshes(subCombineInstances.ToArray(), true, true);
                CombineInstance finalCombine = new CombineInstance
                {
                    mesh = subMesh,
                    subMeshIndex = 0
                };
                combineInstances.Add(finalCombine);
            }

            Mesh combinedMesh = new Mesh();
            combinedMesh.CombineMeshes(combineInstances.ToArray(), false, false);

            GameObject mergedObject = new GameObject("MergedMesh");
            MeshFilter meshFilterComponent = mergedObject.AddComponent<MeshFilter>();
            MeshRenderer meshRendererComponent = mergedObject.AddComponent<MeshRenderer>();

            meshFilterComponent.sharedMesh = combinedMesh;
            meshRendererComponent.sharedMaterials = materials.ToArray();

            foreach (GameObject obj in objects)
            {
                Undo.RecordObject(obj, "Disable Merged Objects");
                obj.SetActive(false);
            }

            return mergedObject;
        }
    }
}
