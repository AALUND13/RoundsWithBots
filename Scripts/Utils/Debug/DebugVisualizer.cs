using RoundsWithBots.Menu;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace RoundsWithBots.Utils.Debug {
    internal class DebugVisualizer : MonoBehaviour {
        [Flags]
        public enum VisualizerCategory {
            None = 0,

            MapScanner = 1 << 0,
            Bots = 1 << 1,
        }

        class DebugObject {
            public GameObject gameObject;
            public bool callLastFrame;
        }

        public static VisualizerCategory EnableCategory => RWBMenu.GetEnableVisualizer();

        private static DebugVisualizer instance;

        private readonly Dictionary<string, DebugObject> debugSphereObjects = new Dictionary<string, DebugObject>();
        private readonly Dictionary<string, DebugObject> debugLineObjects = new Dictionary<string, DebugObject>();
        private readonly Dictionary<string, DebugObject> debugTextObjects = new Dictionary<string, DebugObject>();
        private Material debugSphereMaterial;
        private Material debugLineMaterial;

        private void Awake() {
            if (instance != null) {
                Destroy(this);
                return;
            }

            instance = this;

            debugSphereMaterial = new Material(Shader.Find("Unlit/Color"));
            debugLineMaterial = new Material(Shader.Find("Sprites/Default"));
        }

        private void LateUpdate() {
            RemoveNonCallObject(debugSphereObjects);
            RemoveNonCallObject(debugLineObjects);
            RemoveNonCallObject(debugTextObjects);
        }


        private List<string> RemoveNonCallObject(Dictionary<string, DebugObject> dictionary) {
            List<string> keysToRemove = new List<string>();

            foreach (var kvp in dictionary) {
                if (!kvp.Value.callLastFrame) {
                    Destroy(kvp.Value.gameObject);
                    keysToRemove.Add(kvp.Key);
                } else {
                    kvp.Value.callLastFrame = false;
                }
            }
            foreach (var key in keysToRemove) {
                dictionary.Remove(key);
            }

            return keysToRemove;
        }

        private GameObject InternalCreateDebugSphere(string name, Color color, Vector2 position, float size = 0.5f) {
            if (debugSphereObjects.ContainsKey(name)) {
                DebugObject debugObj = debugSphereObjects[name];
                Renderer debugRenderer = debugObj.gameObject.GetComponent<Renderer>();
                debugRenderer.material.color = color;
                debugObj.gameObject.transform.localPosition = new Vector3(position.x, position.y, -5f);
                debugObj.callLastFrame = true;
                return debugObj.gameObject;
            }

            GameObject debugObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            Renderer renderer = debugObject.GetComponent<Renderer>();

            renderer.material = debugSphereMaterial;
            renderer.material.color = color;

            debugObject.name = name;
            debugObject.transform.localScale = Vector3.one * size;
            debugObject.transform.localPosition = new Vector3(position.x, position.y, -5f);

            DebugObject debugObjStruct = new DebugObject {
                gameObject = debugObject,
                callLastFrame = true
            };

            debugSphereObjects.Add(name, debugObjStruct);
            return debugObject;
        }

        private GameObject InternalCreateDebugLine(string name, Color color, Vector2 firstPoint, Vector2 secondPoint) {
            if (debugLineObjects.ContainsKey(name)) {
                DebugObject debugObj = debugLineObjects[name];
                LineRenderer line = debugObj.gameObject.GetComponent<LineRenderer>();

                line.SetPositions(new Vector3[2] { new Vector3(firstPoint.x, firstPoint.y, -5f), new Vector3(secondPoint.x, secondPoint.y, -5f) });
                line.startColor = color;
                line.endColor = color;

                debugObj.callLastFrame = true;
                return debugObj.gameObject;
            }

            GameObject debugLineRender = new GameObject(name);
            LineRenderer lineRenderer = debugLineRender.AddComponent<LineRenderer>();

            lineRenderer.SetPositions(new Vector3[2] { new Vector3(firstPoint.x, firstPoint.y, -5f), new Vector3(secondPoint.x, secondPoint.y, -5f) });

            lineRenderer.material = debugLineMaterial;
            lineRenderer.startColor = color;
            lineRenderer.endColor = color;

            lineRenderer.startWidth = 0.25f;
            lineRenderer.endWidth = 0.25f;

            DebugObject debugObjStruct = new DebugObject {
                gameObject = debugLineRender,
                callLastFrame = true
            };

            debugLineObjects.Add(name, debugObjStruct);
            return debugLineRender;
        }

        private GameObject InternalCreateDebugText(string name, Vector2 position, Color color, string text) {
            if (debugTextObjects.TryGetValue(name, out DebugObject debugObj)) {
                TextMeshPro tmp = debugObj.gameObject.GetComponent<TextMeshPro>();
                tmp.sortingOrder = 1000;

                tmp.text = text;
                tmp.color = color;

                debugObj.gameObject.transform.position = new Vector3(position.x, position.y, -5f);
                debugObj.callLastFrame = true;

                return debugObj.gameObject;
            }

            GameObject debugObject = new GameObject(name);

            TextMeshPro tmpText = debugObject.AddComponent<TextMeshPro>();
            tmpText.sortingOrder = 1000;

            tmpText.text = text;
            tmpText.color = color;
            tmpText.fontSize = 6;
            tmpText.alignment = TextAlignmentOptions.Left;
            tmpText.enableWordWrapping = false;

            debugObject.transform.position = new Vector3(position.x, position.y, -5f);

            DebugObject debugObjStruct = new DebugObject {
                gameObject = debugObject,
                callLastFrame = true
            };

            debugTextObjects.Add(name, debugObjStruct);

            return debugObject;
        }


        public static GameObject RenderDebugSphere(VisualizerCategory category, string name, Color color, Vector2 position, float size = 0.5f) {
            if (instance == null || !EnableCategory.HasFlag(category)) {
                return null;
            }
            return instance.InternalCreateDebugSphere(name, color, position, size);
        }

        public static GameObject RenderDebugLine(VisualizerCategory category, string name, Color color, Vector2 firstPoint, Vector2 secondPoint) {
            if (instance == null || !EnableCategory.HasFlag(category)) {
                return null;
            }
            return instance.InternalCreateDebugLine(name, color, firstPoint, secondPoint);
        }

        public static GameObject RenderDebugText(VisualizerCategory category, string name, Vector2 position, Color color, string text) {
            if (instance == null || !EnableCategory.HasFlag(category))
                return null;

            return instance.InternalCreateDebugText(name, position, color, text);
        }
    }
}
