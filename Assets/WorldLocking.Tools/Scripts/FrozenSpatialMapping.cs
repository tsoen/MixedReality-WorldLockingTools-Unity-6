// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

#pragma warning disable CS0618

using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Assertions;
using System;
using System.Collections.Generic;

using Microsoft.MixedReality.WorldLocking.Core;

namespace Microsoft.MixedReality.WorldLocking.Tools
{
    /// <summary>
    /// Class to reinterpret spatial mapping data from "spongy" space into "frozen" space.
    /// This is unnecessary when using MRTK's spatial mapping, which provides this and
    /// other enhancements over the native spatial mapping.
    /// </summary>
    public class FrozenSpatialMapping : MonoBehaviour
    {
        /// <summary>
        /// The 3 states of baking a surface can be in.
        /// </summary>
        private enum BakedState
        {
            NeverBaked = 0,
            Baked,
            UpdatePostBake
        }

        /// <summary>
        /// This class holds data that is kept by the system to prioritize Surface baking. 
        /// </summary>
        private class SurfaceEntry
        {
            public GameObject surfaceObject = null; // the GameObject this surface hangs off of.
            public GameObject worldAnchorChild = null; // the GameObject child of surfaceObject generated to hold the WorldAnchor
            public int handle = 0; // this surface's identifier
            public DateTime updateTime = DateTime.MinValue; // update time as reported by the system
            public BakedState currentState = BakedState.NeverBaked;
        }

        /// <summary>
        /// Store known surfaces by handle.
        /// </summary>
        private readonly Dictionary<int, SurfaceEntry> surfaces = new Dictionary<int, SurfaceEntry>();

        /// <summary>
        /// Frozen World Manager for conversion to Frozen space.
        /// </summary>
        private WorldLockingManager manager { get { return WorldLockingManager.GetInstance(); } }

        [SerializeField]
        [Tooltip("Whether the Mapping is active. If inactive, all resources disposed and only remade when active again.")]
        private bool active = true;
        /// <summary>
        /// Whether the Mapping is active. If inactive, all resources disposed and only remade when active again.
        /// </summary>
        public bool Active { get { return active; } set { active = value; } }

        [SerializeField]
        [Tooltip("Material to draw surfaces with. May be null if no display wanted.")]
        private Material drawMaterial = null;
        /// <summary>
        /// Material to draw surfaces with. May be null if no display wanted. 
        /// </summary>
        public Material DrawMaterial => drawMaterial;

        [SerializeField]
        [Tooltip("Whether to render the active surfaces with the given material")]
        private bool display = true;
        /// <summary>
        /// Whether to render the active surfaces with the given material.
        /// </summary>
        public bool Display {
            get { return display && (DrawMaterial != null); }
            set
            {
                // Note that changing the value of display might not change the value of Display (if displayMat == null).
                display = value;
            }
        }

        [SerializeField]
        [Tooltip("Whether to perform collisions and raycasts against these surfaces")]
        private bool collide = true;
        /// <summary>
        /// Whether to perform collisions and raycasts against these surfaces.
        /// </summary>
        public bool Collide => collide;

        [SerializeField]
        [Tooltip("Object to attach surface objects to. May be null to add surface objects to scene root.")]
        private Transform hangerObject = null;
        /// <summary>
        /// Object to attach surface objects to. May be null to add surface objects to scene root.
        /// </summary>
        public Transform HangerObject => hangerObject;

        [SerializeField]
        [Tooltip("Object around which spatial mappings are centered. Set to null to center around the camera.")]
        private Transform centerObject = null;
        /// <summary>
        /// Object around which spatial mappings are centered. Set to null to center around the camera.
        /// </summary>
        public Transform CenterObject => centerObject;

        [SerializeField]
        [Tooltip("Period in seconds at which to update surfaces.")]
        private float updatePeriod = 2.5f;
        /// <summary>
        /// Period in seconds at which to update surfaces.
        /// </summary>
        public float UpdatePeriod => updatePeriod;

        [SerializeField]
        [Tooltip("Radius around the camera to map.")]
        private float radius = 7.0f;
        /// <summary>
        /// Radius around the camera to map.
        /// </summary>
        public float Radius => radius;

        /// <summary>
        /// Supported tessellation quality levels.
        /// </summary>
        public enum QualityType
        {
            Low = 0,
            Medium = 1,
            High = 2
        };

        [SerializeField]
        [Tooltip("Quality at which to tessellate.")]
        private QualityType quality = QualityType.Medium;
        /// <summary>
        /// Quality at which to tessellate.
        /// </summary>
        public QualityType Quality => quality;

        /// <summary>
        /// Convert an abstract quality to a numeric value suitable for input to spatial mapping system.
        /// </summary>
        private float TrianglesPerCubicMeter
        {
            get
            {
                switch(quality)
                {
                    case QualityType.Low:
                        return 100.0f;
                    case QualityType.Medium:
                        return 300.0f;
                    case QualityType.High:
                        return 600.0f;
                }
                Debug.Assert(false, $"Quality set to invalid value {quality}.");
                return 300.0f;
            }
        }

        /// <summary>
        /// Flag to gate requests to the SurfaceObserver. Only one baking request (via RequestMeshAsync)
        /// is ever in flight at one time.
        /// </summary>
        private bool waitingForBake = false;

        /// <summary>
        /// Countdown to next time to update the surface observer.
        /// </summary>
        private float updateCountdown = 0.0f;

        /// <summary>
        /// Cached spatial mapping layer.
        /// </summary>
        private int spatialMappingLayer = -1;
        
        private void Start()
        {
            spatialMappingLayer = LayerMask.NameToLayer("SpatialMapping");
            // dummy use of variables to silence unused variable warning in non-WSA build.
            if (updateCountdown > 0 && waitingForBake)
            {
                updateCountdown = 0;
            }
        }
    }
}
