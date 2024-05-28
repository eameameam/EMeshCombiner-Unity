using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Editor.EMeshCombiner
{
    public class MeshMergerWindow : EditorWindow
    {
        private List<GameObject> _objectsToMerge = new List<GameObject>();
        private string _savePath = "Assets/MergedMeshes";
        private string _meshName = "MergedMesh";

        [MenuItem("Escripts/EMeshMerger")]
        public static void ShowWindow()
        {
            GetWindow<MeshMergerWindow>("Mesh Merger");
        }

        private void OnGUI()
        {
            GUILayout.Label("Select Objects to Merge", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("+", GUILayout.Width(20)))
            {
                _objectsToMerge.Add(null);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginVertical();

            GUILayout.Space(10);

            Event evt = Event.current;
            Rect dropArea = GUILayoutUtility.GetRect(0.0f, 50.0f, GUILayout.ExpandWidth(true));
            GUI.Box(dropArea, "Drag Objects Here to Add to the List");

            switch (evt.type)
            {
                case EventType.DragUpdated:
                case EventType.DragPerform:
                    if (!dropArea.Contains(evt.mousePosition))
                        break;

                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                    if (evt.type == EventType.DragPerform)
                    {
                        DragAndDrop.AcceptDrag();
                        foreach (Object draggedObject in DragAndDrop.objectReferences)
                        {
                            GameObject go = draggedObject as GameObject;
                            if (go != null)
                            {
                                _objectsToMerge.Add(go);
                            }
                        }
                    }
                    break;
            }
            
            GUILayout.Space(10);

            for (int i = 0; i < _objectsToMerge.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                _objectsToMerge[i] = (GameObject)EditorGUILayout.ObjectField(_objectsToMerge[i], typeof(GameObject), true);
                if (GUILayout.Button("-", GUILayout.Width(20)))
                {
                    _objectsToMerge.RemoveAt(i);
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();

            GUILayout.Space(10);

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Select Path", EditorStyles.helpBox);

            if (GUILayout.Button("Browse", GUILayout.Width(70)))
            {
                string selectedPath = EditorUtility.SaveFolderPanel("Select Save Folder", _savePath, "");
                if (!string.IsNullOrEmpty(selectedPath))
                {
                    _savePath = "Assets" + selectedPath.Substring(Application.dataPath.Length);
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(_savePath, EditorStyles.miniLabel);
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(10);

            _meshName = EditorGUILayout.TextField("Mesh Name", _meshName);

            GUILayout.Space(10);

            GUI.enabled = !string.IsNullOrEmpty(_savePath) && !string.IsNullOrEmpty(_meshName);
            if (GUILayout.Button("Merge Meshes"))
            {
                Undo.RecordObject(this, "Merge Meshes");
                MergeMeshes();
            }
            GUI.enabled = true;
        }

        private void MergeMeshes()
        {
            if (_objectsToMerge == null || _objectsToMerge.Count == 0)
            {
                Debug.LogError("No objects selected for merging.");
                return;
            }

            MeshCombiner combiner = new MeshCombiner();
            GameObject mergedObject = combiner.CombineMeshes(_objectsToMerge.ToArray(), _savePath, _meshName);

            if (mergedObject != null)
            {
                Transform commonParent = TransformUtility.FindCommonParent(_objectsToMerge.ToArray());
                if (commonParent != null)
                {
                    mergedObject.transform.SetParent(commonParent, false);
                }

                Selection.activeObject = mergedObject;
                Undo.RegisterCreatedObjectUndo(mergedObject, "Merged Mesh Object");
            }
        }
    }
}
