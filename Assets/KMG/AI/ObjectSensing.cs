using System.Collections.Generic;
using System;
using UnityEngine;

namespace KMGAI {
    public static class ObjectSensing {
        public struct ObjectSensor {
            [SerializeField] private Transform _parent;
            public Vector3 localPosition;
            public Vector3 localRotation;
            public float fieldOfView;
            public float range;

            public Vector3 position {
                get {
                    return _parent.TransformPoint(localPosition);
                }
            }
            public Vector3 forward {
                get {
                    return _parent.rotation * Quaternion.Euler(localRotation) * Vector3.forward;
                }
            }
            public void Draw() {
                GizmoUtilities.DrawSphereSegment(position, forward, range, fieldOfView);
            }

            public ObjectSensor(Transform parent) {
                this._parent = parent;
                localPosition = localRotation = new Vector3();
                fieldOfView = 70;
                range = 5;
            }

            public void SetParent(Transform t) {
                _parent = t;
            }

            public static List<Collider> SenseObjects(ObjectSensor sensor, Func<Collider, bool> ignoreFunc = null) {
                Vector3 sensorPos = sensor.position;
                Vector3 sensorFwd = sensor.forward;
                var sensedObjects = new List<Collider>();
                foreach (var overlap in Physics.OverlapSphere(sensorPos, sensor.range)) {
                    if (ignoreFunc(overlap)) {
                        continue;
                    }
                    Vector3 r = overlap.transform.position - sensorPos;
                    if (Vector3.Angle(sensorFwd, r) > sensor.fieldOfView / 2) {
                        continue;
                    }
                    foreach (var hit in Physics.RaycastAll(sensorPos, r, sensor.range)) {
                        if (ignoreFunc(overlap)) {
                            continue;
                        }
                        if (hit.collider == overlap) {
                            sensedObjects.Add(overlap);
                        }
                        break;
                    }
                }
                return sensedObjects;
            }
        }
    }
}