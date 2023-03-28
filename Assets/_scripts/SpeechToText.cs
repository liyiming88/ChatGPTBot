using UnityEngine;
using UnityEngine.UI;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using TMPro;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace OpenAI
{
    public class SpeechToText : MonoBehaviour
    {
        public GameObject thinkDots;
        private Animator thinkAnimator;
        private static SpeechToText speechy;

        private Animator animator;
        public static SpeechToText Speechy
        {
            get { return speechy; }
        }
        public TMP_Text outputText;
        // PULLED OUT OF BUTTON CLICK
        SpeechRecognizer recognizer;
        SpeechConfig config;
        SpeechSynthesizer synthesizer;
        // if the whole message has been transcribed over
        bool isMessageOver = false;

        private object threadLocker = new object();
        private string message;
        private bool ifAvatarTalking;
        private bool ifAvatarListening;
        private bool ifAvatarThinking;
        private bool isInit = true;

        // 用来识别整句输出，并且输出完整文本
        private void RecognizedHandler(object sender, SpeechRecognitionEventArgs e)
        {
            Debug.Log("Start to speech to text");
            lock (threadLocker)
            {
                switch (e.Result.Reason)
                {
                    case ResultReason.RecognizedSpeech:
                        Debug.Log($"RECOGNIZED: Text={e.Result.Text}");
                        break;
                    case ResultReason.NoMatch:
                        Debug.Log($"NOMATCH: Speech could not be recognized.");
                        break;
                    case ResultReason.Canceled:
                        var cancellation = CancellationDetails.FromResult(e.Result);
                        Debug.Log($"CANCELED: Reason={cancellation.Reason}");

                        if (cancellation.Reason == CancellationReason.Error)
                        {
                            Debug.Log($"CANCELED: ErrorCode={cancellation.ErrorCode}");
                            Debug.Log($"CANCELED: ErrorDetails={cancellation.ErrorDetails}");
                            Debug.Log($"CANCELED: Did you set the speech resource key and region values?");
                        }
                        break;
                }
                message = e.Result.Text;
                isMessageOver = true;
                
            }
        }
        
        private void StartToThink(object sender, SpeechRecognitionEventArgs e)
        {
               if (isInit) {
                    isInit = false;
                    Debug.Log("Start to think");
                    ifAvatarThinking = true;
                }
        }

        // 开始录音
        public async void OpenMic()
        {
            Debug.Log("Start recording");
            using var audioConfig = AudioConfig.FromDefaultMicrophoneInput();
            await recognizer.StartContinuousRecognitionAsync().ConfigureAwait(false); // this will start the listening when you click the button, if it's already off
            lock (threadLocker)
            {
                Debug.Log("Start recording");
            }
        }

        public async void KillRecord()
        {
            Debug.Log("Kill record");
            await recognizer.StopContinuousRecognitionAsync().ConfigureAwait(false);
            Debug.Log("Kill Avatar speaking");
            await synthesizer.StopSpeakingAsync(); 
        }

        // 文字转语音
        // avatar is talking
        public async void SynthesizeAudioAsync(string text)
        {
            Debug.Log("Start text to speech, and avatar starts to speak");
            await synthesizer.SpeakTextAsync(text);
        }

        // stop recording
        private async void StopRecord(object sender, SpeechSynthesisEventArgs e)
        {
            Debug.Log("Stop recording");
            await recognizer.StopContinuousRecognitionAsync().ConfigureAwait(false); // this will start the listening when you click the button, if it's already off
            lock (threadLocker)
            {
                ifAvatarTalking = true;
                ifAvatarListening = false;
                ifAvatarThinking = false;
                isInit = true;
            }
            Debug.Log("recording stops");
        }

        // player is talking
        private async void RestartRecord(object sender, SpeechSynthesisEventArgs e)
        {
            
            Debug.Log("Avatar stops talking and recording starts");
            await recognizer.StartContinuousRecognitionAsync().ConfigureAwait(false); // this will start the listening when you click the button, if it's already off
            lock (threadLocker)
            {
                ifAvatarTalking = false;
                ifAvatarListening = true;
                ifAvatarThinking = false;
            }
        }


        void Start()
        {
            // 认证Azure speech sdk的权限
            config = SpeechConfig.FromSubscription("e512bf00442d427e9158b0e381563240", "eastus");
            // 这两项是配置文字转语音的语言和人物
            config.SpeechSynthesisLanguage = "en-US";
            config.SpeechSynthesisVoiceName = "en-US-AriaNeural";
            // 新建合成语音转换器
            synthesizer = new SpeechSynthesizer(config);
            synthesizer.SynthesisStarted += StopRecord;
            synthesizer.SynthesisCompleted += RestartRecord;
            synthesizer.SynthesisCanceled += RestartRecord;
            // 新建语音识别器
            recognizer = new SpeechRecognizer(config);
            // 订阅事件：当用户语音完整输出后，调用Handler
            recognizer.Recognized += RecognizedHandler;
            recognizer.Recognizing += StartToThink;
            animator = gameObject.GetComponent<Animator>();
            thinkAnimator = thinkDots.GetComponent<Animator>();
            Debug.Log("Speech sdk inited");
            string[] aaa = Microphone.devices;
        }

        void Update()
        {

            lock (threadLocker)
            {
                if (outputText != null)
                {
                    outputText.text = message;
                }
            }

            if (isMessageOver)
            {
                isMessageOver = false;
                ChatGPTSTT.Chatty.CallChatGPT(message);
            }

            if (ifAvatarTalking)
            {
                ifAvatarTalking = false;
                animator.SetTrigger("talk");
                thinkAnimator.SetBool("isThink", false);

            }
            if (ifAvatarListening)
            {
                ifAvatarListening = false;
                animator.SetTrigger("listen");
                thinkAnimator.SetBool("isThink", false);
            }
            if (ifAvatarThinking)
            {
                ifAvatarThinking = false;
                thinkAnimator.SetBool("isThink", true);
            }
        }

        void Awake()
        {
            if (speechy != null && speechy != this)
            {
                Destroy(gameObject);
                return;
            }
            else
            {
                speechy = this;
                DontDestroyOnLoad(gameObject);
            }
        }


        void OnDestroy()
        {
            Debug.Log("");
        }
    }
}
