using JetBrains.Annotations;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDK3.Components;
using VRC.SDKBase;

namespace UdonSharp.Video
{
    [DefaultExecutionOrder(10)]
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    [AddComponentMenu("Udon Sharp/Video/UI/Video Control Handler")]
    public class VideoControlHandler : UdonSharpBehaviour
    {
        /// <summary>
        /// The video player this UI instance controls and pulls info from
        /// </summary>
        [PublicAPI, NotNull]
        public USharpVideoPlayer targetVideoPlayer;
        
#pragma warning disable CS0649
        [SerializeField]
        private VRCUrlInputField urlField;

        [SerializeField]
        private Text urlFieldPlaceholderText;

        [Header("Status text")]
        [SerializeField]
        private Text statusTextField;

        [SerializeField]
        private Text statusTextDropShadow;

        [Header("Video progress bar")]
        [SerializeField]
        private Slider progressSlider;

        [Header("Lock button")]
        [SerializeField]
        private Graphic lockGraphic;

        [SerializeField]
        private GameObject masterLockedIcon, masterUnlockedIcon;

        [Header("Info panel fields")]
        [SerializeField]
        private Text masterField;

        [SerializeField]
        private Text ownerField;

        [SerializeField]
        private InputField currentURLField, previousURLField;

        [Header("Play/Pause/Stop buttons")]
        [SerializeField]
        private GameObject pauseStopObject;

        [SerializeField]
        private GameObject playObject;

        [SerializeField]
        private GameObject pauseIcon, stopIcon;

        [Header("Loop button")]
        [SerializeField]
        private Graphic loopButtonBackground;

        [SerializeField]
        private Graphic loopButtonIcon;

        [Header("Video/Stream controls")]
        [SerializeField]
        private GameObject videoControls;
        
        [SerializeField]
        private GameObject streamControls;

        [SerializeField]
        private Graphic videoModeButtonBackground;

        [SerializeField]
        private Graphic videoModeButtonIcon;

        [SerializeField]
        private Graphic streamModeButtonBackground;

        [SerializeField]
        private Graphic streamModeButtonIcon;

        [SerializeField]
        private RectTransform screenFitterObject;

        [SerializeField]
        private Transform[] renderTargets;

        [Header("Stream player controls")]
        [SerializeField]
        private GameObject avProRestartButton;

        [SerializeField]
        private GameObject avProModeIcon;

        [SerializeField]
        private GameObject avProModeButton;

        [Header("Error UI Elements")]
        [SerializeField]
        private GameObject errorIconObject;

        [SerializeField]
        private Text errorMessageText;

        [SerializeField]
        private GameObject errorRetryButton;

        [SerializeField]
        private GameObject errorDetailsButton;

        private RectTransform _currentScreenFitter;
        private Vector3 _initialFitterScale;

        private int _lastKnownWidth;
        private int _lastKnownHeight;

        private bool _fetching;

        public void OnFetchVideoResolution(int width, int height)
        {
            _lastKnownWidth = width;
            _lastKnownHeight = height;

            // Adjusts the video render target's aspect ratio based on the video's resolution
            float videoAspect = (float)width / height;
            Vector2 fitterSize = screenFitterObject.sizeDelta;
            float fitterAspect = fitterSize.x / fitterSize.y;

            _currentScreenFitter = screenFitterObject;
            _initialFitterScale = _currentScreenFitter.localScale;

            if (videoAspect > fitterAspect)
            {
                _currentScreenFitter.localScale = new Vector3(1, fitterAspect / videoAspect, 1);
            }
            else
            {
                _currentScreenFitter.localScale = new Vector3(videoAspect / fitterAspect, 1, 1);
            }
        }

        public void SetToUnityPlayer()
        {
            targetVideoPlayer.SetToUnityPlayer();
        }

        public void SetToStreamPlayer()
        {
            targetVideoPlayer.SetToUnityPlayer();
        }

        private void Update()
        {
            if (!_fetching)
            {
                _fetching = true;

                // Update UI based on video player state
                statusTextField.text = targetVideoPlayer.GetStatusText();
                statusTextDropShadow.text = targetVideoPlayer.GetStatusText();
                loopButtonBackground.color = targetVideoPlayer.IsLooping() ? Color.green : Color.red;
                muteToggle.isOn = targetVideoPlayer.IsMuted();
                volumeSlider.value = targetVideoPlayer.GetVolume();

                _fetching = false;
            }
        }

        public void OnVolumeChanged()
        {
            targetVideoPlayer.SetVolume(volumeSlider.value);
        }

        public void OnMuteToggled()
        {
            targetVideoPlayer.SetMuted(muteToggle.isOn);
        }

        public void OnLoopToggled()
        {
            targetVideoPlayer.SetLooping(loopToggle.isOn);
        }

        public void OnSeekSliderBeginDrag()
        {
            if (Networking.IsOwner(targetVideoPlayer.gameObject))
                _draggingSlider = true;
        }

        public void OnSeekSliderEndDrag()
        {
            _draggingSlider = false;
        }

        public void OnSeekSliderChanged()
        {
            if (!_draggingSlider)
                return;

            float progress = progressSlider.value / progressSlider.maxValue;
            targetVideoPlayer.SeekTo(progress);
        }

        public void SetElapsedTime(float elapsedTime)
        {
            elapsedTimeText.text = GetFormattedTime(System.TimeSpan.FromSeconds(elapsedTime));
        }

        public void SetTotalTime(float totalTime)
        {
            totalTimeText.text = GetFormattedTime(System.TimeSpan.FromSeconds(totalTime));
        }

        private string GetFormattedTime(System.TimeSpan time)
        {
            return ((int)time.TotalHours).ToString("D2") + time.ToString(@"\:mm\:ss");
        }

        /// <summary>
        /// Called when the user enters a URL in the url input field, forwards the input to the video player
        /// </summary>
        public void OnURLInput()
        {
            targetVideoPlayer.PlayVideo(urlField.GetUrl());
            urlField.SetUrl(VRCUrl.Empty);
        }

        /// <summary>
        /// Fired when the play button, pause button, or stop button are pressed. 
        /// </summary>
        public void OnPlayButtonPress()
        {
            targetVideoPlayer.SetPaused(!targetVideoPlayer.IsPaused());
        }

        public void OnLockButtonPress()
        {
            if (targetVideoPlayer.IsPrivilegedUser(Networking.LocalPlayer))
            {
                targetVideoPlayer.TakeOwnership();
                targetVideoPlayer.SetLocked(!targetVideoPlayer.IsLocked());
            }
        }

        public void OnReloadButtonPressed()
        {
            targetVideoPlayer.Reload();
        }

        public void OnLoopButtonPressed()
        {
            targetVideoPlayer.TakeOwnership();
            targetVideoPlayer.SetLooping(!targetVideoPlayer.IsLooping());
        }

        public void OnVideoPlayerModeButtonPressed()
        {
            targetVideoPlayer.SetToUnityPlayer();
        }

        public void SetMuted(bool muted)
        {
            if (volumeController) volumeController.SetMuted(muted);
        }

        public void SetVolume(float volume)
        {
            if (volumeController) volumeController.SetVolume(volume);
        }

        public void SetPaused(bool paused)
        {
            bool videoMode = targetVideoPlayer.IsInVideoMode();

            if (pauseIcon) pauseIcon.SetActive(videoMode);
            if (stopIcon) stopIcon.SetActive(!videoMode);

            if (playObject) playObject.SetActive(paused);
            if (pauseStopObject) pauseStopObject.SetActive(!paused);
        }

        public void SetLocked(bool locked)
        {
            if (locked)
            {
                if (masterLockedIcon) masterLockedIcon.SetActive(true);
                if (masterUnlockedIcon) masterUnlockedIcon.SetActive(false);

                if (Networking.IsOwner(targetVideoPlayer.gameObject) || targetVideoPlayer.CanControlVideoPlayer())
                {
                    if (lockGraphic) lockGraphic.color = whiteGraphicColor;
                    if (urlFieldPlaceholderText) urlFieldPlaceholderText.text = "Enter Video URL...";
                }
                else
                {
                    if (lockGraphic) lockGraphic.color = redGraphicColor;
                    if (urlFieldPlaceholderText) urlFieldPlaceholderText.text = $"Only the master {Networking.GetOwner(targetVideoPlayer.gameObject).displayName} may add URLs";
                }
            }
            else
            {
                if (masterLockedIcon) masterLockedIcon.SetActive(false);
                if (masterUnlockedIcon) masterUnlockedIcon.SetActive(true);
                if (lockGraphic) lockGraphic.color = whiteGraphicColor;
                if (urlFieldPlaceholderText) urlFieldPlaceholderText.text = "Enter Video URL... (anyone)";
            }
        }

        public void SetStatusText(string newStatus)
        {
            _currentStatusText = newStatus;
            if (statusTextField) statusTextField.text = _currentStatusText;
            if (statusTextDropShadow) statusTextDropShadow.text = _currentStatusText;
            _lastTime = int.MaxValue;
        }

        public void SetControlledVideoPlayer(USharpVideoPlayer newPlayer)
        {
            if (newPlayer == targetVideoPlayer)
                return;

            targetVideoPlayer.UnregisterControlHandler(this);
            targetVideoPlayer = newPlayer;
            targetVideoPlayer.RegisterControlHandler(this);
            UpdateVideoOwner();

            SetStatusText("");
            _draggingSlider = false;
        }

        public override void OnPlayerLeft(VRCPlayerApi player)
        {
            UpdateMaster();
        }

        public void OnVideoPlayerOwnerTransferred()
        {
            UpdateVideoOwner();
        }
        void UpdateMaster()
        {
        #if !UNITY_EDITOR
        // We know the owner of this will always be the master so just get the owner and update the name
            if (masterField)
            {
                VRCPlayerApi owner = Networking.GetOwner(gameObject);
                if (owner != null && owner.IsValid())
                    masterField.text = Networking.GetOwner(gameObject).displayName;
            }
        #endif
        }

        private void UpdateVideoOwner()
        {
        #if !UNITY_EDITOR
            if (ownerField)
                ownerField.text = Networking.GetOwner(targetVideoPlayer.gameObject).displayName;
        #endif

            SetLocked(targetVideoPlayer.IsLocked());
        }
    }
}
