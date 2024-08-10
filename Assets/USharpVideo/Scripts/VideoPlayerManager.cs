using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.SDK3.Video.Components;
using VRC.SDK3.Video.Components.Base;

#if UNITY_EDITOR && !COMPILER_UDONSHARP
using UnityEditor;
using UdonSharpEditor;
#endif

#pragma warning disable CS0612 // Type or member is obsolete

namespace UdonSharp.Video
{
    /// <summary>
    /// Forwards events sent by Udon video player components to the main video player controller and further abstracts the video players between AVPro and Unity players
    /// This exists so that we can put the Udon video player components on a different object from the main video player
    /// Prior to using this, people would get confused and change settings on the Udon video player components which would break things
    /// </summary>
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    [AddComponentMenu("Udon Sharp/Video/Internal/Video Player Manager")]
    public class VideoPlayerManager : UdonSharpBehaviour
    {
        public USharpVideoPlayer receiver;

        public VRCUnityVideoPlayer unityVideoPlayer;
        public VideoPlayer unityVideoPlayer;
        public Renderer unityTextureRenderer;
        
        public AudioSource[] audioSources;

        private BaseVRCVideoPlayer _currentPlayer;
        private MaterialPropertyBlock _fetchBlock;
        

        private bool _initialized;

        public void Start()
        {
            if (_initialized)
                return;

            _currentPlayer = unityVideoPlayer;

            RenderTexture targetTexture = unityVideoPlayer.targetTexture;
            Material m = unityTextureRenderer.material;
            _fetchBlock = new MaterialPropertyBlock();
            unityFetchMaterial = unityTextureRenderer.material;

            _initialized = true;
        }

        public override void OnVideoEnd()
        {
            receiver.OnVideoEnd();
        }

        public override void OnVideoError(VRC.SDK3.Components.Video.VideoError videoError)
        {
            receiver.OnVideoError(videoError);
        }

        public override void OnVideoLoop()
        {
            receiver.OnVideoLoop();
        }

        public override void OnVideoStart()
        {
            receiver.OnVideoStart();
        }

        public override void OnVideoReady()
        {
            receiver.OnVideoReady();
        }

        public override void OnVideoPause()
        {
            receiver.OnVideoPause();
        }

        public override void OnVideoPlay()
        {
            receiver.OnVideoPlay();
        }

        public override void OnVideoStop()
        {
            receiver.OnVideoStop();
        }

        public void _UpdateFetchBlock()
        {
            unityTextureRenderer.GetPropertyBlock(_fetchBlock);
            _fetchBlock.SetTexture("_MainTex", unityVideoPlayer.targetTexture);
            unityTextureRenderer.SetPropertyBlock(_fetchBlock);
        }

        public void _PlayVideo()
        {
            unityVideoPlayer.Play();
            _UpdateFetchBlock();
        }

        public void _StopVideo()
        {
            unityVideoPlayer.Stop();
            _UpdateFetchBlock();
        }

        public void _PauseVideo()
        {
            unityVideoPlayer.Pause();
            _UpdateFetchBlock();
        }

        public void _SetVideoTime(float time)
        {
            unityVideoPlayer.time = time;
            _UpdateFetchBlock();
        }

        public void _SetVideoUrl(string url)
        {
            unityVideoPlayer.url = url;
            _UpdateFetchBlock();
        }

        public void _SetAudioVolume(float volume)
        {
            foreach (var audioSource in audioSources)
            {
                audioSource.volume = volume;
            }
        }

#if UNITY_EDITOR && !COMPILER_UDONSHARP
        [CustomEditor(typeof(VideoPlayerManager))]
        internal class VideoPlayerManagerInspector : Editor
        {
            private SerializedProperty receiverProperty;
            private SerializedProperty unityVideoProperty;
            private SerializedProperty avProVideoProperty;
            private SerializedProperty unityRendererProperty;
            private SerializedProperty avProRendererProperty;
            private SerializedProperty audioSourcesProperty;

            private void OnEnable()
            {
                receiverProperty = serializedObject.FindProperty(nameof(VideoPlayerManager.receiver));
                unityVideoProperty = serializedObject.FindProperty(nameof(VideoPlayerManager.unityVideoPlayer));
                avProVideoProperty = serializedObject.FindProperty(nameof(VideoPlayerManager.unityVideoPlayer));
                unityRendererProperty = serializedObject.FindProperty(nameof(VideoPlayerManager.unityTextureRenderer));
                avProRendererProperty = serializedObject.FindProperty(nameof(VideoPlayerManager.unityTextureRenderer));
                audioSourcesProperty = serializedObject.FindProperty(nameof(VideoPlayerManager.audioSources));
            }

            public override void OnInspectorGUI()
            {
                if (UdonSharpGUI.DrawConvertToUdonBehaviourButton(target)) return;
                if (UdonSharpGUI.DrawProgramSource(target, false)) return;

                EditorGUILayout.HelpBox("Do not modify the video players on this game object, all modifications must be done on the USharpVideoPlayer. If you change the settings on these, you will break things.", MessageType.Warning);
                EditorGUILayout.PropertyField(receiverProperty);
                EditorGUILayout.PropertyField(unityVideoProperty);
                EditorGUILayout.PropertyField(avProVideoProperty);
                EditorGUILayout.PropertyField(unityRendererProperty);
                EditorGUILayout.PropertyField(avProRendererProperty);
                EditorGUILayout.PropertyField(audioSourcesProperty, true);

                serializedObject.ApplyModifiedProperties();
            }
        }
#endif
    }
}
