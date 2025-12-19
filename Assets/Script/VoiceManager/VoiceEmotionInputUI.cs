using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class VoiceEmotionInputUI : MonoBehaviour
{
    enum VoiceUIState
    {
        Idle,
        Listening,
        Processing
    }

    [Header("UI")]
    public GameObject root;
    public TMP_Text dateText;
    public TMP_Text statusText;
    public TMP_Text resultText;
    public Image statusIcon;  // 마이크 버튼 이미지로 수정

    [Header("Sprites")]
    public Sprite idleSprite;
    public Sprite listeningSprite;
    public Sprite processingSprite;

    [Header("Buttons")]
    public Button startVoiceButton;
    public Button closeButton;

    [Header("Voice")]
  //  public VoiceTest voiceTest;

    private DateTime currentDate;
   private VoiceUIState currentState = VoiceUIState.Idle;

    //  외부(CalenderUI)에서 결과 받기용 콜백
    private Action<DateTime, string> onResultCallback;

    void Awake()
    {
        if (root == null)
            root = gameObject;

        root.SetActive(false);

        startVoiceButton.onClick.AddListener(OnClickStartVoice);
        closeButton.onClick.AddListener(Close);
    }

    void OnEnable()
    {
        VoiceTest.OnSpeechResult += OnReceiveSpeechResult;
    }

    void OnDisable()
    {
        VoiceTest.OnSpeechResult -= OnReceiveSpeechResult;
    }

    // ?? CalenderUI에서 호출
    public void Open(DateTime date, Action<DateTime, string> onResult)
    {
        currentDate = date;
        onResultCallback = onResult;

        root.SetActive(true);

        dateText.text = date.ToString("yyyy.MM.dd" + "의 소비는 어떠셨나요?");
        resultText.text = "";

        SetState(VoiceUIState.Idle);
    }

    public void Close()
    {
        root.SetActive(false);
        SetState(VoiceUIState.Idle);
    }

    // ---------------- 상태 제어 ----------------

    private void SetState(VoiceUIState state)
    {
        currentState = state;

        switch (state)
        {
            case VoiceUIState.Idle:
                statusText.text = "소비 감정을 말씀해주세요! ";
                statusIcon.sprite = idleSprite;
                startVoiceButton.interactable = true;
                break;

            case VoiceUIState.Listening:
                statusText.text = "음성을 입력하고 있습니다…";
                statusIcon.sprite = listeningSprite;
                startVoiceButton.interactable = false;
                break;

            case VoiceUIState.Processing:
                statusText.text = "음성을 텍스트로 변환 중입니다…";
                statusIcon.sprite = processingSprite;
                startVoiceButton.interactable = false;
                break;
        }
    }

    // ---------------- 버튼 ----------------

    private void OnClickStartVoice()
    {
        if (!MicPermissionUtil.EnsureMicPermission())
            return;

        SetState(VoiceUIState.Listening);

     //   voiceTest.StartVoice();
    }

    // ---------------- 음성 결과 수신 ----------------

    private void OnReceiveSpeechResult(string text)
    {
        if (currentState != VoiceUIState.Listening)
            return;

        SetState(VoiceUIState.Processing);

        resultText.text = text;

        //  CalenderUI로 전달
        onResultCallback?.Invoke(currentDate, text);

        // 처리 끝났으므로 UI 닫기
        Close();
    }
}

