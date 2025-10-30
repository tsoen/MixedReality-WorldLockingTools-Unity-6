// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

#pragma warning disable CS0618

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.XR;

namespace Microsoft.MixedReality.WorldLocking.Core
{
    /// <summary>
    /// Encapsulation of spongy world (raw input) state. Its primary duty is the creation and maintenance
    /// of the graph of (spongy) anchors built up over the space traversed by the camera.
    /// </summary>
    /// <remarks>
    /// Anchor and Edge creation algorithm:
    /// 
    /// Goal: a simple and robust algorithm that guarantees an even distribution of anchors, fully connected by
    /// edges between nearest neighbors with a minimum of redundant edges
    ///
    /// For simplicity, the algorithm should be stateless between time steps
    ///
    /// Rules
    /// * two parameters define spheres MIN and MAX around current position
    /// * whenever MIN does not contain any anchors, a new anchor is created
    /// * when a new anchor is created is is linked by edges to all anchors within MAX
    /// * the MAX radius is 20cm larger than MIN radius which would require 12 m/s beyond world record sprinting speed to cover in one frame
    /// * whenever MIN contains more than one anchor, the anchor closest to current position is connected to all others within MIN 
    /// </remarks>
    public class AnchorManagerWSA : AnchorManager
    {
        /// <inheritdoc/>
        public override bool SupportsPersistence { get { return true; } }

        protected override float TrackingStartDelayTime { get { return SpongyAnchorWSA.TrackingStartDelayTime; } }

        public static AnchorManagerWSA TryCreate(IPlugin plugin, IHeadPoseTracker headTracker)
        {
            if (!UnityEngine.XR.XRSettings.enabled)
            {
                Debug.LogWarning($"Warning: Legacy WSA AnchorManager selected but legacy WSA not enabled. Check Player Settings/XR.");
            }

            AnchorManagerWSA anchorManagerWSA = new AnchorManagerWSA(plugin, headTracker);

            return anchorManagerWSA;
        }

        /// <summary>
        /// Set up an anchor manager.
        /// </summary>
        /// <param name="plugin">The engine interface to update with the current anchor graph.</param>
        private AnchorManagerWSA (IPlugin plugin, IHeadPoseTracker headTracker) : base(plugin, headTracker)
        {
        }

        protected override bool IsTracking()
        {
            return true;
        }

        protected override SpongyAnchor CreateAnchor(AnchorId id, Transform parent, Pose initialPose)
        {
            var newAnchorObject = new GameObject(id.FormatStr());
            newAnchorObject.transform.parent = parent;
            newAnchorObject.transform.SetGlobalPose(initialPose);
            return newAnchorObject.AddComponent<SpongyAnchorWSA>();
        }

        protected override SpongyAnchor DestroyAnchor(AnchorId id, SpongyAnchor spongyAnchor)
        {
            if (spongyAnchor != null)
            {
                GameObject.Destroy(spongyAnchor.gameObject);
            }
            RemoveSpongyAnchorById(id);

            return null;
        }

        protected override async Task SaveAnchors(List<SpongyAnchorWithId> spongyAnchors)
        {
            await Task.CompletedTask;
        }


        /// <summary>
        /// Load the spongy anchors from persistent storage
        /// </summary>
        /// <remarks>
        /// The set of spongy anchors loaded by this routine is defined by the frozen anchors
        /// previously loaded into the plugin.
        /// 
        /// Likewise, when a spongy anchor fails to load, this routine will delete its frozen
        /// counterpart from the plugin.
        /// </remarks>
        protected override async Task LoadAnchors(IPlugin plugin, AnchorId firstId, Transform parent, List<SpongyAnchorWithId> spongyAnchors)
        {
            await Task.CompletedTask;
        }
    }
}
