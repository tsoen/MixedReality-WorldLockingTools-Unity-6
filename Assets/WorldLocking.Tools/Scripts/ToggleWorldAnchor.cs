// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Microsoft.MixedReality.WorldLocking.Core;

namespace Microsoft.MixedReality.WorldLocking.Tools
{
    public class ToggleWorldAnchor : MonoBehaviour
    {
        protected IAttachmentPoint AttachmentPoint { get; private set; }

        private bool frozenPoseIsSpongy = false;
        private Pose frozenPose = Pose.identity;

        [SerializeField]
        [Tooltip("Always use WorldAnchor to world lock, whether Frozen World is active or not.")]
        private bool alwaysLock = false;
        /// <summary>
        /// Always use WorldAnchor to world lock, whether Frozen World is active or not.
        /// </summary>
        public bool AlwaysLock { get { return alwaysLock; } set { alwaysLock = value; } }

        // Start is called before the first frame update
        void Start()
        {
            Debug.Assert(WorldLockingManager.GetInstance() != null, "Unexpected null WorldLockingManager");
            // dummy use of variables to silence unused variable warning in non-WSA build.
            if (frozenPoseIsSpongy)
            {
                frozenPose = Pose.identity;
            }
        }
    }
}
