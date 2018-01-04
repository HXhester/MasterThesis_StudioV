// Copyright © 2015 – Property of Tobii AB (publ) - All Rights Reserved

namespace Tobii.VR
{
    using UnityEngine;
    using System;
    using UnityEngine.UI;

    public class TobiiVR_HMDPositionGuide : MonoBehaviour
    {
        public TobiiVR_HMDPlacementCanvas HMDPlacementCanvas;
        public KeyCode ToggleVisualizationKey = KeyCode.F4;

        private Vector2 _leftPupilXY;
        private Vector2 _rightPupilXY;

        private Vector2 _sizeOfparent;
        private float _lcs;

        void Start()
        {
            TobiiVR_Host.Instance.ValidTrackerData += OnValidData;

            _sizeOfparent = HMDPlacementCanvas.LeftPupil.parent.GetComponent<RectTransform>().sizeDelta;
        }

        public void ToggleVisualization()
        {
            HMDPlacementCanvas.gameObject.SetActive(!HMDPlacementCanvas.gameObject.activeSelf);
        }
        
        private void OnValidData(object sender, EventArgs e)
        {
            _leftPupilXY = TobiiVR_Host.Instance.LatestData.Left.PupilPositionInSensorAreaXy;
            _rightPupilXY = TobiiVR_Host.Instance.LatestData.Right.PupilPositionInSensorAreaXy;
        }

        /// <summary>
        /// Warning, a lot of guesstimating happening right here!!!
        /// </summary>
        void Update()
        {
            if (Input.GetKeyUp(ToggleVisualizationKey))
            {
                ToggleVisualization();
            }

            if (HMDPlacementCanvas.gameObject.activeSelf == false)
            {
                return;
            }

            float lensCupSeparationInM;
            if (TobiiVR_Util.TryGetHmdLensCupSeparationInMeter(out lensCupSeparationInM) == false)
            {
                TobiiVR_Logging.Log("Failed to get hmd lens cup separation.");
            }
            else
            {
                _lcs = lensCupSeparationInM;

                if (TobiiVR_Host.Instance.IsCalibrating == false && Time.frameCount % 45 == 0)
                {
                    TobiiVR_Util.SetLensCupSeparation(_lcs, TobiiVR_Host.Instance.DeviceContext);
                }
            }

            if (HMDPlacementCanvas != null && HMDPlacementCanvas.gameObject.activeSelf)
            {
                var hmdLcsInMM = _lcs * 1000f;

                var lHPos = new Vector3(-hmdLcsInMM, 0);
                var rHPos = new Vector3(hmdLcsInMM, 0);

                HMDPlacementCanvas.TargetLeft.localPosition = lHPos;
                HMDPlacementCanvas.TargetRight.localPosition = rHPos;

                var pupilLeft = new Vector2(
                    _leftPupilXY.x * _sizeOfparent.x,
                    _leftPupilXY.y * _sizeOfparent.y * -1
                    );

                var pupilRight = new Vector2(
                    _rightPupilXY.x * _sizeOfparent.x,
                    _rightPupilXY.y * _sizeOfparent.y * -1
                    );

                if (Mathf.Abs(pupilLeft.y - pupilRight.y) < (0.15f * _sizeOfparent.y))
                {
                    pupilLeft.y = (pupilLeft.y + pupilRight.y) * 0.5f;
                    pupilRight.y = pupilLeft.y;
                }

                HMDPlacementCanvas.LeftPupil.anchoredPosition = pupilLeft;
                HMDPlacementCanvas.RightPupil.anchoredPosition = pupilRight;

                // Coloring
                var c = new Vector2(0.5f, 0.5f);
                var distLeft = Vector2.Distance(c, _leftPupilXY);
                var distRight = Vector2.Distance(c, _rightPupilXY);

                HMDPlacementCanvas.LeftPupil.GetComponent<Image>().color = Color.Lerp(Color.green, Color.red, distLeft / 0.35f);
                HMDPlacementCanvas.RightPupil.GetComponent<Image>().color = Color.Lerp(Color.green, Color.red, distRight / 0.35f);

                // Info to the user
                HMDPlacementCanvas.Status.text = distLeft + distRight < 0.25f ? "Awesome!" : "OK";
            }
        }
    }
}