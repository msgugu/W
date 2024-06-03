using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterController : MonoBehaviour
{
    private Animator anim; // 애니메이터 컴포넌트
    public SkinnedMeshRenderer skinnedMeshRenderer;

    public float max_rotation_angle = 45.0f; // 머리 회전 최대 각도
    public float ear_max_threshold = 0.38f; // 눈 비율(EAR)의 최대 임계값
    public float ear_min_threshold = 0.30f; // 눈 비율(EAR)의 최소 임계값
    public bool isAutoBlinkActive; // 자동 눈 깜빡임 활성화 여부
    private MonoBehaviour autoBlinkScript; // AutoBlink 스크립트 참조

    [HideInInspector]
    public float eye_ratio_close = 85.0f; // 눈이 닫힌 상태의 비율
    [HideInInspector]
    public float eye_ratio_half_close = 20.0f; // 눈이 반쯤 닫힌 상태의 비율
    [HideInInspector]
    public float eye_ratio_open = 0.0f; // 눈이 열린 상태의 비율

    public float mar_max_threshold = 1.0f; // 입 비율(MAR)의 최대 임계값
    public float mar_min_threshold = 0.0f; // 입 비율(MAR)의 최소 임계값

    private Transform neck; // 목 트랜스폼 참조
    private Quaternion neck_quat; // 초기 목 회전 값

    private float roll = 0, pitch = 0, yaw = 0; // 머리 회전 값 변수
    private float ear_left = 0, ear_right = 0; // 눈 비율 값 변수
    private float eyebrow_left = 0, eyebrow_right = 0; // 눈썹 비율 값 변수
    private float mar = 0; // 입 비율 값 변수

    private int leftEyeIndex;
    private int rightEyeIndex;
    private int mouthIndex;
    private int leftEyebrowIndex;
    private int rightEyebrowIndex;

    void Start()
    {
        anim = GetComponent<Animator>(); // 애니메이터 컴포넌트 가져오기
        neck = anim.GetBoneTransform(HumanBodyBones.Neck); // 목 뼈 트랜스폼 가져오기
        neck_quat = Quaternion.Euler(0, 90, -90); // 초기 목 회전 값 설정
        autoBlinkScript = GetComponent("AutoBlink") as MonoBehaviour; // AutoBlink 스크립트 가져오기
        autoBlinkScript.enabled = isAutoBlinkActive; // 자동 눈 깜빡임 활성화 여부 설정
        SetEyes(eye_ratio_open, eye_ratio_open); // 눈 상태 초기화
        skinnedMeshRenderer = FindObjectOfType<SkinnedMeshRenderer>();

        // 블렌드 쉐이프 인덱스를 이름을 통해 찾기
        leftEyeIndex = skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex("leftEyeBlendShape");
        rightEyeIndex = skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex("rightEyeBlendShape");
        mouthIndex = skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex("mouthBlendShape");
        leftEyebrowIndex = skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex("leftEyebrowBlendShape");
        rightEyebrowIndex = skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex("rightEyebrowBlendShape");

        // UI 시스템 로드 및 초기화
        //GameObject.FindWithTag("GameController").GetComponent<UISystem>().LoadData();
        // GameObject.FindWithTag("GameController").GetComponent<UISystem>().InitUI();
    }

    // 수신된 TCP 메시지를 파싱하여 값 추출
    public void parseMessage(String message)
    {
        string[] res = message.Split(' ');

        if (res.Length < 12)
        {
            Debug.LogError("수신된 데이터 요소가 충분하지 않습니다.");
            return;
        }

        try
        {
            // 수신된 데이터를 파싱
            roll = float.Parse(res[0]);
            pitch = float.Parse(res[1]);
            yaw = float.Parse(res[2]);
            ear_left = float.Parse(res[3]);
            ear_right = float.Parse(res[4]);
            mar = float.Parse(res[9]);
            eyebrow_left = float.Parse(res[10]);
            eyebrow_right = float.Parse(res[11]);
        }
        catch (FormatException e)
        {
            Debug.LogError("수신된 데이터 파싱 중 오류 발생: " + e.ToString());
        }
    }

    void Update()
    {
        Debug.Log(string.Format("Roll: {0:F}; Pitch: {1:F}; Yaw: {2:F}; Eyebrow Left: {3:F}; Eyebrow Right: {4:F}",
                                 roll, pitch, yaw, eyebrow_left, eyebrow_right)); // 머리 회전 값 디버그 출력

        HeadRotation(); // 머리 회전 업데이트

        if (!isAutoBlinkActive)
            EyeBlinking(); // 자동 눈 깜빡임이 비활성화된 경우 눈 깜빡임 업데이트

        MouthMoving(); // 입 움직임 업데이트

        EyebrowMoving(); // 눈썹 움직임 업데이트
    }

    // 파싱된 값에 따라 머리 회전
    void HeadRotation()
    {
        float pitch_clamp = Mathf.Clamp(pitch, -max_rotation_angle, max_rotation_angle);
        float yaw_clamp = Mathf.Clamp(yaw, -max_rotation_angle, max_rotation_angle);
        float roll_clamp = Mathf.Clamp(roll, -max_rotation_angle, max_rotation_angle);

        neck.rotation = Quaternion.Euler(pitch_clamp, yaw_clamp, roll_clamp) * neck_quat;
    }

    // 눈 비율(EAR)에 따라 눈 깜빡임 제어
    void EyeBlinking()
    {
        float ear_left_clamped = Mathf.Clamp(ear_left, ear_min_threshold, ear_max_threshold);
        float ear_right_clamped = Mathf.Clamp(ear_right, ear_min_threshold, ear_max_threshold);
        float x_left = Mathf.Abs((ear_left_clamped - ear_min_threshold) / (ear_max_threshold - ear_min_threshold) - 1);
        float y_left = 90 * Mathf.Pow(x_left, 2) - 5 * x_left;
        float x_right = Mathf.Abs((ear_right_clamped - ear_min_threshold) / (ear_max_threshold - ear_min_threshold) - 1);
        float y_right = 90 * Mathf.Pow(x_right, 2) - 5 * x_right;
        SetEyes(y_left, y_right);
    }

    // 비율에 따라 눈 블렌드 쉐이프 웨이트 설정
    void SetEyes(float left_ratio, float right_ratio)
    {
        if (leftEyeIndex >= 0)
            skinnedMeshRenderer.SetBlendShapeWeight(leftEyeIndex, left_ratio);  // 왼쪽 눈
        if (rightEyeIndex >= 0)
            skinnedMeshRenderer.SetBlendShapeWeight(rightEyeIndex, right_ratio); // 오른쪽 눈
    }

    // 자동 눈 깜빡임 기능 활성화 또는 비활성화
    public void EnableAutoBlink(bool enabled)
    {
        autoBlinkScript.enabled = enabled;
        isAutoBlinkActive = enabled;
    }

    // 입 비율(MAR)에 따라 입 움직임 제어
    void MouthMoving()
    {
        float mar_clamped = Mathf.Clamp(mar, mar_min_threshold, mar_max_threshold);
        float ratio = (mar_clamped - mar_min_threshold) / (mar_max_threshold - mar_min_threshold);
        ratio = ratio * 100 / (mar_max_threshold - mar_min_threshold);
        SetMouth(ratio);
    }

    // 비율에 따라 입 블렌드 쉐이프 웨이트 설정
    void SetMouth(float ratio)
    {
        if (mouthIndex >= 0)
            skinnedMeshRenderer.SetBlendShapeWeight(mouthIndex, ratio);
    }

    #region 눈썹
    // 눈썹 비율에 따라 눈썹 움직임 제어
    void EyebrowMoving()
    {
        float eyebrow_left_clamped = Mathf.Clamp(eyebrow_left, 0.0f, 1.0f);
        float eyebrow_right_clamped = Mathf.Clamp(eyebrow_right, 0.0f, 1.0f);
        SetEyebrows(eyebrow_left_clamped * 100, eyebrow_right_clamped * 100);
    }

    // 비율에 따라 눈썹 블렌드 쉐이프 웨이트 설정
    void SetEyebrows(float left_ratio, float right_ratio)
    {
        if (leftEyebrowIndex >= 0)
            skinnedMeshRenderer.SetBlendShapeWeight(leftEyebrowIndex, left_ratio); // 왼쪽 눈썹
        if (rightEyebrowIndex >= 0)
            skinnedMeshRenderer.SetBlendShapeWeight(rightEyebrowIndex, right_ratio); // 오른쪽 눈썹
    }
    #endregion

    // 현재 설정을 저장 데이터에 저장
    public void PopulateSaveData(CharacterPref characterPref)
    {
        characterPref.max_rotation_angle = max_rotation_angle;
        characterPref.ear_max_threshold = ear_max_threshold;
        characterPref.ear_min_threshold = ear_min_threshold;
        characterPref.isAutoBlinkActive = isAutoBlinkActive;
        characterPref.mar_max_threshold = mar_max_threshold;
        characterPref.mar_min_threshold = mar_min_threshold;
    }

    // 저장된 데이터에서 설정 로드
    public void LoadFromSaveData(CharacterPref characterPref)
    {
        max_rotation_angle = characterPref.max_rotation_angle;
        ear_max_threshold = characterPref.ear_max_threshold;
        ear_min_threshold = characterPref.ear_min_threshold;
        isAutoBlinkActive = characterPref.isAutoBlinkActive;
        mar_max_threshold = characterPref.mar_max_threshold;
        mar_min_threshold = characterPref.mar_min_threshold;
    }
}
