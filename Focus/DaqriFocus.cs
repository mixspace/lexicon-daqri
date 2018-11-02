// Copyright (c) 2018 Mixspace Technologies, LLC. All rights reserved.

using Mixspace.Lexicon;
using UnityEngine;

namespace Mixspace.Lexicon.Samples
{
    public class DaqriFocus : MonoBehaviour
    {
        public float dwellSpeed = 0.1f;

        private LexiconFocusManager focusManager;

        private Vector3 lastPosition;

        private GameObject reticle;

        void Start()
        {
            focusManager = LexiconFocusManager.Instance;
            reticle = GameObject.Find("Reticle");
            if (reticle == null)
            {
                Debug.LogError("DaqriFocus requires a gameobject name Reticle");
                enabled = false;
            }
        }

        private void LateUpdate()
        {
            CaptureFocus();
        }

        public void CaptureFocus()
        {
            // Get a focus position data entry from the pool.
            FocusPosition focusPosition = focusManager.GetPooledData<FocusPosition>();

            Ray cameraRay = new Ray(reticle.transform.position, reticle.transform.forward);
            RaycastHit hit;
            if (Physics.Raycast(cameraRay, out hit))
            {
                // This sample uses a LexiconSelectable component to determine object selectability.
                // You could just as easily use layers, tags, or your own scripts.
                LexiconSelectable selectable = hit.collider.gameObject.GetComponentInParent<LexiconSelectable>();
                if (selectable != null)
                {
                    FocusSelection focusSelection = focusManager.GetPooledData<FocusSelection>();
                    focusSelection.SelectedObject = selectable.gameObject;
                    focusManager.AddFocusData(focusSelection);
                }

                // Set the focus position to the hit point if present.
                focusPosition.Position = hit.point;
                focusPosition.Normal = hit.normal;
            }
            else
            {
                // Set the focus position in front of the camera if no hit point.
                focusPosition.Position = reticle.transform.position;
                focusPosition.Normal = -reticle.transform.forward;
            }

            // Add our focus position data entry.
            focusManager.AddFocusData(focusPosition);

            // Add a dwell position entry if the user's gaze is lingering on a spot.
            float speed = Vector3.Magnitude(lastPosition - focusPosition.Position) / Time.deltaTime;
            if (speed < dwellSpeed)
            {
                FocusDwellPosition dwellPosition = focusManager.GetPooledData<FocusDwellPosition>();
                dwellPosition.Position = focusPosition.Position;
                dwellPosition.Normal = focusPosition.Normal;
                focusManager.AddFocusData(dwellPosition);
            }

            lastPosition = focusPosition.Position;
        }
    }
}
