// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

#pragma warning disable CS0618

using UnityEngine;
using Microsoft.MixedReality.WorldLocking.Core;

namespace Microsoft.MixedReality.WorldLocking.Tools
{
    /// <summary>
    /// Simple class to adapt Unity's input results from spongy space into frozen space.
    /// This is unnecessary when using MRTK's input system, which already provides this
    /// and other enhancements and abstactions.
    /// </summary>
    public class FrozenTapToAdd : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("The prefab to place in the world at gaze position on air taps.")]
        private GameObject prefabToPlace = null;
        /// <summary>
        /// The prefab to place in the world at gaze position on air taps.
        /// </summary>
        public GameObject PrefabToPlace => prefabToPlace;

        /// <summary>
        /// Enable and disable processing of tap events.
        /// </summary>
        public bool Active { get; set; }

        private WorldLockingManager manager {  get { return WorldLockingManager.GetInstance(); } }
    }
}
